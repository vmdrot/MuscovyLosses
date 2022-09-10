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
            Console.Read();
            string inputPath = args[0];
            string outputPath = args[1];
            string labelCategories = args.Length > 2 && !string.IsNullOrWhiteSpace(args[2]) ? args[2] : string.Empty;
            var stats = LossesRawTextMinfinParser.Parse(File.ReadAllText(inputPath));
            LossesAnalyzer.CleanUpGarbase(stats);
            LossesAnalyzer.InduceFillNetDayLosses(stats);
            List<DateTime> dates = LossesAnalyzer.ListDatesDistinct(stats);
            DateTime minDt = dates.Min();
            DateTime mxDt = dates.Max();
            List<string> labels = LossesAnalyzer.ListLabelsDistinct(stats);
            if (string.IsNullOrWhiteSpace(labelCategories))
                Output(labels, outputPath, stats, minDt, mxDt);
            else
            {
                string outDir = Path.GetDirectoryName(outputPath);
                string fn = Path.GetFileNameWithoutExtension(outputPath);
                string ext = Path.GetExtension(outputPath);
                string[] cats = labelCategories.Split(';');
                for (int i = 0; i < cats.Length; i++)
                {
                    List<string> currLbls = new List<string>(cats[i].Split(','));
                    Output(currLbls, Path.Combine(outDir, $"{fn}.{i}{ext}"), stats, minDt, mxDt);
                }
            }
        }

        private static void Output(List<string> labels, string outputPath, List<LossDayStats> stats, DateTime minDt, DateTime mxDt)
        {
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