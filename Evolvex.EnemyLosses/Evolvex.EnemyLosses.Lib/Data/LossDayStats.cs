using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolvex.EnemyLosses.Lib.Data
{
    public class LossDayStats
    {
        public DateTime Date { get; set; }
        public Dictionary<string,LossItem> Records { get; set; }
    }
}
