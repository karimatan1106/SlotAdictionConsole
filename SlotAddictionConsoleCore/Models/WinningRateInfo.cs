namespace SlotAddictionCore.Models
{
    /// <summary>
    /// 勝利確率
    /// </summary>
    public class WinninRateInfo
    {
        /// <summary>
        /// 合算確率From
        /// </summary>
        public double? CombineFrom { get; set; }
        /// <summary>
        /// 合算確率To
        /// </summary>
        public double? CombineTo { get; set; }
        /// <summary>
        /// BB確率
        /// </summary>
        public double? BigBonus { get; set; }
        /// <summary>
        /// RB確率
        /// </summary>
        public double? RegularBonus { get; set; }
    }
}
