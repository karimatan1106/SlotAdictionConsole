using SlotAddictionCore.Const;

namespace SlotAddictionCore.Models
{
    public class WinningHistoryDetail
    {
        /// <summary>
        /// 大当たり
        /// </summary>
        public string HitCount { get; set; }
        /// <summary>
        /// 当選時点のスタート回数
        /// </summary>
        public int HitStart { get; set; }
        /// <summary>
        /// 当選による出玉
        /// </summary>
        public int HitPayout { get; set; }
        /// <summary>
        /// 当選種類
        /// </summary>
        public WinningType WinningType { get; set; }
        /// <summary>
        /// 当選時間
        /// </summary>
        public string HitTime { get; set; }
    }
}
