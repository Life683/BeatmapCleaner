using System;
using System.Collections.Generic;
using BeatmapExporterCore.Exporters.Lazer.LazerDB;
using BeatmapExporterCore.Utilities;
using BeatmapExporterGUI.Exporter;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Realms;
using BeatmapExporterCore.Exporters.Lazer;

namespace BeatmapExporterGUI.ViewModels;

/// <summary>
/// The top menu row/bar providing user access to all functions of the program.
/// </summary>
public partial class MenuRowViewModel : ViewModelBase
{
    private readonly OuterViewModel outer;

    public MenuRowViewModel(OuterViewModel outer)
    {
        this.outer = outer;
    }

    /// <summary>
    /// If an osu! database is currently loaded into the application.
    /// </summary>
    private bool DatabaseLoaded => Exporter.Lazer != null;

    /// <summary>
    /// If user navigation around the program should be allowed.
    /// </summary>
    private bool CanNavigate => !outer.IsCleaning;

    /// <summary>
    /// If the user should be able to unload the osu! database or start a clean.
    /// </summary>
    private bool CanExport => DatabaseLoaded && CanNavigate;

    /// <summary>
    /// User-requested action to exit the entire BeatmapExporter program.
    /// </summary>
    [RelayCommand]
    private void Exit() => ExporterApp.Exit();

    /// <summary>
    /// User-requested action to unload the currently loaded osu! database.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExport))]
    private void Close()
    {
        Exporter.Unload();
        outer.Home.SetNotLoaded();
        outer.NavigateHome();
    }

    /// <summary>
    /// User-requested action to clean media files from the loaded beatmaps.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExport))]
    public async Task Clean()
    {
        List<Guid> mapsetIdsToClean = Exporter.Lazer!.SelectedBeatmapSetIds;
        Exporter.AddSystemMessage($"Cleaning {mapsetIdsToClean.Count} beatmap set(s)...");

        if (mapsetIdsToClean.Count == 0)
        {
            Exporter.AddSystemMessage("No beatmap sets selected for cleaning.", error: true);
            return;
        }

        outer.IsCleaning = true;
        NotifyNavigationCommands();
        try
        {
            await Task.Run(() =>
            {
                Exporter.Lazer!.CleanBeatmapsById(mapsetIdsToClean);
            });
        }
        finally
        {
            outer.IsCleaning = false;
            NotifyNavigationCommands();
        }

        Exporter.AddSystemMessage("Clean media files complete.");
    }

    /// <summary>
    /// Manually refreshes CanExecute state for commands gated on IsCleaning/CanNavigate,
    /// since IsCleaning lives on OuterViewModel and won't auto-notify these RelayCommands.
    /// </summary>
    private void NotifyNavigationCommands()
    {
        CloseCommand.NotifyCanExecuteChanged();
        CleanCommand.NotifyCanExecuteChanged();
        BeatmapsCommand.NotifyCanExecuteChanged();
        CollectionsCommand.NotifyCanExecuteChanged();
        ConfigurationCommand.NotifyCanExecuteChanged();
        HomeCommand.NotifyCanExecuteChanged();
    }

    // The below commands are user-requested navigation to specific program pages/functionality.

    [RelayCommand(CanExecute = nameof(CanNavigate))]
    private void Home() => outer.NavigateHome();

    [RelayCommand(CanExecute = nameof(CanExport))]
    private void Beatmaps() => outer.ListBeatmaps();

    [RelayCommand(CanExecute = nameof(CanExport))]
    private void Collections() => outer.ListCollections();

    [RelayCommand(CanExecute = nameof(CanExport))]
    private void Configuration() => outer.EditFilters();

    // The below properties are references to the relevant BeatmapExporter version numbers

    public string ProgramVersion => ExporterUpdater.FeatureVersion;

    public string DatabaseVersion => LazerDatabase.LazerSchemaVersion.ToString();

    public string LazerVersion => LazerDatabase.FirstLazerVersion;

    // The below commands are all web links available for the user to open in browser.

    public void GitHub() => PlatformUtil.Open(ExporterUpdater.Project);

    public void Releases() => PlatformUtil.Open(ExporterUpdater.Releases);

    public void Osu() => PlatformUtil.Open("https://github.com/ppy/osu/releases");

    /// <summary>
    /// User-requested action to view the BeatmapExporter application data
    /// </summary>
    public void ApplicationData() => PlatformUtil.Open(ClientSettings.APPDIR);

    /// <summary>
    /// User-requested action to reset the program preferences to defaults
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanNavigate))]
    private void Reset()
    {
        var defaults = new ClientSettings();
        defaults.TrySave();

        Exporter.AddSystemMessage("BeatmapExporter settings/filters have been reset to defaults and the database has been unloaded.");

        if (DatabaseLoaded)
        {
            Close();
        }
    }
}