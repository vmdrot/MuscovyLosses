using Evolvex.EnemyLosses.Lib.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Evolvex.EnemyLosses.Lib.Parsers
{
    public static class LossesRawTextMinfinParser
    {
        private static readonly Regex _dateSplitter = new Regex("[\n\r]+[0-9]{2}\\.[0-9]{2}\\.[0-9]{4}[\n\r]+");
        private static readonly Regex _doubleCRLF = new Regex("[\n\r]{3,}");
        private static readonly Regex _snglCRLF = new Regex("[\n]{1}");
        private static readonly Regex _hyphen = new Regex("\\s[\\—\\-]{1}\\s");
        private static readonly Regex _accrualRgx = new Regex("[\\(]{0,1}[\\s]{0,}\\+{1}[0-9]+[\\s]{0,}[\\)]{0,1}");
        private static readonly Regex _nrRgx = new Regex("[0-9\\.\\,]+");
        private static readonly CultureInfo _dateFormat = CultureInfo.GetCultureInfoByIetfLanguageTag("uk-UA");
        public static List<Tuple<string,string>> SplitByDates(string totalRaw)
        {
            List<Tuple<string, string>> rslt = new List<Tuple<string, string>>();
            var pureContents = _dateSplitter.Split(totalRaw);
            var dates = _dateSplitter.Matches(totalRaw);
            int i = 0;
            foreach (var m in dates)
            {
                string currCont = pureContents[i + 1];
                var trimmed = _doubleCRLF.Split(currCont);
                rslt.Add(new Tuple<string, string>(m.ToString(), trimmed?.FirstOrDefault().ToString()));
                i++;
            }
            return rslt;
        }

        public static List<LossDayStats> Parse(string totalRaw)
        {
            var rslt = new List<LossDayStats>();
            var byDates = SplitByDates(totalRaw);
            foreach (var byDate in byDates)
            {
                string pureDateStr = byDate.Item1.Trim();
                if (string.IsNullOrWhiteSpace(pureDateStr))
                    continue;
                DateTime dt;
                if (!DateTime.TryParse(pureDateStr, _dateFormat.DateTimeFormat, DateTimeStyles.None, out dt))
                    continue;
                LossDayStats curr = new LossDayStats();
                curr.Date = dt;
                var rawRecs = _snglCRLF.Split(byDate.Item2);
                curr.Records = new Dictionary<string, LossItem>();
                foreach (var rr in rawRecs)
                {
                    string rrTrimmed = rr?.Trim();
                    if (string.IsNullOrWhiteSpace(rrTrimmed))
                        continue;
                    LossItem currRec = ParseItem(rrTrimmed);
                    curr.Records.Add(currRec.ItemLabel, currRec);
                }
                rslt.Add(curr);
            }
            return rslt;
        }

        public static LossItem ParseItem(string itemRaw)
        {
            string lbl = _hyphen.Split(itemRaw)[0];
            LossItem rslt = new LossItem() { OriginalRawText = itemRaw, ItemLabel = lbl };
            var accrualMatch =_accrualRgx.Match(itemRaw);
            string accrualStr = null;
            bool hasAccrual = false;
            if (accrualMatch.Success)
            {
                hasAccrual = true;
                accrualStr = accrualMatch.ToString();
            }

            if (hasAccrual)
            {
                int netDayLosses;
                var netDayLossesMatch = _nrRgx.Match(accrualStr);
                if (netDayLossesMatch.Success && int.TryParse(netDayLossesMatch.ToString(), out netDayLosses))
                    rslt.NetDayLosses = netDayLosses;
            }
            string cumLossesRaw = itemRaw.Replace(lbl, string.Empty);
            if (hasAccrual)
                cumLossesRaw.Replace(accrualStr, string.Empty);
            int cumLosses;
            var cumLossesMatch = _nrRgx.Match(cumLossesRaw);
            if (cumLossesMatch.Success && int.TryParse(cumLossesMatch.ToString(), out cumLosses))
                rslt.CumulativeLosses = cumLosses;
            return rslt;
        }

    }
}
