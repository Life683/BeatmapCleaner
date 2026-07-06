using BeatmapExporterCore.Exporters;
using BeatmapExporterCore.Exporters.Lazer;
using System.Linq;

namespace BeatmapExporterGUI.ViewModels.HomePage
{
    /// <summary>
    /// Page displayed when a lazer database is loaded, displaying basic database stats.
    /// </summary>
    public class LoadedViewModel : ViewModelBase
    {
        public LoadedViewModel()
        {
        }

        /// <summary>
        /// The LazerCleaner instance currently loaded.
        /// </summary>
        public LazerCleaner Lazer => Exporter.Lazer!;

        /// <summary>
        /// Reference to the filters currently applied to this LazerCleaner.
        /// </summary>
        public int Filters => Lazer.Configuration.Filters.Count();

        /// <summary>
        /// Count of beatmap sets currently selected for cleaning.
        /// </summary>
        public int SelectedSets => Lazer.SelectedBeatmapSetCount;
    }
}