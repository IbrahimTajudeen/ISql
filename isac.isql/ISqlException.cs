﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Isac.Isql
{
    public class ISqlException : Exception
    {
        public ISqlException(string Message) : base(Message)
        {
        }
    }
    
    public class ISqlPermissionException: ISqlException
    {
    	public ISqlPermissionException(string Message) : base(Message)
    	{
    		
    	}
    }
    
    public class ISqlLoaderException: ISqlException
    {
    	public ISqlLoaderException(string Message) : base(Message)
    	{
    		
    	}
    }
    
    public class ISqlMethodNotFoundException: ISqlException
    {
    	public ISqlMethodNotFoundException(string Message) : base(Message)
    	{
    		
    	}
    }
	
	public class ISqlPropertyNotFoundException: ISqlException
    {
    	public ISqlPropertyNotFoundException(string Message) : base(Message)
    	{
    		
    	}
    }
	
    public class ISqlTableExistsException : ISqlException
    {
        public ISqlTableExistsException(string Message) : base(Message)
        {
        }
    }
    public class ISqlDatabaseExistsException : ISqlException
    {
        public ISqlDatabaseExistsException(string Message) : base(Message)
        {
        }
    }
	
    public class ISqlTableNotFoundException : ISqlException
    {
        public ISqlTableNotFoundException(string Message) : base(Message)
        {
        }
    }
    public class ISqlFormatException : ISqlException
    {
    	public ISqlFormatException(string Message) : base(Message){}
    }
    public class ISqlDatabaseNotFoundException : ISqlException
    {
        public ISqlDatabaseNotFoundException(string Message) : base(Message)
        {
        }
    }
    public class ISqlColumnNotFoundException : ISqlException
    {
        public ISqlColumnNotFoundException(string Message) : base(Message)
        {

        }
    }
    public class ISqlSyntaxException : ISqlException
    {
        public ISqlSyntaxException(string Message) : base(Message)
        {
        }
    }
    public class ISqlArithemeticException : ISqlException
    {
        public ISqlArithemeticException(string Message) : base(Message)
        {
        }
    }
    public class ISqlLogicException : ISqlException
    {
        public ISqlLogicException(string Message) : base(Message)
        {
        }
    }
    public class ISqlTypeException : ISqlException
    {
        public ISqlTypeException(string Message) : base(Message)
        {
        }
    }
    public class ISqlModifierException : ISqlException
    {
        public ISqlModifierException(string Message) : base(Message)
        {

        }
    }
    public class ISqlArguementException : ISqlException
    {
        public ISqlArguementException(string Message) : base(Message)
        {

        }
    }
    public class ISqlConnectionNotFoundException : ISqlException
    {
        public ISqlConnectionNotFoundException(string Message) : base(Message)
        {

        }
    }
}





