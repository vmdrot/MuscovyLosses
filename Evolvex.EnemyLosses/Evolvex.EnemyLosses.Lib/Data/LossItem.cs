using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolvex.EnemyLosses.Lib.Data
{
    public class LossItem
    {
        public string ItemLabel { get; set; }
        public int? CumulativeLosses { get; set; }
        public int? NetDayLosses { get; set; }
        public string OriginalRawText { get; set; }
    }
}
