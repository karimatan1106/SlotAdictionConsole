using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using SlotAddictionCore.Extentions;
using SlotAddictionCore.Models;
using SlotAddictionCore.Models.Scrapings;

namespace SlotAddictionLogic.Models
{
    public class AnalyseSlotData
    {
        #region フィールド
        /// <summary>
        /// usingステートメントを使用して何度もソケットを開放するとリソースを食いつぶす為staticなHttpClientを作成する
        /// </summary>
        private static readonly HttpClient _httpClient = new HttpClient();
        /// <summary>
        /// HTML解析オブジェクト
        /// </summary>
        private readonly Goraggio _analysisHTML = new Goraggio();
        /// <summary>
        /// データ取得日
        /// </summary>
        private readonly DateTime _dataDate;
        /// <summary>
        /// 検索対象の店舗情報
        /// </summary>
        private List<TenpoInfo> _tenpoInfoList;
        /// <summary>
        /// 検索する機種名
        /// </summary>
        private List<string> _slotMachineSearchNames;
        /// <summary>
        /// 狙い目台の情報
        /// </summary>
        private List<AimMachineInfo> _aimMachineList;
        /// <summary>
        /// フロアを検索するURI
        /// </summary>
        private readonly HashSet<Uri> _floorSearchUri = new HashSet<Uri>();
        /// <summary>
        /// 遊技台のURI
        /// </summary>
        private readonly HashSet<Uri> _slotDataUri = new HashSet<Uri>();
        #endregion

        #region プロパティ
        /// <summary>
        /// 遊技履歴を格納
        /// </summary>
        public HashSet<SlotPlayData> SlotPlayDataCollection { get; set; }
        /// <summary>
        /// 狙い目の台を格納
        /// </summary>
        public HashSet<SlotPlayData> AimMachineCollection { get; set; }
        #endregion

        #region コンストラクタ
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataDate"></param>
        public AnalyseSlotData(DateTime dataDate)
        {
            ServicePointManager.DefaultConnectionLimit = 1000;
            _dataDate = dataDate;
        }
        #endregion

        #region メソッド
        /// <summary>
        /// 遊技台のデータ取得
        /// </summary>
        /// <param name="tenpoInfoList"></param>
        /// <param name="slotMachineSearchNames"></param>
        /// <param name="aimMachineInfo"></param>
        /// <returns></returns>
        public async Task GetSlotDataAsync(List<TenpoInfo> tenpoInfoList, List<string> slotMachineSearchNames, List<AimMachineInfo> aimMachineInfo)
        {
            SlotPlayDataCollection = new HashSet<SlotPlayData>();
            AimMachineCollection = new HashSet<SlotPlayData>();

            _tenpoInfoList = tenpoInfoList;
            _slotMachineSearchNames = slotMachineSearchNames;
            _aimMachineList = aimMachineInfo;

            //foreach (var tempo in _tenpoInfoList.Where(x => x.TenpoUri.ToString() == "https://daidata.goraggio.com/100162/"))
            foreach (var tempo in _tenpoInfoList)
            {
                //初期化
                _floorSearchUri.Clear();
                _slotDataUri.Clear();

                //一旦置いとく
                //var slotMachineStartNo = tempo.SlotMachineStartNo;
                //var slotMachineEndNo = tempo.SlotMachineEndNo;
                //var slotMachineNumbers = Enumerable.Range(slotMachineStartNo, slotMachineEndNo - slotMachineStartNo + 1);
                var slotMachineNumbers = Enumerable.Empty<int>();

                if (_slotMachineSearchNames != null)
                {
                    //指定した台が店舗に存在するか確かめられるURIを作成
                    var floorSearchUri = _slotMachineSearchNames.Select(slotMachineSearchName => new Uri($"{tempo.TenpoUri.ToString()}unit_list?model={slotMachineSearchName}"));
                    _floorSearchUri.AddRange(await floorSearchUri.GetExistUriAsync(_httpClient));

                    //指定した台が無ければ次の店舗の検索をする
                    if (!_floorSearchUri.Any()) continue;

                    try
                    {
                        var floorStreamTasks = _floorSearchUri.Select(floorUri => _httpClient.GetStreamAsync(floorUri));
                        var streamResponses = await Task.WhenAll(floorStreamTasks);

                        //取得したソースを解析
                        var floorAnalyseTasks = streamResponses.Select(response => _analysisHTML.AnalyseFloorAsync(response));
                        var slotMachineNumbersForSlotModels = await Task.WhenAll(floorAnalyseTasks);

                        //リストの平準化
                        slotMachineNumbers = slotMachineNumbersForSlotModels.SelectMany(x => x);
                        //slotMachineNumbers = Enumerable.Repeat(271, 1);
                    }
                    catch
                    {
                        //指定した台が無ければ次の店舗の検索をする
                        continue;
                    }
                }

                try
                {
                    //遊技台の情報があるURIを作成する
                    var month = $"{_dataDate.Month:D2}";
                    var day = $"{_dataDate.Day:D2}";
                    var slotDataUri = slotMachineNumbers
                        .Select(slotMachineNumber => new Uri(
                            $"{tempo.TenpoUri.ToString()}detail?unit={slotMachineNumber}&target_date={_dataDate.Year}-{month}-{day}"));
                    _slotDataUri.AddRange(slotDataUri);

                    //存在するURI内のソースを取得
                    var slotDataStreamTasks = _slotDataUri
                        .Select(uri => _httpClient.GetStreamAsync(uri));
                    var streamResponses = await Task.WhenAll(slotDataStreamTasks);

                    //取得したソースを解析
                    var slotDataAnalyseTasks =
                        streamResponses.Select((response, index)
                            => _analysisHTML.AnalyseAsync(response, tempo, _slotMachineSearchNames, _aimMachineList, _slotDataUri.ElementAt(index)));
                    var analysisSlotData = await Task.WhenAll(slotDataAnalyseTasks);

                    //解析したデータをコレクションに追加
                    SlotPlayDataCollection.AddRange(analysisSlotData.Where(x => x != null));

                    //狙い目の台をコレクションに追加
                    AimMachineCollection.AddRange(SlotPlayDataCollection.Where(x => x.Status != null));
                }
                catch (Exception e)
                {
                    var check = _floorSearchUri;
                    var check2 = _slotDataUri;

                    //他に何があるんやろうか
                    throw;
                }
            }
        }

        #endregion
    }
}
