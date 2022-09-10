using Evolvex.EnemyLosses.Lib.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
