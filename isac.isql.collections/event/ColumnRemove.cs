using System;
using System.Linq;
using System.Collections.Generic;

namespace Isac.Isql.Collections
{
	//ColumnDefination Level
    public sealed class ColumnRemoveEventArgs : EventArgs  
    {         
        public Column RemovedColumn;
    }
}