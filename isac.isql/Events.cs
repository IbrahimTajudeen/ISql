using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using Isac.Isql.Collections;

namespace Isac.Isql
{
    public class CreateEventArgs : EventArgs
    {   
    	internal string user = "";
    	internal string name = "";
    	internal string createType = "";
    	internal Encoding charSet;
    	internal DateTime clock;
    	
    	
    	public string CreateType
    	{
    		get { return createType; }
    	}
    	
    	public string UserName
    	{
    		get { return user; }
    	}
    	
    	public string CreateName
    	{
    		get { return name; }
    	}
    	
    	public Encoding CharSet
    	{
    		get { return charSet; }
    	}
    	
    	public DateTime Time
    	{
    		get { return clock; }
    	}
    	
    }
    
    public class CreatedEventArgs : EventArgs
    {   
    	internal string user = "";
    	internal string name = "";
    	internal string createType = "";
    	internal Encoding charSet;
    	internal DateTime clock;
    	
    	public string CreateType
    	{
    		get { return createType; }
    	}
    	
    	public string UserName
    	{
    		get { return user; }
    	}
    	
    	public string CreateName
    	{
    		get { return name; }
    	}
    	
    	public Encoding CharSet
    	{
    		get { return charSet; }
    	}
    	
    	public DateTime Time
    	{
    		get { return clock; }
    	}
    }
    
    public class DropStartEventArgs : EventArgs
    {
    	internal string user = "";
    	internal string name = "";
    	internal string dropType = "";
    	internal DateTime time;
    	
    	public string DropType
    	{
    		get { return dropType; }
    	}
    	
    	public string Name
    	{
    		get { return name; }
    	}
    	
    	public string UserName
    	{
    		get { return user; }
    	}
    	
    	public DateTime Time
    	{
    		get { return time; }
    	}
    }
    
    public class DropFinishEventArgs : EventArgs
    {
    	internal string user = "";
    	internal string name = "";
    	internal string dropType = "";
    	internal DateTime time;
    	
    	public string DropType
    	{
    		get { return dropType; }
    	}
    	
    	public string Name
    	{
    		get { return name; }
    	}
    	
    	public string UserName
    	{
    		get { return user; }
    	}
    	
    	public DateTime Time
    	{
    		get { return time; }
    	}
    }
}



