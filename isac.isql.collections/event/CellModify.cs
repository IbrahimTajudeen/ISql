using System;
using System.Linq;
using System.Collections.Generic;

namespace Isac.Isql.Collections
{
	//Cell Level
    public sealed class CellModifyEventArgs : EventArgs  
    {         
        public object OldData;
        public object NewData;
        public string ModifyName;
    }
}