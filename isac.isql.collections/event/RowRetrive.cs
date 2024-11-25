using System;
using System.Linq;
using System.Collections.Generic;

namespace Isac.Isql.Collections
{
	//RowCollection Level
    public sealed class RowGetEventArgs : EventArgs  
    {         
        public Row RowGet;
        public int IndexGet;
    }
}