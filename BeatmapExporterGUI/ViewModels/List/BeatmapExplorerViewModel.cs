using BeatmapExporterCore.Exporters.Lazer.LazerDB.Schema;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeatmapExporterGUI.ViewModels.List
{
    /// <summary>
    /// Page for exploring the files within a single beatmap set. Currently displays the beatmap difficulties on top and all files (including difficulty files) on the bottom.
    /// </summary>
    public partial class BeatmapExplorerViewModel : ViewModelBase
    {
        private readonly BeatmapListViewModel beatmapListView;
        private readonly BeatmapSet selectedSet;

        public BeatmapExplorerViewModel(BeatmapListViewModel parent, BeatmapSet set)
        {
            beatmapListView = parent;
            selectedSet = set;

            SelectedDisplayOption = parent.SelectedDisplayOption; // inherit all/selected option from parent list
            SelectedDiffIndex = -1;
            SelectedFileIndex = -1;
            SelectedReplayIndex = -1;

            _DisplayedDiffs = new();
            _DiffNames = new();
            FileNames = new();
            BeatmapScores = new();
            Replays = new();

            Task.Run(() => Exporter.RealmScheduler.Schedule(async () =>
            {
                var metadata = set.DiffMetadata;

                SetName = $"{metadata?.Title} ({metadata?.Author.Username})";
                OnPropertyChanged(nameof(SetName));

                FileNames = set.Files.Select(f => f.Filename).ToList();
                OnPropertyChanged(nameof(FileNames));

                BeatmapScores = set.AllScores.Where(s => s != null).ToList();
                Replays = BeatmapScores.Select(s => s.Details()).ToList();
                OnPropertyChanged(nameof(Replays));

                await ApplyDisplaySetting();
            }));
        }

        /// <summary>
        /// The song and mapper name of the beatmap set this explorer represents.
        /// </summary>
        public string SetName { get; private set; } = string.Empty;

        #region Diff Display Settings
        /// <summary>
        /// List of all the displayed difficulties, may change with user selection change.
        /// </summary>
        [ObservableProperty]
        private List<Beatmap> _DisplayedDiffs;

        /// <summary>
        /// List of all the user scores for this beatmap set.
        /// </summary>
        private List<Score> BeatmapScores;

        /// <summary>
        /// The currently selected display option, which is indexed 1:1 to the <see cref="BeatmapSorting.View" /> enum values.
        /// </summary>
        [ObservableProperty]
        private int _SelectedDisplayOption;

        /// <summary>
        /// The string representations for all supported display options. ex. Display all beatmaps
        /// </summary>
        public List<string> DisplayOptionNames => beatmapListView.DisplayOptions.Select(d => d.DiffName()).ToList();

        partial void OnSelectedDisplayOptionChanged(int value)
        {
            Task.Run(() => ApplyDisplaySetting());
        }

        /// <summary>
        /// Updates the displayed beatmap difficulties to match the current <see cref="SelectedDisplayOption" />
        /// </summary>
        private async Task ApplyDisplaySetting() => await Exporter.RealmScheduler.Schedule(() =>
        {
            if (SelectedDisplayOption == (int)BeatmapSorting.View.Selected)
            {
                DisplayedDiffs = selectedSet.SelectedBeatmaps.ToList();
            }
            else
            {
                DisplayedDiffs = selectedSet.Beatmaps.ToList();
            }
            DiffNames = DisplayedDiffs.Select(d => d.Details()).ToList();
        });
        #endregion

        #region File/Replay Display
        /// <summary>
        /// The string representations for the difficulties listed within this beatmap set, indexed 1:1 to <see cref="DisplayedDiffs" />
        /// </summary>
        [ObservableProperty]
        private List<string> _DiffNames;

        /// <summary>
        /// The index of the currently user-selected difficulty, indexed to both <see cref="DisplayedDiffs" /> and <see cref="DiffNames" />
        /// </summary>
        [ObservableProperty]
        private int _SelectedDiffIndex;

        /// <summary>
        /// The index of the currently user-selected file, indexed to both <see cref="FileNames" /> and <see cref="BeatmapSet.Files" />
        /// </summary>
        [ObservableProperty]
        private int _SelectedFileIndex;

        /// <summary>
        /// The index of the currently user-selected score replay, indexed to both <see cref="BeatmapScores"/> and <see cref="Replays" />
        /// </summary>
        [ObservableProperty]
        private int _SelectedReplayIndex;

        /// <summary>
        /// The string representations for the files listed within this beatmap set. 
        /// </summary>
        public List<string> FileNames { get; private set; }

        /// <summary>
        /// The string representations for the player replays for this beatmap set.
        /// </summary>
        public List<string> Replays { get; private set; }
        #endregion
    }
}