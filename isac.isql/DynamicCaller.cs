using System;
using System.Linq;
using System.Collections.Generic;

using System.Reflection;

namespace Isac.Isql
{
    internal class DynamicCaller 
    {         
        public DynamicCaller()
        {}
        public object Caller(object instance, string methodName, 
        					 params object[] parameters)
        {
        	methodName = methodName.ToUpper();
			Type typeInstace = instance.GetType();
			var inst  = Activator.CreateInstance(typeInstace, null);
			MethodInfo meth = typeInstace.GetMethod(methodName);
			ParameterInfo[] paras = meth.GetParameters();
			
			return new object();
        }
    }
}