using Evolvex.EnemyLosses.Lib.Data;
using Evolvex.EnemyLosses.Lib.Misc;
using Evolvex.EnemyLosses.Lib.Parsers;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;

namespace Evolvex.EnemyLosses.CLI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Console.Read();
            string inputPath = args[0];
            string outputPath = args[1];
            bool categorizeLabels = args.Length > 2 && !string.IsNullOrWhiteSpace(args[2]) ? bool.Parse(args[2]) : false;
            bool monthly = args.Length > 3 && !string.IsNullOrWhiteSpace(args[3]) ? bool.Parse(args[3]) : false;
            var stats = LossesRawTextMinfinParser.Parse(File.ReadAllText(inputPath));

            LossesAnalyzer.CleanUpGarbase(stats);
            LossesAnalyzer.InduceFillNetDayLosses(stats);
            List<string> labels = LossesAnalyzer.ListLabelsDistinct(stats);
            if (monthly)
            {
                stats = GroupByMonth(stats, labels);
            }
            List<DateTime> dates = LossesAnalyzer.ListDatesDistinct(stats);
            DateTime minDt = dates.Min();
            DateTime mxDt = dates.Max();
            
            if (!categorizeLabels)
                Output(labels, outputPath, stats, minDt, mxDt);
            else
            {
                string outDir = Path.GetDirectoryName(outputPath);
                string fn = Path.GetFileNameWithoutExtension(outputPath);
                string ext = Path.GetExtension(outputPath);
                string json = Tools.ReadEmbeddedResource("Evolvex.EnemyLosses.Lib.Resources.MaterielCategories.json", typeof(Tools).Assembly);
                LossesCategorization categorization = JsonConvert.DeserializeObject<LossesCategorization>(json);

                foreach (string catKey in categorization.ScaleCategorization.Keys)
                {
                    List<string> currLbls = categorization.ScaleCategorization[catKey];
                    Output(currLbls, Path.Combine(outDir, $"{fn}.{catKey}{ext}"), stats, minDt, mxDt);
                }
            }
        }

        private static List<LossDayStats> GroupByMonth(List<LossDayStats> stats, List<string> labels)
        {
            List<LossDayStats> rslt = new List<LossDayStats>();
            var yrMoDistinct = stats.Select(s => s.Date).Select(d => int.Parse(d.ToString("yyyyMM"))).Distinct().OrderBy(ym => ym);
            foreach (int ym in yrMoDistinct)
            {
                var ymStr = ym.ToString();
                var currYMs = DateTime.Parse($"{ymStr.Substring(0, 4)}-{ymStr.Substring(4, 2)}-01");
                var currYMe = currYMs.AddMonths(1).AddDays(-1);
                var yrMoRecs = stats.Where(s => s.Date >= currYMs && s.Date <= currYMe).ToList();
                var currMoStats = new LossDayStats() { Date = currYMs, Records = new Dictionary<string, LossItem>() };
                labels.ForEach(l => {
                    var currLblSum = 0;
                    yrMoRecs.ForEach(rs => {
                        currLblSum += rs.Records.Where(r => r.Key == l).Select(r => r.Value.NetDayLosses ?? 0).ToList().Sum();
                    });
                    currMoStats.Records.Add(l, new LossItem() { ItemLabel = l, NetDayLosses = currLblSum });
                });
                rslt.Add(currMoStats);
            }
            return rslt;
        }

        private static void Output(List<string> labels, string outputPath, List<LossDayStats> stats, DateTime minDt, DateTime mxDt, bool monthly = false)
        {
            using (StreamWriter sw = new StreamWriter(outputPath, false, Encoding.Unicode))
            {
                sw.Write("Date");
                foreach (string label in labels)
                    sw.Write($"\t{label}");
                sw.WriteLine();
                for (DateTime currDt = minDt.Date; currDt.Date <= mxDt.Date; currDt = monthly ? currDt.AddMonths(1) : currDt.AddDays(1))
                {
                    LossDayStats lds = stats.FirstOrDefault(r => r.Date.Date == currDt.Date);
                    if (lds == null)
                        continue;
                    sw.Write(lds.Date.ToString("yyyy-MM-dd"));
                    foreach (string lbl in labels)
                    {
                        int? currNetLosses = lds.Records.ContainsKey(lbl) ? lds.Records[lbl].NetDayLosses : null;
                        sw.Write($"\t{currNetLosses}");
                    }
                    sw.WriteLine();
                }
            }

        }
    }
}