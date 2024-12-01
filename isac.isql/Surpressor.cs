using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Isac.Isql
{
    public static class Surpressor
    {
        private static List<Exception> logs = new List<Exception>();
        private static bool isSurpressed = false;
        internal static bool IsSurpressed
        {
            get { return isSurpressed; }
            set { isSurpressed = value; }
        }
        public static void Surpress() { IsSurpressed = true; }
        public static void Release(bool clearLogs = false)
        {
            if (clearLogs)
                ClearLog();
            IsSurpressed = false;
        }
        public static void ClearLog() { logs.Clear(); }
        internal static void Add(Exception ex) { logs.Add(ex); }
    }
}
