using Evolvex.EnemyLosses.Lib.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Evolvex.EnemyLosses.Lib.Misc
{
    public static class LossesAnalyzer
    {
        public static void InduceFillNetDayLosses(List<EnemyLosses.Lib.Data.LossDayStats> subj)
        {
            List<DateTime> dates = subj.Select(r => r.Date).Distinct().ToList();
            DateTime minDt = dates.Min();
            DateTime mxDt = dates.Max();
            for (DateTime currDt = minDt.Date; currDt.Date <= mxDt.Date; currDt = currDt.AddDays(1))
            {
                LossDayStats lds = subj.FirstOrDefault(r => r.Date.Date == currDt.Date);
                if (lds == null)
                    continue;
                foreach (var currLbl in lds.Records.Keys)
                {
                    var item = lds.Records[currLbl];
                    if (item.NetDayLosses == null)
                    {
                        if (currDt.Date == minDt.Date)
                            item.NetDayLosses = item.CumulativeLosses;
                        else
                        {
                            var allPrevStats = subj.Where(s => s.Date.Date < lds.Date.Date && s.Records.ContainsKey(currLbl) && s.Records[currLbl].CumulativeLosses != null);
                            if (!allPrevStats.Any())
                            {
                                item.NetDayLosses = item.CumulativeLosses;
                                continue;
                            }
                            DateTime prevDt = allPrevStats.Select(s => s.Date.Date).Max();
                            int prevCumLosses = (int)allPrevStats.FirstOrDefault(s => s.Date.Date == prevDt.Date).Records[currLbl].CumulativeLosses;
                            item.NetDayLosses = (int)item.CumulativeLosses - prevCumLosses;
                        }
                    }
                }
            }
        }


        public static List<string> ListLabelsDistinct(List<EnemyLosses.Lib.Data.LossDayStats> src)
        {
            List<string> labelsNonDistinct = new List<string>();
            src.ForEach(s =>
            {
                foreach (string key in s.Records.Keys)
                    labelsNonDistinct.Add(key);
            }
            );
            return labelsNonDistinct.Distinct().ToList();
        }

        public static List<DateTime> ListDatesDistinct(List<EnemyLosses.Lib.Data.LossDayStats> src)
        {
            return src.Select(r => r.Date.Date).Distinct().OrderBy(d => d).ToList();
        }


        public static List<string> DetectFalseLabels(List<EnemyLosses.Lib.Data.LossDayStats> src)
        {
            var lbls = ListLabelsDistinct(src);
            List<string> falseLabels = new List<string>();
            foreach (string lbl in lbls)
            {
                var cumLossesDistinct = src.Where(s => s.Records.ContainsKey(lbl)).Select(s => s.Records[lbl].CumulativeLosses).Distinct();
                if (cumLossesDistinct.Any() && cumLossesDistinct.Count() > 1)
                    continue;
                var val = cumLossesDistinct.FirstOrDefault();
                if (val == 0 || val == null)
                    falseLabels.Add(lbl);
            }
            return falseLabels;
        }
        public static void CleanUpGarbase(List<EnemyLosses.Lib.Data.LossDayStats> subj)
        {
            var falseLbls = DetectFalseLabels(subj);
            foreach (var stat in subj)
            {
                foreach(string lbl in falseLbls)
                {
                    if (stat.Records.ContainsKey(lbl))
                        stat.Records.Remove(lbl);
                }
            }
        }

        public static List<Tuple<int, int>> Categorize(List<EnemyLosses.Lib.Data.LossDayStats> subj)
        {
            var labels = ListLabelsDistinct(subj);

            List<Tuple<int, int>> rslt = new List<Tuple<int, int>>();

            Dictionary<string, int> avgs = new Dictionary<string, int>();

            //todo

            return rslt;
        }
    }
}
