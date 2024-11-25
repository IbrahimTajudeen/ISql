using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Isac.Isql.Collections
{
    interface ITreeMap 
    {
        void AddChildNode(Cell parent, Cell child);
        void RemoveChild(Cell parent);
        Cell GetChild(int child_index);
        /// <summary>
        /// this should goto TreeMap
        /// </summary>
        /// <param name="chid_node"></param>
        /// <returns></returns>
        Cell GetParent(Cell chid_node);
        Cell GetFirstChild();
        Cell GetLastChild();

    }
}
