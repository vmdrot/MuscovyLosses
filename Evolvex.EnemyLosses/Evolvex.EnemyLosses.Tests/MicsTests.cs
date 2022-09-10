using Evolvex.EnemyLosses.Lib.Data;
using Evolvex.EnemyLosses.Lib.Misc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolvex.EnemyLosses.Tests
{
    [TestClass]
    public class MicsTests
    {
        [TestMethod]
        public void ReadCasualtiesCategorizationConfigTest()
        {
            string json = Tools.ReadEmbeddedResource("Evolvex.EnemyLosses.Lib.Resources.MaterielCategories.json", typeof(Tools).Assembly);
            LossesCategorization categorization = JsonConvert.DeserializeObject<LossesCategorization>(json);
            Assert.IsNotNull(categorization);
            Assert.IsNotNull(categorization.ScaleCategorization);
            Assert.IsNotNull(categorization.MaterielTypeCategorization);
            Assert.AreNotEqual(0, categorization.ScaleCategorization.Count);
            Assert.AreNotEqual(0, categorization.MaterielTypeCategorization.Count);
            Trace.WriteLine(JsonConvert.SerializeObject(categorization, Formatting.Indented));
        }
    }
}
