using System;
using System.Linq;
using System.Collections.Generic;

namespace Isac.Isql.Logistics
{
    internal class PortEvent : EventArgs
    {         
        public List<dynamic> Data = new List<dynamic>();
        public List<string> Tree = new List<string>();
        public Dictionary<int, dynamic[]> Ports = new Dictionary<int, dynamic[]>();
        public Collections.DataTable DTable = null;
    }
}
