using BeatmapExporterCore.Filters;
using BeatmapExporterCore.Utilities;
using System.Diagnostics.CodeAnalysis;

namespace BeatmapExporterCore.Exporters
{
    public class ExporterConfiguration
    {
        private ClientSettings settings;
        private bool combineFilterMode;

        public ExporterConfiguration(ClientSettings settings)
        {
            ApplySettings(settings);
        }

        /// <summary>
        /// Applies the settings from a <see cref="ClientSettings" /> profile to this exporter configuration.
        /// </summary>
        [MemberNotNull(nameof(settings), nameof(Filters))]
        public void ApplySettings(ClientSettings settings)
        {
            this.settings = settings;
            combineFilterMode = settings.MatchAllFilters;

            Filters = settings.AppliedFilters
                .Select(f => f.ToBeatmapFilter())
                .Where(f => f != null)
                .Select(f => f!)
                .ToList();
        }

        /// <summary>
        /// The beatmap filters currently applied to this exporter.
        /// </summary>
        public List<BeatmapFilter> Filters { get; set; }

        /// <summary>
        /// Notify the user settings container to update the currently persisted filters.
        /// </summary>
        public void SaveFilters() => settings.SaveFilters([.. Filters]);

        /// <summary>
        /// If filters should be applied with AND logic where beatmaps must match all filters.
        /// </summary>
        public bool CombineFilterMode
        {
            get => combineFilterMode;
            set
            {
                combineFilterMode = value;
                settings.SaveFilterMode(value);
            }
        }
    }
}