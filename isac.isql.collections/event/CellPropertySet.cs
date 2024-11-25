using System;
using System.Linq;
using System.Collections.Generic;

namespace Isac.Isql.Collections
{
	//Cell Level
    public sealed class CellPropertySetEventArgs : EventArgs  
    {         
        public dynamic OldValue;
        public dynamic NewValue;
        public string PropertyName;
    }
}