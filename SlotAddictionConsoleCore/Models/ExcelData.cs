using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using SlotAddictionCore.Const;

namespace SlotAddictionCore.Models
{
    public class ExcelData
    {
        #region フィールド
        private static string ReadMasterExcelName = @"C:\Users\y\Desktop\test\SlotAddictionDataM.xlsx";
        #endregion

        #region メソッド

        private static void SetReadExcelPath()
        {
            var folderPath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
            var folderName = Path.GetFileName(folderPath);
            if (folderName == "Debug"
                || folderName == "Release")
            {
                ReadMasterExcelName = @"C:\Users\y\Desktop\test\SlotAddictionDataMDebug.xlsx";
            }
        }

        /// <summary>
        /// データを取得したい機種名を検索する機種名を取得する
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, List<string>> GetSlotMachineSearchNames()
        {
            SetReadExcelPath();
            var folderPath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
            var folderName = Path.GetFileName(folderPath);
            Console.WriteLine(folderName + "フォルダ");
            using (var package = new ExcelPackage())
            using (var masterStream = new FileStream(ReadMasterExcelName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                package.Load(masterStream);

                //データを取得したい機種名を入力したシートのデータを取得
                var slotMachineObtainNameSheet = package.Workbook.Worksheets[SheetIndex.MachineList];
                var slotMachineObtainNames = new List<string>();

                //読み込んだシートに入力されているセルの中で最大の行数を取得
                //EPPlusは1ベースである為タイトルを読まなくていいようにする為にはi = 2からスタートさせる
                for (var i = 2; i <= slotMachineObtainNameSheet.Dimension.Rows; ++i)
                {
                    var exeFolderName = slotMachineObtainNameSheet.Cells[i, 1].Value;
                    var slotMachineObtainName = slotMachineObtainNameSheet.Cells[i, 2].Value;

                    //実行するフォルダ名と等しければデータを取得したい機種名を追加
                    if (exeFolderName != null
                        && slotMachineObtainName != null
                        && exeFolderName.ToString() == folderName)
                    {
                        slotMachineObtainNames.Add(slotMachineObtainName.ToString());
                        Console.WriteLine(slotMachineObtainName);
                    }
                }

                //検索用の機種名を入力したシートのデータを取得
                var slotMachineSearchNameSheet = package.Workbook.Worksheets[SheetIndex.SearchMachineList];
                var slotMachines = new List<Machine>();

                //読み込んだシートに入力されているセルの中で最大の行数を取得
                //EPPlusは1ベースである為タイトルを読まなくていいようにする為にはi = 2からスタートさせる
                for (var i = 2; i <= slotMachineSearchNameSheet.Dimension.Rows; ++i)
                {
                    var slotMachineObtainName = slotMachineSearchNameSheet.Cells[i, 1].Value;
                    var slotMachineSearchName = slotMachineSearchNameSheet.Cells[i, 2].Value;
                    if (slotMachineObtainName != null
                        && slotMachineSearchName != null)
                    {
                        slotMachines.Add(new Machine
                        {
                            Name = slotMachineObtainName.ToString(),
                            SearchName = slotMachineSearchName.ToString(),
                        });
                    }
                }

                return slotMachines
                    .Where(x => slotMachineObtainNames.Contains(x.Name))
                    .GroupBy(x => x.Name)
                    .ToDictionary(
                        x => x.Key,
                        x => x.Select(y => y.SearchName).ToList());
            }
        }
        /// <summary>
        /// 店舗情報を取得する
        /// </summary>
        /// <returns></returns>
        public static List<TenpoInfo> GetTenpoInfo()
        {
            SetReadExcelPath();
            using (var package = new ExcelPackage())
            using (var masterStream = new FileStream(ReadMasterExcelName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                package.Load(masterStream);

                //店舗シートのデータを取得
                var tenpoSheet = package.Workbook.Worksheets[SheetIndex.TenpoList];
                var tenpoInfoList = new List<TenpoInfo>();

                //読み込んだシートに入力されているセルの中で最大の行数を取得
                //EPPlusは1ベースである為タイトルを読まなくていいようにする為にはi = 2からスタートさせる
                for (var i = 2; i <= tenpoSheet.Dimension.Rows; ++i)
                {
                    var tenpoName = tenpoSheet.Cells[i, 1].Value;
                    if (tenpoName != null)
                    {
                        var tenpoUri = new Uri(tenpoSheet.Cells[i, 2].Value.ToString());
                        tenpoInfoList.Add(new TenpoInfo
                        {
                            TenpoName = tenpoName.ToString(),
                            TenpoUri = tenpoUri,
                        });
                    }
                }

                return tenpoInfoList;
            }
        }
        /// <summary>
        /// 狙い目の台情報を取得する
        /// </summary>
        /// <returns></returns>
        public static List<AimMachineInfo> GetAimMachineInfo()
        {
            SetReadExcelPath();
            using (var package = new ExcelPackage())
            using (var masterStream = new FileStream(ReadMasterExcelName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                package.Load(masterStream);

                //狙い目シートのデータを取得
                var aimMachineSheet = package.Workbook.Worksheets[SheetIndex.AimMachineList];
                var aimMachineInfo = new List<AimMachineInfo>();

                //読み込んだシートに入力されているセルの中で最大の行数を取得
                //EPPlusは1ベースである為2列目から読ませる為にi = 2からスタートさせる
                for (var i = 2; i <= aimMachineSheet.Dimension.Columns; ++i)
                {
                    var machineName = aimMachineSheet.Cells[AimMachineSheetRowIndex.MachineName, i].Value;
                    if (machineName == null) continue;

                    //入力必須項目
                    var memo = aimMachineSheet.Cells[AimMachineSheetRowIndex.Memo, i].Value;
                    var expectedValue = int.Parse(aimMachineSheet.Cells[AimMachineSheetRowIndex.ExpectedValue, i].Value.ToString());
                    var provisionStartCount = int.Parse(aimMachineSheet.Cells[AimMachineSheetRowIndex.ProvisionStartCount, i].Value.ToString());
                    var provisionTotalCount = int.Parse(aimMachineSheet.Cells[AimMachineSheetRowIndex.ProvisionTotalCount, i].Value.ToString());
                    var provisionOvernightCount = int.Parse(aimMachineSheet.Cells[AimMachineSheetRowIndex.ProvisionOvernightCount, i].Value.ToString());
                    var firstWinningCount = int.Parse(aimMachineSheet.Cells[AimMachineSheetRowIndex.FirstWinningCount, i].Value.ToString());

                    //任意項目
                    var provisionWinningCount = aimMachineSheet.Cells[AimMachineSheetRowIndex.ProvisionWinningCountFrom, i].Value;
                    int? provisionWinningCountFrom = null;
                    if (provisionWinningCount != null)
                    {
                        provisionWinningCountFrom = int.Parse(provisionWinningCount.ToString());
                    }
                    provisionWinningCount = aimMachineSheet.Cells[AimMachineSheetRowIndex.ProvisionWinningCountTo, i].Value;
                    int? provisionWinningCountTo = null;
                    if (provisionWinningCount != null)
                    {
                        provisionWinningCountTo = int.Parse(provisionWinningCount.ToString());
                    }
                    var winninRateInfo = new WinninRateInfo();
                    var provisionCombineWinningRateFrom = aimMachineSheet.Cells[AimMachineSheetRowIndex.ProvisionCombineWinningRateFrom, i].Value;
                    if (provisionCombineWinningRateFrom != null)
                    {
                        winninRateInfo.CombineFrom = double.Parse(provisionCombineWinningRateFrom.ToString());
                    }
                    var provisionCombineWinningRateTo = aimMachineSheet.Cells[AimMachineSheetRowIndex.ProvisionCombineWinningRateTo, i].Value;
                    if (provisionCombineWinningRateTo != null)
                    {
                        winninRateInfo.CombineTo = double.Parse(provisionCombineWinningRateTo.ToString());
                    }
                    var provisionBigBonusWinningRate = aimMachineSheet.Cells[AimMachineSheetRowIndex.ProvisionBigBonusWinningRate, i].Value;
                    if (provisionBigBonusWinningRate != null)
                    {
                        winninRateInfo.BigBonus = double.Parse(provisionBigBonusWinningRate.ToString());
                    }
                    var provisionRegularBonusWinninRate = aimMachineSheet.Cells[AimMachineSheetRowIndex.ProvisionRegularBonusWinninRate, i].Value;
                    if (provisionRegularBonusWinninRate != null)
                    {
                        winninRateInfo.RegularBonus = double.Parse(provisionRegularBonusWinninRate.ToString());
                    }

                    var betweenWinningType = WinningType.None;
                    var betweenWinningProvisionCount = 0;
                    var betweenWinningHistory = aimMachineSheet.Cells[AimMachineSheetRowIndex.BetweenWinningType, i].Value;
                    if (betweenWinningHistory != null)
                    {
                        betweenWinningType = ConvertWinningType(betweenWinningHistory.ToString());
                        if (betweenWinningType != WinningType.None)
                        {
                            betweenWinningProvisionCount = int.Parse(aimMachineSheet.Cells[AimMachineSheetRowIndex.BetweenWinningProvisionCount, i].Value.ToString());
                        }
                    }

                    var winningList = new List<WinningType>();
                    for (var j = AimMachineSheetRowIndex.WinningHistoryStart; j <= aimMachineSheet.Dimension.Rows; ++j)
                    {
                        var winningTypeCellValue = aimMachineSheet.Cells[j, i].Value;
                        if (winningTypeCellValue == null) break;
                        var winningType = ConvertWinningType(winningTypeCellValue.ToString());
                        winningList.Add(winningType);
                    }

                    aimMachineInfo.Add(new AimMachineInfo
                    {
                        MachineName = machineName.ToString(),
                        Memo = memo.ToString(),
                        ExpectedValue = expectedValue,
                        ProvisionStartCount = provisionStartCount,
                        ProvisionTotalCount = provisionTotalCount,
                        ProvisionOvernightCount = provisionOvernightCount,
                        FirstWinningCount = firstWinningCount,
                        ProvisionWinningCountFrom = provisionWinningCountFrom,
                        ProvisionWinningCountTo = provisionWinningCountTo,
                        WinningRateInfo = winninRateInfo,
                        BetweenWinningType = betweenWinningType,
                        BetweenWinningProvisionCount = betweenWinningProvisionCount,
                        WinningList = winningList,
                    });
                }

                return aimMachineInfo;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="winningType"></param>
        /// <returns></returns>
        private static WinningType ConvertWinningType(string winningType)
        {
            switch (winningType)
            {
                case "ART":
                    return WinningType.ART;
                case "BB":
                    return WinningType.BB;
                case "RB":
                    return WinningType.RB;
                case "BBorRB":
                    return WinningType.BB | WinningType.RB;
                case "ARTorBB":
                    return WinningType.ART | WinningType.BB;
                case "ARTorRB":
                    return WinningType.ART | WinningType.RB;
                case "ARTorBBorRB":
                    return WinningType.ART | WinningType.BB | WinningType.RB;
                default:
                    return WinningType.None;
            }
        }

        #endregion

        #region クラス
        /// <summary>
        /// 台クラス
        /// </summary>
        private class Machine
        {
            /// <summary>
            /// 機種名
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// 検索名
            /// </summary>
            public string SearchName { get; set; }
        }
        /// <summary>
        /// </summary>
        private static class SheetIndex
        {
            public const int MachineList = 1;
            public const int SearchMachineList = 2;
            public const int TenpoList = 3;
            public const int AimMachineList = 4;
        }
        /// <summary>
        /// 行番号
        /// </summary>
        private static class AimMachineSheetRowIndex
        {
            /// <summary>
            /// 機種名
            /// </summary>
            public const int MachineName = 1;
            /// <summary>
            /// メモ
            /// </summary>
            public const int Memo = 2;
            /// <summary>
            /// 期待値
            /// </summary>
            public const int ExpectedValue = 3;
            /// <summary>
            /// 規定スタート数
            /// </summary>
            public const int ProvisionStartCount = 4;
            /// <summary>
            /// 規定総回転数
            /// </summary>
            public const int ProvisionTotalCount = 5;
            /// <summary>
            ///宵越し規定ゲーム数 
            /// </summary>
            public const int ProvisionOvernightCount = 6;
            /// <summary>
            /// 初回当選ゲーム数
            /// </summary>
            public const int FirstWinningCount  = 7;
            /// <summary>
            /// 規定当選回数From
            /// </summary>
            public const int ProvisionWinningCountFrom = 8;
            /// <summary>
            /// 規定当選回数To
            /// </summary>
            public const int ProvisionWinningCountTo = 9;
            /// <summary>
            /// 規定合算確率From(分母)
            /// </summary>
            public const int ProvisionCombineWinningRateFrom = 10;
            /// <summary>
            /// 規定合算確率To(分母)
            /// </summary>
            public const int ProvisionCombineWinningRateTo = 11;
            /// <summary>
            /// 規定BB確率(分母)
            /// </summary>
            public const int ProvisionBigBonusWinningRate = 12;
            /// <summary>
            /// 規定RB確率(分母)
            /// </summary>
            public const int ProvisionRegularBonusWinninRate = 13;
            /// <summary>
            /// 当選履歴の対象
            /// </summary>
            public const int BetweenWinningType = 14;
            /// <summary>
            /// 当選履歴間回転数
            /// </summary>
            public const int BetweenWinningProvisionCount = 15;
            /// <summary>
            /// 当選履歴
            /// </summary>
            public const int WinningHistoryStart = 16;
        }

        #endregion
    }
}
