using System;
using System.Linq;
using System.Collections.Generic;

namespace Isac.Isql.Collections
{
	//RowCollection Level
    public sealed class RowRemoveEventArgs : EventArgs  
    {         
        public Row RemovedRow;
        public int RemovedIndex;
    }
}