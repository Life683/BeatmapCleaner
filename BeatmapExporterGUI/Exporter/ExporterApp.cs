using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using BeatmapExporterCore.Exporters;
using BeatmapExporterCore.Exporters.Lazer;
using BeatmapExporterCore.Exporters.Lazer.LazerDB;
using BeatmapExporterCore.Exporters.Lazer.LazerDB.Schema;
using BeatmapExporterCore.Utilities;
using BeatmapExporterGUI.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using Realms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;

namespace BeatmapExporterGUI.Exporter
{
    public class ExporterApp : ObservableObject
    {
        public ExporterApp()
        {
            var cts = new CancellationTokenSource();
            RealmScheduler = new RealmTaskScheduler(cts.Token);
            RealmScheduler.Start();

            SystemMessages = new();
        }

        public static void Exit()
        {
            if (Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        }

        public static void OpenLatestRelease() => PlatformUtil.Open(ExporterUpdater.Latest);

        public bool LoadDatabase(string? userDir)
        {
            ClientSettings settings;
            try
            {
                settings = ClientSettings.LoadFromFile();
            }
            catch (Exception e)
            {
                AddSystemMessage($"Unable to load application settings: {e.Message}", error: true);
                settings = new();
            }

            List<string?> userDirs = [userDir, settings.DatabasePath];
            var checkDirs = userDirs.Concat(LazerDatabase.GetDefaultDirectories());
            AddSystemMessage("Now checking known osu!lazer storage locations.");

            string? dbFile = null;
            foreach (var dir in checkDirs)
            {
                if (dir is null) continue;
                AddSystemMessage($"Checking directory: {dir}");
                dbFile = LazerDatabase.GetDatabaseFile(dir);
                if (dbFile is null)
                {
                    AddSystemMessage($"osu! song database not found at {dir}.", error: true);
                }
                else
                {
                    break;
                }
            }

            if (dbFile is null)
            {
                AddSystemMessage("osu! song database not found. Please find and provide your osu!lazer data folder.", error: true);
                AddSystemMessage("The folder should contain a \"client.realm\" file and can be opened from in-game to locate it.");
                return false;
            }

            var database = new LazerDatabase(dbFile);
            Realm? realm;
            try
            {
                realm = database!.Open();
                if (realm is null)
                    throw new IOException("Unable to open osu! database.");
            }
            catch (Exception e)
            {
                AddSystemMessage($"Error opening database: {e.Message}", error: true);
                if (e is LazerVersionException version)
                {
                    foreach (var message in version.Details)
                    {
                        AddSystemMessage(message, error: true);
                    }
                }
                else
                {
                    AddSystemMessage("This is an abnormal error, and you may need to open a GitHub issue for further assistance.", error: true);
                }
                return false;
            }

            AddSystemMessage($"Opened osu! database: {dbFile}");
            AddSystemMessage("Loading database...");
            settings.SaveDatabase(dbFile);

            List<BeatmapSet> beatmaps = realm!.All<BeatmapSet>().ToList();
            List<BeatmapCollection> collections = realm.All<BeatmapCollection>().ToList();

            Lazer = new(database, settings, beatmaps, collections);
            AddSystemMessage($"Load complete. Found {beatmaps.Count} beatmaps, {collections.Count} collections.");
            return true;
        }

        public void Unload() => Lazer = null;

        public LazerCleaner? Lazer { get; private set; }

        public ExporterConfiguration? Configuration => Lazer?.Configuration;

        public RealmTaskScheduler RealmScheduler { get; }

        public record struct Message(bool IsError, string Content, DateTime Timestamp)
        {
            public override readonly string? ToString() => $"{(IsError ? "(!)" : "")}{Timestamp:HH:mm} - {Content}";
        }

        public ObservableCollection<Message> SystemMessages { get; }

        public void AddSystemMessage(string message, bool error = false) => SystemMessages.Add(new(error, message, DateTime.Now));
    }
}