using System;
using System.Linq;
using System.Collections.Generic;

namespace Isac.Isql.Collections
{
	//Row Level
    public sealed class RowModifyEventArgs : EventArgs  
    {   
    	public Row OldData;
    	public Row NewData;
        public Cell IncomingCell;
        public int ModifyPoint;
    }
}