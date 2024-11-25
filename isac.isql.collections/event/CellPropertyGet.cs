using System;
using System.Linq;
using System.Collections.Generic;

namespace Isac.Isql.Collections
{
	//Cell Level
    public sealed class CellPropertyGetEventArgs : EventArgs  
    {         
        public dynamic PropertyValue;
        public string PropertyName;
    }
}