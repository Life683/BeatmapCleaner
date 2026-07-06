using BeatmapExporterCore.Exporters.Lazer.LazerDB.Schema;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeatmapExporterGUI.ViewModels.List
{
    /// <summary>
    /// Page for listing all beatmaps. Currently displays all beatmap sets on one side and further explores a selected beatmap further on the other.
    /// </summary>
    public partial class BeatmapListViewModel : ViewModelBase
    {
        public BeatmapListViewModel()
        {
            BeatmapSetList = new();
            _DisplayedBeatmapSets = new();

            SelectedSetIndex = -1;
            SelectedDisplayOption = 0;
            SelectedSortOption = 0;

            SortOptions = BeatmapSorting.AllSortOptions.ToList();
            DisplayOptions = BeatmapSorting.AllDisplayOptions.ToList();

            // Applies the initial display setting - loading and displaying beatmaps
            Task.Run(() => ApplyDisplaySetting());
        }

        #region Beatmap Listing/Selection
        /// <summary>
        /// ViewModel for the currently selected beatmap - right hand side of the user interface.
        /// </summary>
        [ObservableProperty]
        private BeatmapExplorerViewModel? _BeatmapExplorer;

        /// <summary>
        /// The currently selected beatmap set by the user.
        /// </summary>
        [ObservableProperty]
        private int _SelectedSetIndex;

        /// <summary>
        /// When the selected beatmap set is changed by the user, the right hand side "explorer" view is changed to that new beatmap set.
        /// </summary>
        partial void OnSelectedSetIndexChanged(int value)
        {
            if (value != -1)
            {
                var selectedSet = DisplayedBeatmapSets[value];
                BeatmapExplorer = new(this, selectedSet);
            }
        }

        /// <summary>
        /// A list of beatmap sets that should currently be dispalyed to the user
        /// </summary>
        [ObservableProperty]
        private List<BeatmapSet> _DisplayedBeatmapSets;

        partial void OnDisplayedBeatmapSetsChanged(List<BeatmapSet> value)
        {
            Task.Run(() => Exporter.RealmScheduler.Schedule(async () =>
            {
                await ApplySorting();
            }));
        }

        /// <summary>
        /// A list of displayable strings directly representing the current DisplayedBeatmapSet list 
        /// </summary>
        [ObservableProperty]
        private List<string> _BeatmapSetList;
        #endregion

        #region Beatmap Sorting/Viewing
        internal List<BeatmapSorting.SortBy> SortOptions { get; }

        internal List<BeatmapSorting.View> DisplayOptions { get; }

        public List<string> SortOptionNames => SortOptions.Select(s => s.FullName()).ToList();

        [ObservableProperty]
        private int _SelectedSortOption;

        partial void OnSelectedSortOptionChanged(int value)
        {
            Task.Run(() => ApplySorting());
        }

        private async Task ApplySorting() => await Exporter.RealmScheduler.Schedule(() =>
        {
            if (SelectedSortOption != -1)
            {
                var sortBy = (BeatmapSorting.SortBy)SelectedSortOption;

                int stableComparer(BeatmapSet x, BeatmapSet y)
                {
                    var selectedSort = sortBy.Comparer()(x, y);
                    if (selectedSort == 0)
                    {
                        return x.ID.CompareTo(y.ID);
                    }
                    return selectedSort;
                }

                DisplayedBeatmapSets.Sort(stableComparer);
                BeatmapSetList = DisplayedBeatmapSets.Select(set => set.DiffSummary()).ToList();
            }
        });

        public List<string> DisplayOptionNames => DisplayOptions.Select(d => d.SetName()).ToList();

        [ObservableProperty]
        private int _SelectedDisplayOption;

        partial void OnSelectedDisplayOptionChanged(int value)
        {
            Task.Run(() => ApplyDisplaySetting());
        }

        private async Task ApplyDisplaySetting() => await Exporter.RealmScheduler.Schedule(() =>
        {
            IEnumerable<BeatmapSet> displayMaps;
            if (SelectedDisplayOption == (int)BeatmapSorting.View.Selected)
            {
                displayMaps = Exporter.Lazer!.SelectedBeatmapSets;
            }
            else
            {
                displayMaps = Exporter.Lazer!.AllBeatmapSets;
            }
            if (!string.IsNullOrWhiteSpace(UserSearchInput))
            {
                displayMaps = displayMaps.Where(set => set.DiffSummary().Contains(UserSearchInput, StringComparison.OrdinalIgnoreCase));
            }

            DisplayedBeatmapSets = displayMaps.ToList();
        });

        [ObservableProperty]
        private string _UserSearchInput;

        partial void OnUserSearchInputChanged(string value)
        {
            Task.Run(() => ApplyDisplaySetting());
        }
        #endregion
    }
}