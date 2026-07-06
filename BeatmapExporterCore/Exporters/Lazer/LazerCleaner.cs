using BeatmapExporterCore.Exporters.Lazer.LazerDB;
using BeatmapExporterCore.Exporters.Lazer.LazerDB.Schema;
using BeatmapExporterCore.Exporters.Stable.Collections;
using BeatmapExporterCore.Filters;
using BeatmapExporterCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BeatmapExporterCore.Exporters.Lazer
{
    /// <summary>
    /// Handles loading osu!lazer beatmap data, applying user filters to select beatmap sets,
    /// and cleaning (stripping media from) the selected sets.
    /// </summary>
    public class LazerCleaner
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        readonly LazerDatabase lazerDb;
        readonly List<Beatmap> allBeatmapDiffs;

        /// <param name="lazerDb">The lazer database, referenced for opening files later.</param>
        /// <param name="settings">The user's last known <see cref="ClientSettings"/></param>
        /// <param name="beatmapSets">All beatmap sets loaded into memory.</param>
        /// <param name="lazerCollections">All collections loaded into memory.</param>
        public LazerCleaner(LazerDatabase lazerDb, ClientSettings settings, List<BeatmapSet> beatmapSets, List<BeatmapCollection> lazerCollections)
        {
            this.lazerDb = lazerDb;

            AllBeatmapSets = beatmapSets
                .Where(set => set.Beatmaps.Count > 0)
                .OrderBy(set => set.OnlineID)
                .ToList();
            SelectedBeatmapSets = AllBeatmapSets;
            TotalBeatmapSetCount = AllBeatmapSets.Count;
            SelectedBeatmapSetCount = TotalBeatmapSetCount;
            SelectedBeatmapSetIds = AllBeatmapSets.Select(s => s.ID).ToList();

            allBeatmapDiffs = AllBeatmapSets.SelectMany(s => s.Beatmaps).ToList();

            TotalBeatmapCount = allBeatmapDiffs.Count;
            SelectedBeatmapCount = TotalBeatmapCount;

            var colCount = 0;
            Collections = new();
            foreach (var coll in lazerCollections)
            {
                colCount++;
                var colMaps = allBeatmapDiffs
                    .Where(b => coll.BeatmapMD5Hashes.Contains(b.MD5Hash))
                    .ToList();
                Collections[coll.Name] = new MapCollection(colCount, colMaps);
            }
            CollectionCount = colCount;

            Configuration = new ExporterConfiguration(settings);
            if (settings.AppliedFilters.Count > 0)
            {
                UpdateSelectedBeatmaps();
            }
        }

        /// <summary>
        /// Count of the total beatmap sets discovered.
        /// </summary>
        public int TotalBeatmapSetCount { get; }

        /// <summary>
        /// Count of the individual beatmap difficulties discovered.
        /// </summary>
        public int TotalBeatmapCount { get; }

        /// <summary>
        /// All beatmap sets, without any filtering.
        /// </summary>
        public List<BeatmapSet> AllBeatmapSets { get; }

        /// <summary>
        /// The beatmap sets currently selected, after filters are applied.
        /// </summary>
        public List<BeatmapSet> SelectedBeatmapSets { get; private set; }

        public List<Guid> SelectedBeatmapSetIds { get; private set; } = new();

        /// <summary>
        /// Count of the individual beatmap difficulties currently selected, after filters are applied.
        /// </summary>
        public int SelectedBeatmapCount { get; private set; }

        /// <summary>
        /// Count of the beatmap sets currently selected, after filters are applied.
        /// </summary>
        public int SelectedBeatmapSetCount { get; private set; }

        /// <summary>
        /// All discovered collections. The dictionary key represents the collection's name as chosen by the user.
        /// </summary>
        public Dictionary<string, MapCollection> Collections { get; }

        /// <summary>
        /// Count of all collections discovered
        /// </summary>
        public int CollectionCount { get; }

        /// <summary>
        /// The (mutable) configuration, containing beatmap filter rules used to select sets for cleaning.
        /// </summary>
        public ExporterConfiguration Configuration { get; }

        public record struct FilterDetail(int Id, string Description, int DiffCount);

        /// <summary>
        /// Returns a list of 'FilterDetail' containers with information about applied filters
        /// </summary>
        public IEnumerable<FilterDetail> Filters() => Configuration.Filters.Select((filter, i) =>
        {
            int diffCount = allBeatmapDiffs.Count(diff => filter.Includes(diff));
            return new FilterDetail(i + 1, filter.Description, diffCount);
        });

        private readonly Regex idCollection = new("#([0-9]+)", RegexOptions.Compiled);

        /// <summary>
        /// Update the set of 'selected' beatmaps by applying all filters from this Configuration.Filters.
        /// </summary>
        /// <param name="collectionFailure">An optional callback to notify users on a collection filter mismatch.</param>
        public void UpdateSelectedBeatmaps(Action<string>? collectionFailure = null)
        {
            List<string> collFilters = new();
            List<BeatmapFilter> beatmapFilters = new();
            bool negateColl = false;
            foreach (var filter in Configuration.Filters)
            {
                if (filter.Collections is not null)
                {
                    List<string> filteredCollections = new();
                    foreach (var requestedFilter in filter.Collections)
                    {
                        string? targetCollection = null;
                        var match = idCollection.Match(requestedFilter);
                        if (match.Success)
                        {
                            var collectionId = int.Parse(match.Groups[1].Value);
                            targetCollection = Collections.FirstOrDefault(c => c.Value.CollectionID == collectionId).Key;
                        }
                        else
                        {
                            var exists = Collections.ContainsKey(requestedFilter);
                            if (exists)
                            {
                                targetCollection = requestedFilter;
                            }
                        }
                        if (targetCollection != null)
                        {
                            filteredCollections.Add(targetCollection);
                        }
                        else
                        {
                            collectionFailure?.Invoke(requestedFilter);
                        }
                    }
                    collFilters.AddRange(filteredCollections);
                    negateColl = filter.Negated;
                }
                else
                    beatmapFilters.Add(filter);
            }

            if (collFilters.Count > 0)
            {
                var includedHashes = Collections
                    .Where(c => collFilters.Any(c => c == "-all") switch
                    {
                        true => true,
                        false => collFilters.Any(filter => string.Equals(filter, c.Key, StringComparison.OrdinalIgnoreCase))
                    })
                    .SelectMany(c => c.Value.Beatmaps.Select(b => b.ID))
                    .ToList();

                string desc = string.Join(", ", collFilters);
                BeatmapFilter collFilter = new(desc, negateColl,
                    b => includedHashes.Contains(b.ID),
                    FilterTemplate.Collections);

                beatmapFilters.Add(collFilter);
            }

            Configuration.Filters = new(beatmapFilters);
            Configuration.SaveFilters();

            int selectedCount = 0;
            int selectedSetCount = 0;
            List<BeatmapSet> selectedSets = new();
            foreach (var set in AllBeatmapSets)
            {
                var filteredMaps = set.Beatmaps
                    .Where(map =>
                    {
                        if (Configuration.CombineFilterMode)
                        {
                            return Configuration.Filters.All(f => f.Includes(map));
                        }
                        else
                        {
                            return Configuration.Filters.Any(f => f.Includes(map));
                        }
                    });

                var selected = filteredMaps.ToList();

                set.SelectedBeatmaps = selected;
                selectedCount += selected.Count;

                if (selected.Count > 0)
                {
                    selectedSetCount++;
                    selectedSets.Add(set);
                }
            }

            SelectedBeatmapSets = selectedSets;
            SelectedBeatmapSetCount = selectedSetCount;
            SelectedBeatmapCount = selectedCount;
            SelectedBeatmapSetIds = selectedSets.Select(s => s.ID).ToList();
        }

        /// <summary>
        /// Processes beatmap cleaning on a background thread using a localized database instance.
        /// </summary>
        public void CleanBeatmapsById(List<Guid> mapsetIds)
        {
            using var threadSafeRealm = lazerDb.Open();

            string pixelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "pixel.png");
            string[] imageVideoExts = { ".jpg", ".jpeg", ".png", ".avi", ".mp4", ".flv", ".wmv" };
            string[] audioExts = { ".mp3", ".ogg", ".wav" };

            int cleanedSets = 0;
            int cleanedFiles = 0;

            foreach (var id in mapsetIds)
            {
                var mapset = threadSafeRealm.Find<BeatmapSet>(id);
                if (mapset == null)
                {
                    Logger.Warn($"Clean: mapset {id} not found in Realm.");
                    continue;
                }

                HashSet<string> protectedAudio = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var fileUsage in mapset.NamedFiles.Where(f => f.Filename.EndsWith(".osu", StringComparison.OrdinalIgnoreCase)))
                {
                    using var stream = lazerDb.OpenHashedFile(fileUsage.File.Hash);
                    using var reader = new StreamReader(stream);
                    while (reader.ReadLine() is { } line)
                    {
                        if (line.StartsWith("AudioFilename:", StringComparison.OrdinalIgnoreCase))
                        {
                            var parts = line.Split(':', 2);
                            if (parts.Length > 1) protectedAudio.Add(parts[1].Trim());
                            break;
                        }
                    }
                }

                List<RealmNamedFileUsage> itemsToUnlinkFromDb = new List<RealmNamedFileUsage>();

                foreach (var fileUsage in mapset.NamedFiles.ToList())
                {
                    string filename = fileUsage.Filename;
                    string ext = Path.GetExtension(filename).ToLowerInvariant();

                    if (protectedAudio.Contains(filename)) continue;

                    if (imageVideoExts.Contains(ext))
                    {
                        if (File.Exists(pixelPath))
                        {
                            string physicalPath = lazerDb.HashedFilePath(fileUsage.File.Hash);
                            File.Copy(pixelPath, physicalPath, true);
                        }
                        else
                        {
                            Logger.Warn($"Clean: pixel.png not found at {pixelPath}, skipping image replacement for {filename}.");
                        }
                    }
                    else if (audioExts.Contains(ext))
                    {
                        string physicalPath = lazerDb.HashedFilePath(fileUsage.File.Hash);
                        if (File.Exists(physicalPath))
                        {
                            File.WriteAllBytes(physicalPath, Array.Empty<byte>());
                        }
                        itemsToUnlinkFromDb.Add(fileUsage);
                    }
                }

                if (itemsToUnlinkFromDb.Count > 0)
                {
                    threadSafeRealm.Write(() =>
                    {
                        foreach (var fileUsage in itemsToUnlinkFromDb)
                        {
                            mapset.NamedFiles.Remove(fileUsage);
                        }
                    });
                    cleanedSets++;
                    cleanedFiles += itemsToUnlinkFromDb.Count;
                }
            }

            Logger.Info($"Clean complete: {cleanedSets}/{mapsetIds.Count} mapsets modified, {cleanedFiles} audio files unlinked.");
        }
    }
}