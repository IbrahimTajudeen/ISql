using System;
using System.Linq;
using System.Collections.Generic;

namespace Isac.Isql.Configurations
{
    public class TableConfiguaration 
    {         
        private bool isReadOnly = false;
        private bool isWriteOnly = false;
        private bool isReadWrite = true;
        private bool allowColumnModification = true;
        private bool allowRowModification = true;
        private bool rowLock = false;	//nothing can enter the column Defination
        private bool columnLock = false;//nothing can enter the row defination
        private bool isLocked = false;//nothing can enter the table
        private bool allowVirtualManipulation = true;
        private User createdBy;
        internal string CreationType = "NaN";
        private DateTime creationTime = DateTime.Now;
        
        public bool AllowVirtualManipulation
        {
        	get	{ return allowVirtualManipulation; }
        	set 
        	{ 
        		allowVirtualManipulation = value;
        	}
        }
        
        public DateTime CreationTime
        {
        	get { return creationTime; } 
        }
        
        public User CreatedBy
        {
        	get { return createdBy; }
        }
        
        public bool LockTable
        {
        	get { return isLocked; }
        	set
        	{
        		isLocked = value;
        	}
        }
        
        public bool LockColumn
        {
        	get { return columnLock; }
        	set
        	{
        		columnLock = value;
        	}
        }
        
        public bool LockRow
        {
        	get { return rowLock; }
        	set	
        	{
        		rowLock = value;
        	}
        }
        
        public bool AllowRowEdit
        {
        	get { return allowRowModification; }
        	set
        	{
        		allowRowModification = value;
        	}
        }
        
        public bool AllowColumnEdit
        {
        	get	{ return allowColumnModification; }
        	set
        	{
        		allowColumnModification = value; 
        	}
        }
        
        public bool IsReadOnly
        {
        	get { return isReadOnly; }
        	set 
        	{
        		isReadOnly = value;
        		isWriteOnly = !value;
        		isReadWrite = false;
        	}
        }
        
        public bool IsWriteOnly
        {
        	get { return isWriteOnly; }
        	set 
        	{
        		isWriteOnly = value;
        		isReadOnly = !value;
        		isReadWrite = false;
        	}
        }
        
        public bool IsReadWrite
        {
        	get { return isReadWrite; }
        	set 
        	{
        		isReadWrite = value;
        		isReadOnly = (value == true) ? !value : isReadOnly;
        		isWriteOnly = (value == true) ? !value : isWriteOnly;
        	}
        }
        
        
        //private 
        //private bool 
    }
}







