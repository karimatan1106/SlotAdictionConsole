using System;

namespace SlotAddictionCore.Const
{
    [Flags]
    public enum WinningType
    {
        /// <summary>
        /// 無し
        /// </summary>
        None = 0,
        /// <summary>
        /// ART
        /// </summary>
        ART = 1,
        /// <summary>
        /// ビッグボーナス
        /// </summary>
        BB = 2,
        /// <summary>
        /// レギュラーボーナス
        /// </summary>
        RB = 4,
    }
}
