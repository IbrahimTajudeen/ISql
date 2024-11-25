using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Isac.Isql.Collections.Cell;

namespace Isac.Isql.Collections
{
    public class TreeMap
    {
        private List<Cell> map_list = new List<Cell>();
        public object this[int index]
        {
            get { return this.map_list[index]; }
        }
        
    }
}
