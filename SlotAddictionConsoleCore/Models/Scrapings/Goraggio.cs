using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Parser.Html;
using SlotAddictionCore.Const;

namespace SlotAddictionCore.Models.Scrapings
{
    /// <summary>
    /// Gorragioの解析クラス
    /// </summary>
    public class Goraggio
    {
        #region フィールド
        /// <summary>
        /// 
        /// </summary>
        private readonly HtmlParser _htmlParser = new HtmlParser();
        /// <summary>
        /// 
        /// </summary>
        private bool _hasARTData = true;
        #endregion

        #region 定数

        private const string MaxPayoutLabel = "最大持玉";
        private const string TotalCountLabel = "累計スタート";
        private const string FinalStartCountYesterDayLabel = "前日最終スタート";
        private const string CombinedWinningRateLabel = "合成確率";
        private const string BigBonusWinningRateLabel = "BB確率";
        private const string RegularBonusWinningRateLabel = "RB確率";

        #endregion
        #region メソッド
        /// <summary>
        /// HTMLを解析します。
        /// </summary>
        /// <param name="html">解析したいHTMLを指定します。</param>
        /// <param name="tenpoInfo"></param>
        /// <param name="slotMachineSearchNames"></param>
        /// <param name="aimMachineList"></param>
        /// <param name="slotMachineUri"></param>
        /// <returns></returns>
        public async Task<SlotPlayData> AnalyseAsync(Stream html, TenpoInfo tenpoInfo,
            List<string> slotMachineSearchNames, List<AimMachineInfo> aimMachineList, Uri slotMachineUri)
        {
            if (html == null) return null;

            // HTMLをAngleSharp.Parser.Html.HtmlParserオブジェクトパースさせる
            var doc = await _htmlParser.ParseAsync(html);

            //Bodyを取得
            var body = doc.QuerySelector("#Main-Contents");

            //台名を取得
            var id_pachinkoTi = body.QuerySelector("#pachinkoTi");
            var slotMachineTitle = id_pachinkoTi.QuerySelector("strong").TextContent.TrimEnd();

            //以下のように取得される
            //(20円スロット                            | 1番台)
            var coinPriceAndMachineNO = id_pachinkoTi.QuerySelector("span").TextContent;
            var coinPriceAndMachineNOFormat = coinPriceAndMachineNO.Substring(1, coinPriceAndMachineNO.Length - 2).Split('|');
            for (var i = 0; i < coinPriceAndMachineNOFormat.Length; ++i)
            {
                coinPriceAndMachineNOFormat[i] = coinPriceAndMachineNOFormat.ElementAt(i).Trim();
            }

            //コイン単価を取得する
            var coinPricePosition = coinPriceAndMachineNOFormat.First().IndexOf('円');
            var coinPrice = string.Empty;

            if (coinPricePosition < 0)
            {
                //なぜか1枚単価の金額ではなく 47枚スロット などとなっている。
                coinPricePosition = coinPriceAndMachineNOFormat.First().IndexOf('枚');
                coinPrice = coinPriceAndMachineNOFormat.First().Substring(0, coinPricePosition);
                coinPrice = (Math.Round((double)(1000 / int.Parse(coinPrice)), 1, MidpointRounding.AwayFromZero)).ToString();
            }
            else
            {
                coinPrice = coinPriceAndMachineNOFormat.First().Substring(0, coinPricePosition);
            }

            //台番号を取得する
            var machineNOPosition = coinPriceAndMachineNOFormat.ElementAt(1).IndexOf('番');
            var machineNO = coinPriceAndMachineNOFormat.ElementAt(1).Substring(0, machineNOPosition);

            //最新更新日時を取得する
            var latestClass = body.QuerySelector(".latest");
            var latest = (latestClass == null) ? body.QuerySelector(".older").TextContent : latestClass.TextContent;

            //サブ情報を取得する
            var subInfoDictionary = new Dictionary<string, object>
            {
                { MaxPayoutLabel,0},
                { TotalCountLabel,0},
                { FinalStartCountYesterDayLabel,0},
                { CombinedWinningRateLabel,string.Empty},
                { BigBonusWinningRateLabel,string.Empty},
                { RegularBonusWinningRateLabel,string.Empty},
            };

            var class_col2Right_overviewTable3 = body.QuerySelector(".col2Right .overviewTable3");
            var subInfo = Regex.Replace(class_col2Right_overviewTable3.TextContent, " ", string.Empty).Split('\n').Where(x => x != string.Empty);
            var maxPayoutLabel = subInfo.ElementAtOrDefault(0);
            var maxPayout = subInfo.ElementAtOrDefault(1);
            var totalCountLabel = subInfo.ElementAtOrDefault(2);
            var totalCount = subInfo.ElementAtOrDefault(3);
            var finalStartCountYesterDayLabel = subInfo.ElementAtOrDefault(4);
            var finalStartCountYesterDay = subInfo.ElementAtOrDefault(5);
            var combinedWinningRateLabel = subInfo.ElementAtOrDefault(6);
            var combinedWinningRate = subInfo.ElementAtOrDefault(7);
            var bigBonusWinningRateLabel = subInfo.ElementAtOrDefault(8);
            var bigBonusWinningRate = subInfo.ElementAtOrDefault(9);
            var regularBonusWinningRateLabel = subInfo.ElementAtOrDefault(10);
            var regularBonusWinningRate = subInfo.ElementAtOrDefault(11);


            if (maxPayoutLabel != null)
            {
                subInfoDictionary[maxPayoutLabel] = maxPayout;
            }
            if (totalCountLabel != null)
            {
                subInfoDictionary[totalCountLabel] = totalCount;
            }
            if (finalStartCountYesterDayLabel != null)
            {
                subInfoDictionary[finalStartCountYesterDayLabel] = finalStartCountYesterDay;
            }
            if (combinedWinningRateLabel != null)
            {
                subInfoDictionary[combinedWinningRateLabel] = combinedWinningRate;
            }
            if (bigBonusWinningRateLabel != null)
            {
                subInfoDictionary[bigBonusWinningRateLabel] = bigBonusWinningRate;
            }
            if (regularBonusWinningRateLabel != null)
            {
                subInfoDictionary[regularBonusWinningRateLabel] = regularBonusWinningRate;
            }

            //各回数を取得する
            var class_TextBig25 = body.QuerySelector(".Text-Big25");
            if (class_TextBig25 == null) return null;
            var bbCount = class_TextBig25.TextContent;
            var textBig19classes = body.QuerySelectorAll(".Text-Big19");
            var rbCount = textBig19classes?.First().TextContent;
            var artCount = textBig19classes.ElementAt(1).TextContent;

            //BB/RB/スタート回数しかない場合を考慮
            var startCount = "0";
            try
            {
                startCount = textBig19classes.ElementAt(2).TextContent;
            }
            catch
            {
                startCount = artCount;
                artCount = "0";
                _hasARTData = false;
            }

            var slotPlayData = new SlotPlayData
            {
                StoreName = tenpoInfo.TenpoName,
                Title = slotMachineTitle,
                CoinPrice = Convert.ToDecimal(coinPrice),
                MachineNO = Convert.ToInt32(machineNO),
                LatestUpdateDatetime = latest,
                MaxPayout = int.TryParse(subInfoDictionary[MaxPayoutLabel].ToString(), out var number) ? number : 0,
                TotalCount = int.TryParse(subInfoDictionary[TotalCountLabel].ToString(), out number) ? number : 0,
                FinalStartCountYesterDay = int.TryParse(subInfoDictionary[FinalStartCountYesterDayLabel].ToString(), out number) ? number : 0,
                CombinedWinningRate = subInfoDictionary[CombinedWinningRateLabel].ToString(),
                BigBonusWinningRate = subInfoDictionary[BigBonusWinningRateLabel].ToString(),
                RegularBonusWinningRate = subInfoDictionary[RegularBonusWinningRateLabel].ToString(),
                BigBonusCount = Convert.ToInt32(bbCount),
                RegulerBonusCount = Convert.ToInt32(rbCount),
                ARTCount = Convert.ToInt32(artCount),
                StartCount = Convert.ToInt32(startCount),
                SlotMachineUri = slotMachineUri,
            };

            slotPlayData.CombinedWinningRateDenominator = double.Parse(slotPlayData.CombinedWinningRate.Split('/')[1]);
            slotPlayData.BigBonusWinningRateDenominator = double.Parse(slotPlayData.BigBonusWinningRate.Split('/')[1]);
            slotPlayData.RegularBonusWinningRateDenominator = double.Parse(slotPlayData.RegularBonusWinningRate.Split('/')[1]);
            var winningHistory = new List<WinningHistoryDetail>();

            //狙い目がメンテされていれば大当たり履歴を取得する
            if (aimMachineList != null
                && aimMachineList.Any())
            {
                //大当たり履歴が格納されているnumericValueTableクラスを取得
                var class_numericValueTable = doc.QuerySelector(".numericValueTable");

                //大当たり履歴を公開してくれているかを取得
                slotPlayData.IsPublicWinningHistory = class_numericValueTable != null;

                //大当たり履歴を見せてくれないゴミ店が存在した場合を考慮
                if (!slotPlayData.IsPublicWinningHistory)
                {
                    //狙い目にマッチするか確認
                    //大当たり履歴がなくても規定G数以上だけを条件とする台であればマッチする可能性がある
                    foreach (var aimMachine in aimMachineList)
                    {
                        //期待値
                        slotPlayData.ExceptValue = aimMachine.ExpectedValue;

                        //大当たり履歴を必要とする狙い目であれば検索できないので除外する
                        if (aimMachine.WinningList != null
                            && aimMachine.WinningList.Any())
                        {
                            continue;
                        }

                        //抽出条件にあう場合はステータスを更新する
                        if (CheckConditionsSlotPlayData(aimMachine, slotPlayData))
                        {
                            slotPlayData.Status = aimMachine.Memo;
                            return slotPlayData;
                        }
                    }
                }
                else
                {
                    //大当たり履歴を格納
                    var trTagChildTdTags = class_numericValueTable.QuerySelectorAll("tr td");

                    //大当たり履歴の有無を確認
                    slotPlayData.IsPublicWinningHistory = trTagChildTdTags.Any();

                    for (var i = 0; i < trTagChildTdTags.Length; i += 5)
                    {
                        var hitCount = trTagChildTdTags[i].TextContent;
                        var hitStart = Convert.ToInt32(trTagChildTdTags[i + 1].TextContent);
                        var hitPayout = Convert.ToInt32(trTagChildTdTags[i + 2].TextContent);
                        var winingType = (WinningType)Enum.Parse(typeof(WinningType), trTagChildTdTags[i + 3].TextContent);
                        var hitTime = trTagChildTdTags[i + 4].TextContent;
                        winningHistory.Add(new WinningHistoryDetail
                        {
                            HitCount = hitCount,
                            HitStart = hitStart,
                            HitPayout = hitPayout,
                            WinningType = winingType,
                            HitTime = hitTime,
                        });
                    }

                    //当選履歴
                    slotPlayData.WinningHistory = winningHistory;

                    //狙い目にマッチするか確認
                    foreach (var aimMachine in aimMachineList)
                    {
                        //当選履歴の回数が狙い目を判断できる回数未満であれば比較できない
                        if (aimMachine.WinningList.Count > winningHistory.Count) continue;

                        //抽出条件にあわない場合は次の狙い目と比較
                        if (!CheckConditionsSlotPlayData(aimMachine, slotPlayData)) continue;

                        //期待値のセット
                        slotPlayData.ExceptValue = aimMachine.ExpectedValue;

                        var winningHistorySlice = winningHistory.Select(x => x.WinningType).Take(aimMachine.WinningList.Count).ToList();
                        var isAimMachine = aimMachine.WinningList.Select((x, index) => x & winningHistorySlice[index]).All(x => x != WinningType.None);
                        if (isAimMachine)
                        {
                            slotPlayData.Status = aimMachine.Memo;
                            return slotPlayData;
                        }
                    }
                }
            }

            return slotPlayData;
        }
        /// <summary>
        /// 指定した機種が何番から何番まで置いてあるのかを取得する
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public async Task<HashSet<int>> AnalyseFloorAsync(Stream html)
        {
            if (html == null) return null;

            //指定した機種が存在する台番号を格納するリストを作成
            var slotModelUnits = new HashSet<int>();

            // HTMLをAngleSharp.Parser.Html.HtmlParserオブジェクトパースさせる
            var doc = await _htmlParser.ParseAsync(html);

            //店内の機種一覧を取得
            var classes_TextUnderLine = doc.QuerySelectorAll(".Text-UnderLine");

            foreach (var class_TextUnderLine in classes_TextUnderLine)
            {
                var isNumber = int.TryParse(class_TextUnderLine.TextContent, out var number);
                if (isNumber)
                {
                    slotModelUnits.Add(number);
                }
            }

            return slotModelUnits;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="aimMachine"></param>
        /// <param name="slotPlayData"></param>
        /// <returns></returns>
        private bool CheckConditionsSlotPlayData(AimMachineInfo aimMachine, SlotPlayData slotPlayData)
        {
            var winningCount = slotPlayData.BigBonusCount + slotPlayData.RegulerBonusCount + slotPlayData.ARTCount;
            var hasWon = winningCount > 0;
            var firstWinningData = new WinningHistoryDetail();

            //共通チェック
            if (aimMachine.ProvisionOvernightCount > 0
                || aimMachine.FirstWinningCount > 0)
            {
                //何かしら当選している場合
                if (hasWon)
                {
                    //取得したデータに当選履歴のデータがない場合の処理
                    if (slotPlayData.WinningHistory == null
                        || !slotPlayData.WinningHistory.Any())
                    {
                        return false;
                    }

                    //取得したデータに当選履歴のデータがない場合は初当たりの情報を取得
                    firstWinningData = slotPlayData.WinningHistory.Last();
                }
            }

            //宵越しG数のチェック
            if (aimMachine.ProvisionOvernightCount > 0)
            {
                //初回当選時のG数 + 前日最終スタート数で宵越しG数を求める
                var overnightCount = slotPlayData.FinalStartCountYesterDay + firstWinningData.HitStart;
                if (overnightCount < aimMachine.ProvisionOvernightCount)
                {
                    return false;
                }
            }

            //初回当選G数のチェック
            if (aimMachine.FirstWinningCount > 0)
            {
                if (hasWon)
                {
                    if (firstWinningData.HitStart < aimMachine.FirstWinningCount)
                    {
                        return false;
                    }
                }
                else
                {
                    if (slotPlayData.StartCount < aimMachine.FirstWinningCount)
                    {
                        return false;
                    }
                }
            }

            var aimProvisionWinningCountFrom = aimMachine.ProvisionWinningCountFrom;
            var aimProvisionWinningCountTo = aimMachine.ProvisionWinningCountTo;
            var aimCombineWinninRateFrom = aimMachine.WinningRateInfo.CombineFrom;
            var aimCombineWinninRateTo = aimMachine.WinningRateInfo.CombineTo;
            var aimBigBonusWinningRate = aimMachine.WinningRateInfo.BigBonus;
            var aimRegularBonusWinninRate = aimMachine.WinningRateInfo.RegularBonus;

            //スタートカウントと総回転数が規定G数以上
            if (slotPlayData.CoinPrice >= 20
                && aimMachine.ProvisionStartCount <= slotPlayData.StartCount
                && aimMachine.ProvisionTotalCount <= slotPlayData.TotalCount
                && (aimProvisionWinningCountFrom == null
                    || (aimProvisionWinningCountFrom != null && aimProvisionWinningCountFrom <= winningCount))
                && (aimProvisionWinningCountTo == null
                    || (aimProvisionWinningCountTo != null && aimProvisionWinningCountTo >= winningCount))
                && (aimCombineWinninRateFrom == null
                    || (aimCombineWinninRateFrom != null && aimCombineWinninRateFrom <= slotPlayData.CombinedWinningRateDenominator))
                && (aimCombineWinninRateTo == null
                    || (aimCombineWinninRateTo != null && aimCombineWinninRateTo >= slotPlayData.CombinedWinningRateDenominator))
                && (aimBigBonusWinningRate == null
                    || (aimBigBonusWinningRate != null && aimBigBonusWinningRate >= slotPlayData.BigBonusWinningRateDenominator))
                && (aimRegularBonusWinninRate == null
                    || (aimRegularBonusWinninRate != null && aimRegularBonusWinninRate >= slotPlayData.RegularBonusWinningRateDenominator))
                )
            {
                //当選履歴間の対象が設定されている場合の処理
                if (aimMachine.BetweenWinningType != WinningType.None)
                {
                    //何かしら当たっているかつARTデータが取得できない店舗かつ当選履歴間の対象がARTの場合は判断がつかない為
                    if (hasWon
                        && !_hasARTData
                        && aimMachine.BetweenWinningType == WinningType.ART)
                    {
                        return false;
                    }
                    //何かしら当たっている場合
                    else if (hasWon)
                    {
                        //当選履歴がない場合
                        if (slotPlayData.WinningHistory == null
                            || !slotPlayData.WinningHistory.Any())
                        {
                            //判断がつかない為
                            return false;
                        }
                        else
                        {
                            var betweenCount = slotPlayData.StartCount
                                               + slotPlayData.WinningHistory.TakeWhile(x => (x.WinningType & aimMachine.BetweenWinningType) == WinningType.None).Sum(x => x.HitStart);
                            return betweenCount >= aimMachine.BetweenWinningProvisionCount;
                        }
                    }
                    else
                    {
                        //一度も当選していなければ現在の回転数が規定当選履歴間G数以上であれば正常にチェックを抜ける
                        return slotPlayData.StartCount >= aimMachine.BetweenWinningProvisionCount;
                    }
                }

                return true;
            }

            return false;
        }

        #endregion
    }
}
