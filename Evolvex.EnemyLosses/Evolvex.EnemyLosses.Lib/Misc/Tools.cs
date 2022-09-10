using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Evolvex.EnemyLosses.Lib.Misc
{
    public static class Tools
    {
        public static string ReadEmbeddedResource(string resourceFilePathName, Assembly asmb = null)
        {
            Assembly assembly = asmb;
            if (assembly == null)
                assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceFilePathName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
