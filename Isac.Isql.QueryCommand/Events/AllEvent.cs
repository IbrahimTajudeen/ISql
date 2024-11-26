using System;
using System.Linq;
using System.Collections.Generic;

using System.Text;

namespace Isac.Isql.QueryCommand
{
	#region SELECT
    public class SelectStartEventArgs : EventArgs 
    {         
        internal string user = "";
    	internal Encoding charSet;
    	internal DateTime time;
    	
    	public string UserName
    	{
    		get { return user; }
    	}
    	
    	public Encoding CharSet
    	{
    		get { return charSet; }
    	}
    	
    	public DateTime Time
    	{
    		get { return time; }
    	}
    }
    
    public class SelectingEventArgs : EventArgs 
    {         
        internal string user = "";
    	internal Encoding charSet;
    	internal DateTime time;
    	
    	public string UserName
    	{
    		get { return user; }
    	}
    	
    	public Encoding CharSet
    	{
    		get { return charSet; }
    	}
    	
    	public DateTime Time
    	{
    		get { return time; }
    	}
    }
    
    public class SelectEndEventArgs : EventArgs 
    {         
        internal string user = "";
    	internal Encoding charSet;
    	internal DateTime time;
    	
    	public string UserName
    	{
    		get { return user; }
    	}
    	
    	public Encoding CharSet
    	{
    		get { return charSet; }
    	}
    	
    	public DateTime Time
    	{
    		get { return time; }
    	}
    }
    #endregion
    
    #region ALTER
    public class AlterStartEventArgs : EventArgs
    {
    	internal string user = "";
    	internal string table = "";
    	internal Encoding charSet;
    	internal DateTime time;
    	
    	public string UserName
    	{
    		get { return user; }
    	}
    	
    	public string TableName
    	{
    		get { return table; }
    	}
    	
    	public Encoding CharSet
    	{
    		get { return charSet; }
    	}
    	
    	public DateTime Time
    	{
    		get { return time; }
    	}
    }
    
    public class AlterEndEventArgs : EventArgs
    {
    	internal string user = "";
    	internal string table = "";
    	internal Encoding charSet;
    	internal DateTime time;
    	
    	public string UserName
    	{
    		get { return user; }
    	}
    	
    	public string TableName
    	{
    		get { return table; }
    	}
    	
    	public Encoding CharSet
    	{
    		get { return charSet; }
    	}
    	
    	public DateTime Time
    	{
    		get { return time; }
    	}
    }
    #endregion
    
    #region DELETE
    public class DeleteStartEventArgs : EventArgs
    {
    	internal string user = "";
    	internal string table = "";
    	internal Encoding charSet;
    	internal DateTime time;
    	
    	public string UserName
    	{
    		get { return user; }
    	}
    	
    	public string TableName
    	{
    		get { return table; }
    	}
    	
    	public Encoding CharSet
    	{
    		get { return charSet; }
    	}
    	
    	public DateTime Time
    	{
    		get { return time; }
    	}
    }
    
    public class DeleteEndEventArgs : EventArgs
    {
    	internal string user = "";
    	internal string table = "";
    	internal Encoding charSet;
    	internal DateTime time;
    	
    	public string UserName
    	{
    		get { return user; }
    	}
    	
    	public string TableName
    	{
    		get { return table; }
    	}
    	
    	public Encoding CharSet
    	{
    		get { return charSet; }
    	}
    	
    	public DateTime Time
    	{
    		get { return time; }
    	}
    }
    #endregion
    
    #region TRUNCATE
    public class TruncateStartEventArgs : EventArgs
    {
    	internal string user = "";
    	internal string table = "";
    	internal Encoding charSet;
    	internal DateTime time;
    	
    	public string UserName
    	{
    		get { return user; }
    	}
    	
    	public string TableName
    	{
    		get { return table; }
    	}
    	
    	public Encoding CharSet
    	{
    		get { return charSet; }
    	}
    	
    	public DateTime Time
    	{
    		get { return time; }
    	}
    }
    
    public class TruncateEndEventArgs : EventArgs
    {
    	internal string user = "";
    	internal string table = "";
    	internal Encoding charSet;
    	internal DateTime time;
    	
    	public string UserName
    	{
    		get { return user; }
    	}
    	
    	public string TableName
    	{
    		get { return table; }
    	}
    	
    	public Encoding CharSet
    	{
    		get { return charSet; }
    	}
    	
    	public DateTime Time
    	{
    		get { return time; }
    	}
    }
    #endregion
    
    #region UPDATE
    public class UpdateStartEventArgs : EventArgs
    {
    	internal string user = "";
    	internal string table = "";
    	internal Encoding charSet;
    	internal DateTime time;
    	
    	public string UserName
    	{
    		get { return user; }
    	}
    	
    	public string TableName
    	{
    		get { return table; }
    	}
    	
    	public Encoding CharSet
    	{
    		get { return charSet; }
    	}
    	
    	public DateTime Time
    	{
    		get { return time; }
    	}
    }
    
    public class UpdateEndEventArgs : EventArgs
    {
    	internal string user = "";
    	internal string table = "";
    	internal Encoding charSet;
    	internal DateTime time;
    	
    	public string UserName
    	{
    		get { return user; }
    	}
    	
    	public string TableName
    	{
    		get { return table; }
    	}
    	
    	public Encoding CharSet
    	{
    		get { return charSet; }
    	}
    	
    	public DateTime Time
    	{
    		get { return time; }
    	}
    }
    #endregion
    
}





