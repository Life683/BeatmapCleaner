using BeatmapExporterCore.Utilities;
using Realms;

// Original schema source file Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
namespace BeatmapExporterCore.Exporters.Lazer.LazerDB.Schema
{
    /// <summary>
    /// A realm model containing metadata for a single score.
    /// </summary>
    public class Score : RealmObject
    {
        [PrimaryKey]
        public Guid ID { get; set; }
        public Beatmap? BeatmapInfo { get; set; }
        public string BeatmapHash { get; set; } = string.Empty;
        public IList<RealmNamedFileUsage> Files { get; } = null!;
        public double Accuracy { get; set; }
        public DateTimeOffset Date { get; set; }
        public RealmUser User { get; set; } = null!;
        public string Mods { get; set; } = string.Empty;
        public string Statistics { get; set; } = string.Empty;
        public IList<int> Pauses { get; } = null!;
        public int Rank { get; set; }
        public string ClientVersion { get; set; } = string.Empty;
        public Ruleset Ruleset { get; set; } = null!;
        public string Hash { get; set; } = string.Empty;
        public bool DeletePending { get; set; }
        public long TotalScore { get; set; }
        public long TotalScoreWithoutMods { get; set; }
        public int TotalScoreVersion { get; set; }
        public long? LegacyTotalScore { get; set; }
        public bool BackgroundReprocessingFailed { get; set; }
        public int MaxCombo { get; set; }
        public double? PP { get; set; }
        public long OnlineID { get; set; } = -1;
        public long LegacyOnlineID { get; set; } = -1;
        public string MaximumStatistics { get; set; } = string.Empty;
        public int Combo { get; set; }
        public bool IsLegacyScore { get; set; }

        // Author kabii
        /// <summary>
        /// Produces the output-friendly letter rank for this player score
        /// </summary>
        [Ignored]
        public string RankLetter
        {
            get => Rank switch
            {
                -1 => "F",
                0 => "D",
                1 => "C",
                2 => "B",
                3 => "A",
                4 => "S",
                5 => "S+",
                6 => "SS",
                7 => "SS+",
                _ => "_"
            };
        }

        /// <summary>
        /// A string which distinguishes this score replay in a single beatmap set
        /// </summary>
        public string Details()
        {
            var age = DateTime.Now - Date;
            return $"({age.Days}d) {User.Username} {Accuracy:0.00%} {RankLetter} rank on [{BeatmapInfo!.DifficultyName}]";
        }

        /// <summary>
        /// The full filename to be used for exporting this player score replay.
        /// </summary>
        public string OutputReplayFilename() => 
            $"{User.Username} {RankLetter} rank on {BeatmapInfo!.Metadata.OutputName()} [{BeatmapInfo.DifficultyName}] ({Date.LocalDateTime:yyyy-MM-dd HH-mm-ss}).osr"
            .RemoveFilenameCharacters();
    }
}
