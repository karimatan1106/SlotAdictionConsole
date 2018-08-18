using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlotAddictionLogic.Line;
using SlotAddictionLogic.Models;
using SlotAddictionCore.Models;

namespace SlotAddictionConsole
{
    public class Program
    {
        #region フィールド
        /// <summary>
        /// ライン通知のトークン
        /// </summary>
        private static string _lineToken = "Stjd5Laa2ZPH6tZ6OIxpOJtxR9PbVMxt8kBC9Sa4OYU";
        #endregion

        #region メソッド

        private static async Task Main(string[] args)
        {
            var date = DateTime.Now.AddDays(-1);
#if DEBUG
            if (DateTime.Now.Hour >= 10)
            {
                date = DateTime.Now;
            }
#endif
            //念のため
            try
            {
                var analyseResult = await GetAnalyseResult(date);
#if DEBUG
                _lineToken = "XLzPrFUKoS33P4QCAqO15M31c9RfDhYMJjgJoV5fIJX";
#endif
                LineAlert.Message(_lineToken, analyseResult);
            }
            catch (Exception e)
            {
                var hoge = 0;
                //ignore
            }

#if !DEBUG
            var folderPath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
            var folderName = Path.GetFileName(folderPath);
            _lineToken = "XLzPrFUKoS33P4QCAqO15M31c9RfDhYMJjgJoV5fIJX";
            LineAlert.Message(_lineToken, folderName + Environment.NewLine + "動作してるよ");
#endif
        }
        /// <summary>
        /// 遊技履歴から狙い目台の情報解析を行う
        /// </summary>
        /// <param name="searchDate"></param>
        /// <returns></returns>
        private static async Task<string> GetAnalyseResult(DateTime searchDate)
        {
            var output = new StringBuilder(string.Empty);
            var tenpoInfo = ExcelData.GetTenpoInfo();
            var slotMachineSearchNames = ExcelData.GetSlotMachineSearchNames();
            if (!slotMachineSearchNames.Any())
            {
                return "データを取得したい機種名が設定されてないンゴなぁ";
            }

            var analyseSlotData = new AnalyseSlotData(searchDate);
            foreach (var slotMachineSearchName in slotMachineSearchNames)
            {
                //機種名を検索する文字列を取得して解析する
                var aimMachineInfo = ExcelData.GetAimMachineInfo().Where(x => x.MachineName == slotMachineSearchName.Key).ToList();
                await analyseSlotData.GetSlotDataAsync(tenpoInfo, slotMachineSearchName.Value, aimMachineInfo);

                //狙える台がない場合
                if (!analyseSlotData.SlotPlayDataCollection.Any())
                {
                    output.AppendLine("(設置されて)ないです。");
                }
                else if (analyseSlotData.AimMachineCollection.Any())
                {
                    //機種名を出力
                    output.AppendLine(Environment.NewLine + "【" + slotMachineSearchName.Key + "】");

                    //ステータスでグルーピングする
                    var aimMachineStatusGrouping = analyseSlotData.AimMachineCollection
                        .OrderByDescending(x => x.CoinPrice) //コレクションの中身をコイン単価で降順に並べる
                        .ThenByDescending(x => x.StartCount) //コレクションの中身をスタート回数で降順に並べる
                        .GroupBy(x => new { x.Status, x.ExceptValue }) //ステータスと期待値でグループ化する
                        .OrderByDescending(x => x.Key.ExceptValue); //グループを期待値で降順に並べる
                    foreach (var aimMachines in aimMachineStatusGrouping)
                    {
                        output.AppendLine("[" + aimMachines.Key.Status + "]");

                        foreach (var aimMachine in aimMachines)
                        {
                            output.AppendLine("(" + aimMachine.CoinPrice + "円) " + aimMachine.StoreName + " " + aimMachine.StartCount + "G");
                            output.AppendLine(aimMachine.SlotMachineUri.ToString());
                        }

                    }
                }
            }

            output.AppendLine();
            return output.ToString();
        }
        #endregion
    }
}
