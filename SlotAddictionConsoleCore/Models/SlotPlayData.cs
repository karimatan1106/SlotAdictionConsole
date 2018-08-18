using System;
using System.Collections.Generic;

namespace SlotAddictionCore.Models
{
    /// <summary>
    /// 遊技台の情報
    /// </summary>
    public class SlotPlayData
    {
        /// <summary>
        /// 設置店名
        /// </summary>
        public string StoreName { get; set; }
        /// <summary>
        /// 台名
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// コイン単価
        /// </summary>
        public decimal CoinPrice { get; set; }
        /// <summary>
        /// 台番号
        /// </summary>
        public int MachineNO { get; set; }
        /// <summary>
        /// 最終更新日時
        /// </summary>
        public string LatestUpdateDatetime { get; set; }
        /// <summary>
        /// 最大持球
        /// </summary>
        public int MaxPayout { get; set; }
        /// <summary>
        /// 総回転数
        /// </summary>
        public int TotalCount { get; set; }
        /// <summary>
        /// 前日最終スタート数
        /// </summary>
        public int FinalStartCountYesterDay { get; set; }
        /// <summary>
        /// 合算確率
        /// </summary>
        public string CombinedWinningRate { get; set; }
        /// <summary>
        /// 合算確率の分母
        /// </summary>
        public double CombinedWinningRateDenominator { get; set; }
        /// <summary>
        /// ビッグボーナス確率
        /// </summary>
        public string BigBonusWinningRate { get; set; }
        /// <summary>
        /// ビッグボーナス確率の分母
        /// </summary>
        public double BigBonusWinningRateDenominator { get; set; }
        /// <summary>
        /// レギュラーボーナス確率
        /// </summary>
        public string RegularBonusWinningRate { get; set; }
        /// <summary>
        /// レギュラーボーナス確率の分母
        /// </summary>
        public double RegularBonusWinningRateDenominator { get; set; }
        /// <summary>
        /// BB回数
        /// </summary>
        public int BigBonusCount { get; set; }
        /// <summary>
        /// RB回数
        /// </summary>
        public int RegulerBonusCount { get; set; }
        /// <summary>
        /// ART回数
        /// </summary>
        public int ARTCount { get; set; }
        /// <summary>
        /// 現在の回転数
        /// </summary>
        public int StartCount { get; set; }
        /// <summary>
        /// ステータス
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 当選履歴
        /// </summary>
        public List<WinningHistoryDetail> WinningHistory { get; set; }
        /// <summary>
        /// 台データを参照できるURI
        /// </summary>
        public Uri SlotMachineUri { get; set; }
        /// <summary>
        /// 期待値
        /// </summary>
        public int ExceptValue { get; set; }
        /// <summary>
        /// 当選履歴情報の公開
        /// </summary>
        public bool IsPublicWinningHistory { get; set; }
    }
}
