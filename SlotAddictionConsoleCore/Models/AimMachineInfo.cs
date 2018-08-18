using System.Collections.Generic;
using SlotAddictionCore.Const;

namespace SlotAddictionCore.Models
{
    /// <summary>
    /// 狙い目台情報
    /// </summary>
    public class AimMachineInfo
    {
        /// <summary>
        /// 台名
        /// </summary>
        public string MachineName { get; set; }
        /// <summary>
        /// メモ
        /// </summary>
        public string Memo { get; set; }
        /// <summary>
        /// 期待値
        /// </summary>
        public int ExpectedValue { get; set; }
        /// <summary>
        /// 規定ゲーム数
        /// </summary>
        public int ProvisionStartCount { get; set; }
        /// <summary>
        /// 規定総回転数
        /// </summary>
        public int ProvisionTotalCount { get; set; }
        /// <summary>
        ///宵越し規定ゲーム数 
        /// </summary>
        public int ProvisionOvernightCount { get; set; }
        /// <summary>
        /// 初回当選ゲーム数
        /// </summary>
        public int FirstWinningCount { get; set; }
        /// <summary>
        /// 規定当選回数From
        /// </summary>
        public int? ProvisionWinningCountFrom { get; set; }
        /// <summary>
        /// 規定当選回数To
        /// </summary>
        public int? ProvisionWinningCountTo { get; set; }
        /// <summary>
        /// 勝利確率
        /// </summary>
        public WinninRateInfo WinningRateInfo { get; set; }
        /// <summary>
        /// 当選履歴
        /// </summary>
        public List<WinningType> WinningList { get; set; }
        /// <summary>
        /// 当選履歴間の種類
        /// </summary>
        public WinningType BetweenWinningType { get; set; }
        /// <summary>
        /// 当選履歴間規定ゲーム数
        /// </summary>
        public int BetweenWinningProvisionCount { get; set; }
    }
}
