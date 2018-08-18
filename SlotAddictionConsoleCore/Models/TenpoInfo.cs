using System;
using System.Collections.Generic;

namespace SlotAddictionCore.Models
{
    /// <summary>
    /// 店舗情報
    /// </summary>
    public class TenpoInfo
    {
        /// <summary>
        /// 店舗名
        /// </summary>
        public string TenpoName { get; set; }
        /// <summary>
        /// 店舗URI
        /// </summary>
        public Uri TenpoUri { get; set; }
        /// <summary>
        /// スロットが存在する台番号
        /// </summary>
        public List<(int slotMachineStartNo, int slotMachineEndNo)> SlotMachineExistsFromTo { get; set; }
    }
}
