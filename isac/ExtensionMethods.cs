using System;
using System.Linq;
using System.Collections.Generic;

using System.Reflection;

using Isac.Isql;

namespace Isac
{
    internal static class ExtensionMethods 
    {         
        public static dynamic Cast(this object obj, Type to)
        {
        	return Convert.ChangeType(obj, to);
        }
        
        internal static int MyLastIndexOf(this List<dynamic> li, dynamic data)
        {
        	int last = -1; int count = 0;
        	foreach(var s in li)
        	{
        		if(s.GetType() == data.GetType())
        		{
	        		if(s == data)
	        			last = count;
        		}
        		
        		count++;
        	}
        	return last;
        }
        
        internal static int MyIndexOf(this List<dynamic> li, dynamic data)
        {
        	int last = -1; int count = 0;
        	foreach(var s in li)
        	{
        		if(s.GetType() == data.GetType())
        		{
	        		if(s == data)
	        		{
	        			last = count; break;
	        		}
        		}
        		
        		count++;
        	}
        	return last;
        }
        
        internal static List<dynamic> MyRemoveRange(this List<dynamic> li, int index, int count)
        {
        	if(index < 0 || index >= li.Count)
        		throw new ISqlArguementException($"Error: index out of range");
        	
        	int icount = 0;

        	for(int i = index; i < li.Count; i++)
        	{
        		if(icount == count)
        			break;
        			
        		if(icount < count)
        		{
        			li.RemoveAt(i);
        			
        			i--;
        			icount++;
        			continue;
        		}
        	}

        	return li;
        }
        
        internal static List<string> MyRemoveRange(this List<string> li, int index, int count)
        {
        	if(index < 0 || index >= li.Count)
        		throw new ISqlArguementException($"Error: index out of range");
        	
        	int icount = 0;

        	for(int i = index; i < li.Count; i++)
        	{
        		if(icount <= count)
        		{
        			li.RemoveAt(i);
        			
        			if(icount == count)
        				break;
        			
        			i--;
        			icount++;
        			continue;
        		}
        	}

        	return li;
        }
        
        internal static int GetEnclose(this string data, char open_sym, char close_sym, int start_index, int sead_count = 0)
        {
        	int count = sead_count;
        	for(int i = start_index; i < data.Length; i++)
        	{
        		if(data[i] == open_sym)
        			count++;

        		else if(data[i] == close_sym)
        			count--;
        		if(count < 0) throw new ISqlArguementException($"Error: brackets are out of order");
        		
        		if(count == 0 && data[i] == close_sym)
        			return i;

        	}
        	return -1;
        }
        
        internal static int GetEnclose(this List<dynamic> data, dynamic open_sym, dynamic close_sym, int start_index, int sead_count = 0)
        {
        	int count = sead_count;
        	for(int i = start_index; i < data.Count; i++)
        	{
        		if(data[i].GetType() == open_sym.GetType() && data[i].ToString() == open_sym.ToString())
        			count++;

        		else if (data[i].GetType() == close_sym.GetType() && data[i].ToString() == close_sym.ToString())
       				count--;

        		if(count < 0) throw new ISqlArguementException($"Error: brackets are out of order");
        		
        		if (data[i].GetType() == close_sym.GetType())
        		{
        			if(count == 0 && data[i] == close_sym)
        				return i;
        		}
        		
        	}
        	return -1;
        
        }
        
        public static Isql.Collections.Cell[] ToCellArray(this IEnumerable<object> obj)
        {
        	List<Isql.Collections.Cell> cells  = new List<Isql.Collections.Cell>();
        	foreach(var c in obj)
            {
                if (c.GetType() == typeof(Isql.Collections.Cell))
                    cells.Add((Isql.Collections.Cell)c);
                else
                    cells.Add(new Isql.Collections.Cell(c));
            }
        	
        	return cells.ToArray<Isql.Collections.Cell>();
        }

        public static Dictionary<TK, TV> AddRange<TK,TV>(this Dictionary<TK, TV> dicObj, Dictionary<TK, TV> source)
        {
            if (source.Count == 0)
                return dicObj;

            foreach (var key in source.Keys)
            {
                dicObj.Add(key, source[key]);
            }

            return dicObj;
        }

        public static Dictionary<TK, TV> AddRangeOutIndexes<TK, TV>(this Dictionary<TK, TV> dicObj, Dictionary<TK, TV> source, ref List<int> indexes)
        {
            if (source.Count == 0)
                return dicObj;

            foreach (var key in source.Keys)
            {
                if (dicObj.ContainsKey(key))
                {
                    dicObj[key] = source[key];
                    indexes.Add(dicObj.Keys.ToList<TK>().LastIndexOf(key));
                }
                else
                {
                    dicObj.Add(key, source[key]);
                    indexes.Add(dicObj.Keys.ToList<TK>().LastIndexOf(key));
                }
            }
            indexes = indexes.Distinct().ToList();
            return dicObj;
        }


        internal static Isql.Collections.ColumnDefination GenerateColumnDefination<TK, TV>(this Dictionary<string, Type> dicObj)
        {
            if (dicObj.Count == 0 || dicObj == null)
                throw new Isql.ISqlException($"Error: Dictionary Empty");

            Isql.Collections.ColumnDefination defination = new Isql.Collections.ColumnDefination();

            foreach (var keys in dicObj.Keys)
            {
                if (dicObj[keys] != typeof(None))
                    defination.AddColumn(new Isql.Collections.Column(keys, dicObj[keys]));
                else
                    defination.AddColumn(new Isql.Collections.Column(keys));

            }

            return defination;
        }
    }
}





