using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

using Isac.Isql;

namespace Isac
{
    public static class ObjectLoader 
    {   
    	private static Dictionary<As, List<Type>> builtIns = new Dictionary<As, List<Type>>
    	{
    		[As.Both] = new List<Type> { typeof(BuiltIns) }
    	};
    	
    	private static Dictionary<As, List<Type>> userIns = new Dictionary<As, List<Type>>();
    	
        internal static void MyLoader(Type type, As what = As.Both)
        {
        	if(builtIns.ContainsKey(what))
        	{
        		List<Type> oldtypes = builtIns[what];
        		oldtypes.Add(type);
        		builtIns[what] = oldtypes;
        	}
        	else
        		builtIns.Add(what, new List<Type> { type });
        }
        
        public static void TypeLoader(Type type, As what = As.Both)
        {
        	//if(type == typeof(BuiltIns))
        	//	throw new ISqlLoaderException($"Error: cannot use a builIns type as a user define type");
			
        	if(userIns.ContainsKey(what))
        	{
        		List<Type> oldtypes = userIns[what];
        		oldtypes.Add(type);
        		userIns[what] = oldtypes;
        	}
        	else
        		userIns.Add(what, new List<Type> { type });
        }
        
        internal static dynamic Caller(string name, object[] args, As what, string from = "builtIns")
        {
        	
        	if(from.ToLower() == "builtins")
        	{
        		
        		if(builtIns.ContainsKey(what) && what == As.KeywordObject)
        		{
        			if(args != null || args.Length != 0)
        				throw new ISqlArguementException($"Error: keywords can not have arguements");
        				
        			PropertyInfo property;
        			List<Type> types = builtIns[what];
        			foreach(Type type in types)
        			{
        				property = type.GetProperty(name);
        				if(property != null && property.CanRead)
        					return property.GetValue(type);
        			}
        			throw new ISqlPropertyNotFoundException($"Error: the property '{name}' could not be found from the builtIns type");
        		}
        		
        		else if(builtIns.ContainsKey(what) && what == As.MethodObject)
        		{
        			MethodInfo method;
        			List<Type> types = builtIns[what];
        			foreach(Type type in types)
        			{
        				object instance = Activator.CreateInstance(type);
        				method = type.GetMethod(name, 
        					BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance
        					| BindingFlags.IgnoreCase
        					);
        				if(method != null && method.ReturnType != typeof(void))
        					return method.Invoke(instance, args);
        			}
        			
        			throw new ISqlMethodNotFoundException($"Error: the method '{name}' could not be found from the builtIns type");
        		}
        		
        		else if (builtIns.ContainsKey(As.Both))
        		{
        			if(what == As.KeywordObject)
        			{
	        			if(args != null && args.Length != 0)
	        				throw new ISqlArguementException($"Error: keywords can not have arguements");
	        			
	        			List<Type> types = builtIns[As.Both];
	        			foreach(Type type in types)
	        			{
	        				PropertyInfo property = type.GetProperty(name);
	        				
	        				if(property != null && property.CanRead)
	        					return property.GetValue(type);
	        			}
	        			
	        			throw new ISqlPropertyNotFoundException($"Error: the property '{name}' could not be found from the builtIns type");
        			}
        			
        			else if (what == As.MethodObject)
        			{
	        			List<Type> types = builtIns[As.Both];
	        			foreach(Type type in types)
	        			{
	        				MethodInfo mb = type.GetMethod(name);
	        				
	        				if(mb != null && mb.ReturnType != typeof(void))
	        					return mb.Invoke(type, Parser.ParameterFixer(mb.GetParameters(), args));

	        			}
	        			
	        			throw new ISqlMethodNotFoundException($"Error: the method '{name}' could not be found from the builtIns type");
        			}
        			
        			throw new ISqlException($"Error: the method or property '{name}' could not be found from the builtIns type");
        		}
        		
        		throw new ISqlLoaderException($"Error: builtIns Loader error");
        	}
        	
        	else if(from.ToLower() == "custom")
        	{
        		if(userIns.ContainsKey(what) && what == As.KeywordObject)
        		{
        			if(args != null && args.Length != 0)
        				throw new ISqlArguementException($"Error: keywords can not have arguements");
        				
        			PropertyInfo property;
        			List<Type> types = userIns[what];
        			foreach(Type type in types)
        			{
        				property = type.GetProperty(name);
        				if(property != null && property.CanRead)
        					return property.GetValue(type);
        			}
        			throw new ISqlPropertyNotFoundException($"Error: the property '{name}' could not be found from the userIns type");
        		}
        		
        		else if(userIns.ContainsKey(what) && what == As.MethodObject)
        		{
        			MethodInfo method;
        			List<Type> types = userIns[what];
        			foreach(Type type in types)
        			{
        				method = type.GetMethod(name, BindingFlags.IgnoreCase);
        				if(method != null && method.ReturnType != typeof(void))
        					return method.Invoke(type, args);
        			}
        			
        			throw new ISqlMethodNotFoundException($"Error: the method '{name}' could not be found from the userIns type");
        		}
        		
        		else if (userIns.ContainsKey(As.Both))
        		{
        			if(what == As.KeywordObject)
        			{
        				if(args != null && args.Length != 0)
	        				throw new ISqlArguementException($"Error: keywords can not have arguements");
	        				
	        			PropertyInfo property;
	        			List<Type> types = userIns[As.Both];
	        			foreach(Type type in types)
	        			{
	        				property = type.GetProperty(name);
	        				if(property != null && property.CanRead)
	        					return property.GetValue(type);
	        			}
	        			throw new ISqlPropertyNotFoundException($"Error: the property '{name}' could not be found from the userIns type");
        			}
        			
        			else if (what == As.MethodObject)
        			{
	        			List<Type> types = userIns[As.Both];
	        			foreach(Type type in types)
	        			{
	        				MethodInfo mb = type.GetMethod(name);
	        				
	        				if(mb != null && mb.ReturnType != typeof(void))
	        					return mb.Invoke(type, Parser.ParameterFixer(mb.GetParameters(), args));
	        			}
	        			
	        			throw new ISqlMethodNotFoundException($"Error: the method '{name}' could not be found from the userIns type");
        			}
        			
        			throw new ISqlException($"Error: the method or property '{name}' could not be found from the userIns type");
        		}
        		
        		throw new ISqlLoaderException($"Error: userIns Loader error");
        	}
        	
        	return "";
        }
        
    }
    
    public enum As
    {
    	KeywordObject, MethodObject, Both
    }
}