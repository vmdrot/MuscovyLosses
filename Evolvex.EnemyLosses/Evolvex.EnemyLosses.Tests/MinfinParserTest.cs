using Evolvex.EnemyLosses.Lib.Data;
using Evolvex.EnemyLosses.Lib.Misc;
using Evolvex.EnemyLosses.Lib.Parsers;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Evolvex.EnemyLosses.Tests
{
    [TestClass]
    public class MinfinParserTest
    {
        private static readonly string _inputPath = @"..\..\..\..\..\Evolvex.EnemyLosses\SampleData\MinfinEnemyCasualtiesSample.txt";
        [TestMethod]
        public void DateSplitter()
        {
            var rslts = LossesRawTextMinfinParser.SplitByDates(File.ReadAllText(_inputPath));
            Trace.WriteLine(rslts?.Count);
            if (rslts?.Count >= 3)
            {
                Trace.WriteLine(rslts[2].Item1);
                Trace.WriteLine(rslts[2].Item2);
            }
        }

        [TestMethod]
        public void RoughParse()
        {
            var rslts = LossesRawTextMinfinParser.Parse(File.ReadAllText(_inputPath));
            Trace.WriteLine(rslts?.Count);
            if (rslts?.Count >= 3)
            {
                Trace.WriteLine(JsonConvert.SerializeObject(rslts[2], Formatting.Indented));
            }
        }

        [TestMethod]
        public void DetectMissingDates()
        {
            var rslts = LossesRawTextMinfinParser.Parse(File.ReadAllText(_inputPath));
            List<DateTime> dates = rslts.Select(r => r.Date).Distinct().ToList();
            DateTime minDt = dates.Min();
            Trace.WriteLine($"{nameof(minDt)}: {minDt:s}");
            DateTime mxDt = dates.Max();
            Trace.WriteLine($"{nameof(mxDt)}: {mxDt:s}");
            Trace.WriteLine($"{nameof(rslts)}.{nameof(rslts.Count)}: {rslts.Count}");
            TimeSpan tsDiff = mxDt - minDt;
            Trace.WriteLine($"{nameof(tsDiff)}: {tsDiff.Days+1}");
            List<DateTime> missingDays = new List<DateTime>();
            for (DateTime currDt = minDt.Date; currDt.Date <= mxDt.Date; currDt = currDt.AddDays(1))
            {
                LossDayStats lds = rslts.FirstOrDefault(r => r.Date.Date == currDt.Date);
                if (lds == null)
                    missingDays.Add(currDt.Date);
            }

            Trace.WriteLine($"{nameof(missingDays)}: {missingDays.Count}");
            Trace.WriteLine(String.Join('\n', missingDays));
        }

        [TestMethod]
        public void ParsePrintAll()
        {
            var rslts = LossesRawTextMinfinParser.Parse(File.ReadAllText(_inputPath));
            File.WriteAllText($"{_inputPath}.parsed.json", JsonConvert.SerializeObject(rslts, Formatting.Indented));
        }

        [TestMethod]
        public void ParseInduceNetsPrintAll()
        {
            var rslts = LossesRawTextMinfinParser.Parse(File.ReadAllText(_inputPath));
            LossesAnalyzer.CleanUpGarbase(rslts);
            LossesAnalyzer.InduceFillNetDayLosses(rslts);
            File.WriteAllText($"{_inputPath}.netsfilled.json", JsonConvert.SerializeObject(rslts, Formatting.Indented));
        }

        [TestMethod]
        public void TestDetectFalseLabels()
        {
            var rslts = LossesRawTextMinfinParser.Parse(File.ReadAllText(_inputPath));
            LossesAnalyzer.InduceFillNetDayLosses(rslts);
            var falseLabels = LossesAnalyzer.DetectFalseLabels(rslts);
            Trace.WriteLine(String.Join("\n", falseLabels));
        }

        [TestMethod]
        public void TestAggregateStats()
        {
            var rslts = LossesRawTextMinfinParser.Parse(File.ReadAllText(_inputPath));
            LossesAnalyzer.CleanUpGarbase(rslts);
            LossesAnalyzer.InduceFillNetDayLosses(rslts);
            var aggrs = LossesAnalyzer.AggregateStatsByLabel(rslts);
            //Trace.WriteLine(JsonConvert.SerializeObject(aggrs, Formatting.Indented));
            List<Tuple<string, int, int, int>> sortedStats = new List<Tuple<string, int, int, int>>();
            foreach (string lbl in aggrs.Keys)
            {
                sortedStats.Add(new Tuple<string, int, int, int>(lbl, aggrs[lbl].Item1, aggrs[lbl].Item2, aggrs[lbl].Item3));
            }
            Trace.WriteLine(JsonConvert.SerializeObject(sortedStats.OrderByDescending(t => t.Item4).ToList(), Formatting.Indented));
        }
    }
}