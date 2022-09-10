using Evolvex.EnemyLosses.Lib.Data;
using Evolvex.EnemyLosses.Lib.Misc;
using Evolvex.EnemyLosses.Lib.Parsers;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;

namespace Evolvex.EnemyLosses.CLI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string inputPath = args[0];
            string outputPath = args[1];
            var stats = LossesRawTextMinfinParser.Parse(File.ReadAllText(inputPath));
            LossesAnalyzer.CleanUpGarbase(stats);
            LossesAnalyzer.InduceFillNetDayLosses(stats);
            List<DateTime> dates = LossesAnalyzer.ListDatesDistinct(stats);
            DateTime minDt = dates.Min();
            DateTime mxDt = dates.Max();
            List<string> labels = LossesAnalyzer.ListLabelsDistinct(stats);
            using (StreamWriter sw = new StreamWriter(outputPath, false, Encoding.Unicode))
            {
                sw.Write("Date");
                foreach (string label in labels)
                    sw.Write($"\t{label}");
                sw.WriteLine();
                for (DateTime currDt = minDt.Date; currDt.Date <= mxDt.Date; currDt = currDt.AddDays(1))
                {
                    LossDayStats lds = stats.FirstOrDefault(r => r.Date.Date == currDt.Date);
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