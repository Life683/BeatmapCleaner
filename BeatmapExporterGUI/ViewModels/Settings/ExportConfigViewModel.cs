using BeatmapExporterCore.Exporters;
using BeatmapExporterCore.Exporters.Lazer;
using BeatmapExporterCore.Filters;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BeatmapExporterGUI.ViewModels.Settings
{
    /// <summary>
    /// Page allowing users to configure which beatmaps are selected for cleaning, via filters.
    /// </summary>
    public partial class ExportConfigViewModel : ViewModelBase
    {
        private readonly OuterViewModel outerViewModel;

        public ExportConfigViewModel(OuterViewModel outer)
        {
            outerViewModel = outer;
            BeatmapFilters = new List<string>();
            Task.Run(() => UpdateBeatmapFilters());
            SelectedFilterIndex = -1;
        }

        public LazerCleaner Lazer => Exporter.Lazer!;

        protected ExporterConfiguration Config => Exporter.Configuration!;

        #region Beatmap Filters
        [ObservableProperty]
        private IEnumerable<string> _BeatmapFilters;

        private async Task UpdateBeatmapFilters()
        {
            await Exporter.RealmScheduler.Schedule(() =>
            {
                BeatmapFilters = Exporter.Lazer!.Filters()
                    .Select(filter => $"+ {filter.Description} ({filter.DiffCount} beatmaps)")
                    .ToList();

                Exporter.Lazer.UpdateSelectedBeatmaps();
            });

            OnPropertyChanged(nameof(ShouldDisplayFilterMode));
            OnPropertyChanged(nameof(SelectionSummary));
            ResetFiltersCommand.NotifyCanExecuteChanged();
            RemoveSelectedFilterCommand.NotifyCanExecuteChanged();
        }

        public string SelectionSummary => $"Beatmap sets selected: {Lazer.SelectedBeatmapSetCount}/{Lazer.TotalBeatmapSetCount}\n\nBeatmap diffs selected: {Lazer.SelectedBeatmapCount}/{Lazer.TotalBeatmapCount}";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RemoveSelectedFilterCommand))]
        private int _SelectedFilterIndex;

        public bool IsFilterSelected => SelectedFilterIndex != -1;

        public bool IsResettable => Config.Filters.Count > 0;

        [RelayCommand(CanExecute = nameof(IsFilterSelected))]
        private async Task RemoveSelectedFilter()
        {
            Config.Filters.RemoveAt(SelectedFilterIndex);
            await UpdateBeatmapFilters();
        }

        [RelayCommand(CanExecute = nameof(IsResettable))]
        private async Task ResetFilters()
        {
            Config.Filters.Clear();
            await UpdateBeatmapFilters();
        }

        [RelayCommand]
        private void ListBeatmaps() => outerViewModel.ListBeatmaps();

        [ObservableProperty]
        private NewFilterViewModel? _CurrentFilterCreationControl;

        public void CreateFilterBuilder() => CurrentFilterCreationControl = new(this);

        public void CancelFilterBuilder() => CurrentFilterCreationControl = null;

        public async Task ApplyFilterBuilder(BeatmapFilter filter)
        {
            Config.Filters.Add(filter);
            await UpdateBeatmapFilters();
            CancelFilterBuilder();
        }

        public bool CombineFilterMode
        {
            get => Config.CombineFilterMode;
            set
            {
                Config.CombineFilterMode = value;
                Task.Run(() => UpdateBeatmapFilters());
            }
        }

        public bool ShouldDisplayFilterMode => Config.Filters.Count > 1;
        #endregion

        /// <summary>
        /// Reference to the Clean command, allowing this page to trigger cleaning of the currently filtered/selected beatmap sets.
        /// </summary>
        public ICommand CleanCommand => outerViewModel.MenuRow.CleanCommand;
    }
}