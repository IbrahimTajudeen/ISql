using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using System.Reflection;

using Isac.Isql;

namespace Isac
{
	internal class Parser
	{
		#region Overall Table
		internal Dictionary<string, string[]> STable = new Dictionary<string, string[]>()
		{
			{"@",new string[] {"_abcdefghijklmnopqrstuvwxyz0123456789", ".[()+-/*^%],"} },
			{",", new string[] { "@_abcdefghijklmnopqrstuvwxyz0123456789!([+-'`"} },
			{"[",new string[] { "@_abcdefghijklmnopqrstuvwxyz0123456789(!`[]@-+" } },
			{"]", new string[] {",;])"} },
			{"name", new string[] { "_abcdefghijklmnopqrstuvwxyz0123456789", "+-*/,%^/&|!><=:]()" } },
			{":", new string[] { "_abcdefghijklmnopqrstuvwxyz0123456789`'", ",]" } },
			{";", new string[] { "@_abcdefghijklmnopqrstuvwxyz0123456789(" } },
			{"number", new string[] {"1234567890.","+-*^/:,)%];&|!><="} },
			{"+", new string[] { "1234567890@_abcdefghijklmnopqrstuvwxyz-(^`'"  } },
			{"-", new string[] { "1234567890@_abcdefghijklmnopqrstuvwxyz+(^`'" } },
			{"*", new string[] {"1234567890@_abcdefghijklmnopqrstuvwxyz+-`'(" } },
			{"/", new string[] {"1234567890@_abcdefghijklmnopqrstuvwxyz+-(`'^" } },
			{"%", new string[] {"1234567890@_abcdefghijklmnopqrstuvwxyz+-`'(^" } },
			{"^",new string[] {"1234567890@_abcdefghijklmnopqrstuvwxyz+-(`'"} },
			{"(", new string[] { "@_abcdefghijklmnopqrstuvwxyz12345678909(+-`')[!*/%^;"} },
			{")", new string[] { ";],+:-)^&|!><=*/%" } },
			{".", new string[] { "*_abcdefghijklmnopqrstuvwxyz1234567890", ",^+-*/:%)&]|;!><=" } },
			{ "all", new string[] {"],)"} },
			// newly recentlly added
			{">",new string[] {"=", "(_abcdefghijklmnopqrstuvwxyz0123456789+-@`'"} },
			{"<",new string[] {"=>", "(_abcdefghijklmnopqrstuvwxyz0123456789+-@`'" } },
			{"!",new string[] {"=", "(_abcdefghijklmnopqrstuvwxyz0123456789+-@`'" } },
			{"=",new string[] {"(_abcdefghijklmnopqrstuvwxyz0123456789+-@'`" } },
			{"!=",new string[] {"(_abcdefghijklmnopqrstuvwxyz0123456789+-@'`"} },
			{">=",new string[] {"(_abcdefghijklmnopqrstuvwxyz0123456789+-@`'"} },
			{"<=",new string[] {"(_abcdefghijklmnopqrstuvwxyz0123456789+-@`'"} },
			{"<>",new string[] {"(_abcdefghijklmnopqrstuvwxyz0123456789+-@`'"} },
			{"&",new string[] {"(_abcdefghijklmnopqrstuvwxyz0123456789+-@`'!"} },
			{"|",new string[] {"(_abcdefghijklmnopqrstuvwxyz0123456789+-@`'!"} },
			{"value",new string[] {">=<!+]-*/%^:&|),"} }
		};
		#endregion
		
		internal static Type[] DataTypes = {
									typeof(Integer), typeof(FloatPoint), typeof(Varchar),
									typeof(Character), typeof(Text), typeof(DateTime),
									typeof(Date), typeof(Time), typeof(TimeSpan),
									typeof(Bit), typeof(TinyInt), typeof(Point),
									typeof(BigInt), typeof(Choice), typeof(Media), typeof(Set),
									typeof(int), typeof(float), typeof(double), typeof(byte),typeof(Int16),
									typeof(long), typeof(string), typeof(decimal), typeof(bool),
									typeof(char)
								  };
		
		internal dynamic[] MethodArgsBuilder(List<dynamic> args)
		{
			List<object> args_built = new List<object>();
			//	"23, [1,3,[4, 5], [6]],7"
			int arg_start = 0, brks = 0;
			for(int i = 0; i < args.Count; i++)
			{
				if(args[i].ToString() =="(")
					brks++;
				else if (args[i].ToString() == ")")
					brks--;
				
				if(i + 1 >= args.Count)
				{
					args_built.Add(args[i]);
					var exp = args_built.GetRange(arg_start, args_built.Count - arg_start).ToList<dynamic>();
					
					if(HasLogic(exp))
					{
						var logic = new Isql.Logistics.LogicExpressionEngine();
						logic.SetExpression(exp.ToArray<dynamic>());
						logic.Solve();
						var ans = logic.RawResult;
						args_built.RemoveRange(arg_start, args_built.Count - arg_start);
						args_built.Insert(arg_start, ans);
					}
					
					else if (HasMath(exp))
					{
						var expression = new Isql.Logistics.ExpressionEngine();
						expression.SetExpression(exp.ToArray<dynamic>());
						expression.Calculate();
						var ans = expression.RawResult;
						args_built.RemoveRange(arg_start, args_built.Count - arg_start);
						args_built.Insert(arg_start, ans);
					}
					else if(!HasMath(exp) && !HasLogic(exp) && exp.Count == 1)
					{
						Type prev_type = exp[0].GetType();
						Value ival = new Value(exp[0]);
						args_built.RemoveAt(arg_start);
						args_built.Insert(arg_start, (ival.Anno == "NUM") ? DataConverter(prev_type, ival.Data) : ival.Data);
					}
					
					arg_start = args_built.Count;
					continue;
				}
				
				if(args[i].ToString() == "," && args[i - 1].ToString() != "]" && brks == 0)
				{
					var exp = args_built.GetRange(arg_start, args_built.Count - arg_start).ToList<dynamic>();
				
					if(HasLogic(exp))
					{
						var logic = new Isql.Logistics.LogicExpressionEngine();
						logic.SetExpression(exp.ToArray<dynamic>());
						logic.Solve();
						var ans = logic.RawResult;
						args_built.RemoveRange(arg_start, args_built.Count - arg_start);
						args_built.Insert(arg_start, ans);
					}
					
					else if (HasMath(exp))
					{
						var expression = new Isql.Logistics.ExpressionEngine();
						expression.SetExpression(exp.ToArray<dynamic>());
						expression.Calculate();
						var ans = expression.RawResult;
						args_built.RemoveRange(arg_start, args_built.Count - arg_start);
						args_built.Insert(arg_start, ans);
					}
					else if(!HasMath(exp) && !HasLogic(exp) && exp.Count == 1)
					{
						Type prev_type = exp[0].GetType();
						Value ival = new Value(exp[0]);
						args_built.RemoveAt(arg_start);
						args_built.Insert(arg_start, (ival.Anno == "NUM") ? DataConverter(prev_type, ival.Data) : ival.Data);
					}
					
					arg_start = args_built.Count;
					continue;
				}
				
				if((args[i].ToString() != "[" && args[i].ToString() != "]" && args[i].ToString() != ",") || brks != 0)
				{
					args_built.Add(args[i]);
				}
				else if (args[i].ToString() == "[")
				{
					var array_args = ArrayArgsBuilder(args, i + 1);
					args_built.Add(array_args[0]);
					i = (int) array_args[1]; 

					i--;
					arg_start = args_built.Count;
					continue;
				}
				else if (args[i].ToString() == "]")
					throw new ISqlException($"Error: unknown error");

			}
			return args_built.ToArray<object>();
		}
		
		internal dynamic[] ArrayArgsBuilder(List<dynamic> args, int start)
		{
			List<dynamic> arr = new List<dynamic>(); int arg_start = 0;
			for(int i = start; i < args.Count; i++)
			{
				if(i + 1 >= args.Count && args[i].ToString() != "]")
					throw new ISqlException($"Error: array not closed properly\n'{string.Join(" ", string.Join(" ", args) + " ~")}'");
				
				if(args[i].ToString() == "]")
				{
					
					var exp = arr.GetRange(arg_start, arr.Count - arg_start).ToList<dynamic>();
					
					if(HasLogic(exp))
					{
						var logic = new Isql.Logistics.LogicExpressionEngine();
						logic.SetExpression(exp.ToArray<dynamic>());
						logic.Solve();
						var ans = logic.RawResult;
						arr.RemoveRange(arg_start, arr.Count - arg_start);
						arr.Insert(arg_start, ans);
					}
					
					else if (HasMath(exp))
					{
						var expression = new Isql.Logistics.ExpressionEngine();
						expression.SetExpression(exp.ToArray<dynamic>());
						expression.Calculate();
						var ans = expression.RawResult;
						arr.RemoveRange(arg_start, arr.Count - arg_start);
						arr.Insert(arg_start, ans);
					}
					else if(!HasMath(exp) && !HasLogic(exp) && exp.Count == 1)
					{
						Type prev_type = exp[0].GetType();
						Value ival = new Value(exp[0]);
						arr.RemoveAt(arg_start);
						arr.Insert(arg_start, (ival.Anno == "NUM") ? DataConverter(prev_type, ival.Data) : ival.Data);
					}
					
					arg_start = arr.Count - arg_start;
					return new object[] { arr.ToArray<object>(), i + 1 };
				}
				
				if(args[i].ToString() != "[" && args[i].ToString() != "]" && args[i].ToString() != ",")
					arr.Add(args[i]);
				if(args[i].ToString() == "[")
				{
					var arr_re = ArrayArgsBuilder(args, i + 1);
					arr.Add(arr_re[0]); i = (int)arr_re[1];
					arg_start = arr.Count;
				 	i--;
				}
				else if(args[i].ToString() == "," && args[i - 1].ToString() != "]")
				{
					
					var exp = arr.GetRange(arg_start, arr.Count - arg_start).ToList<dynamic>();
					
					if(HasLogic(exp))
					{
						var logic = new Isql.Logistics.LogicExpressionEngine();
						logic.SetExpression(exp.ToArray<dynamic>());
						logic.Solve();
						var ans = logic.RawResult;
						arr.RemoveRange(arg_start, arr.Count - arg_start);
						arr.Insert(arg_start, ans);
					}
					
					else if (HasMath(exp))
					{
						var expression = new Isql.Logistics.ExpressionEngine();
						expression.SetExpression(exp.ToArray<dynamic>());
						expression.Calculate();
						var ans = expression.RawResult;
						arr.RemoveRange(arg_start, arr.Count - arg_start);
						arr.Insert(arg_start, ans);
					}
					else if(!HasMath(exp) && !HasLogic(exp) && exp.Count == 1)
					{
						Type prev_type = exp[0].GetType();
						Value ival = new Value(exp[0]);
						arr.RemoveAt(arg_start);
						arr.Insert(arg_start, (ival.Anno == "NUM") ? DataConverter(prev_type, ival.Data) : ival.Data);
					}
					
					arg_start = arr.Count - arg_start;
					continue;
				}
				else if(args[i].ToString() == "]")
					return new object[] { arr.ToArray<object>(), i + 1 };
			}
			
			throw new ISqlException($"Error: array not closed properly\n'{string.Join(" ", string.Join(" ", args) + " ~")}'");
		}
		
		internal static void DigPrint(dynamic[] data, int tab = 0)
		{
			
			foreach (var d in data)
			{
				if(d.GetType().IsArray)
				{
					DigPrint(d, tab + 1);
					continue;
				}
				string tb = string.Join("", Enumerable.Repeat("\t", tab).ToArray<string>());
				Console.WriteLine(tb+d);
			}
		}

		internal static dynamic ParameterFixer(ParameterInfo[] info, object[] input_args)
		{
			int targer_length = info.Length;
			
			int count = 0, args_count = 0;
			List<dynamic> output_args = new List<dynamic>();
			List<dynamic> temp_args = new List<dynamic>();
			
			foreach(var input in input_args.ToList<dynamic>())
			{
				//when arguements is greater than the specified parameters
				if(count >= targer_length - 1)
				{
					if(!(info[count].ParameterType.IsArray || info[count].ParameterType == typeof(Set)) && input.GetType().IsArray)
							throw new ISqlArguementException($"Error: invalid parameter arguement. cannot convert type of '{info[count].ParameterType}' to '{input.GetType()}'");
							
					if(info[count].ParameterType.IsArray)
					{
						if(input.GetType().IsArray)
						{
							
							foreach(var parg in input)
								temp_args.Add(parg);
						}
						else
							temp_args.Add(input);
							
					}
					
					else if (info[count].ParameterType == typeof(Set))
						temp_args.Add(input);
					
					else if(!info[count].ParameterType.IsArray && info[count].ParameterType != typeof(Set))
					{
						if(input.GetType().IsArray || info[count].ParameterType == typeof(Set))
							throw new ISqlArguementException($"Error: invalid parameter arguement. cannot convert type of '{input.GetType()}' to '{info[count].ParameterType}'");
						
						temp_args.Add(input);
					}
				}
				
				if(count < targer_length - 1)
				{
					if(info[count].ParameterType.IsArray)
					{
						if(input.GetType().IsArray)
							output_args.Add(input);
						
						else if(!input.GetType().IsArray)
							output_args.Add(new object[] { input });
					}
					
					else if(!info[count].ParameterType.IsArray) 
						output_args.Add(input);
				}
				
				if(args_count >= input_args.Length - 1 && temp_args.Count > 0)
				{
					if(info[count].ParameterType.IsArray)
						output_args.Add(temp_args.ToArray<object>());
					
					else if(!info[count].ParameterType.IsArray && info[count].ParameterType != typeof(Set))
					{
						foreach(var i in temp_args)
							output_args.Add(i);
					}
					
					else if (info[count].ParameterType == typeof(Set))
					{
						Set set = new Set();
						
						foreach(var it in temp_args)
						{
							if(it.GetType() == typeof(Set) || it.GetType().IsArray)
							{
								foreach(var s in it)
									set.Add(s);
							}
							else
								set.Add(it);
						}
							output_args.Add(set);
					}
					
					temp_args.Clear();
					break;
				}
				
				if(count + 1 > targer_length - 1 == false)
					count++;
					
				args_count++;
			}
			
			if(output_args.Count != info.Length && info.Where(x => x.GetType().IsArray).ToArray<object>().Length == 0)
				throw new ISqlArguementException("Error: parameter mismatch ");
			
			//DigPrint(output_args.ToArray<dynamic>());
			
			return output_args.ToArray<object>();
		}
		
		internal static string[] QuerySeprator(string query)
		{
			List<string> pre_element = new List<string>();
			//this put the terminate symbol ';' in specific places
			string value = "", open_tick = ""; 
			int count = 0; bool in_backtick = false;
			
			for (int i = 0; i < query.Length; i++)
			{
				if (count < 0)
					throw new ISqlSyntaxException($"Error: unexpected token encountered '{query[i - 1]}'\nbrackets are placed in incorrect order.");
				
				if((query[i] == '`' || query[i].ToString() == "'") && !in_backtick && open_tick == "")
				{
					in_backtick = true;
					open_tick = query[i].ToString();
				}
				
				else if ((query[i] == '`' || query[i].ToString() == "'") && in_backtick && open_tick == query[i].ToString())
				{
					in_backtick = false;
					open_tick = "";
				}
				
				if ((query[i] == '[' || query[i] == '(') && !in_backtick)
				{
					value += query[i];
					count++;
				}
				
				else if ((query[i] == '[' || query[i] == '(') && in_backtick)
					value += query[i];
	
				else if ((query[i] == ']' || query[i] == ')') && !in_backtick)
				{
					value += query[i];
					count--;
				}
				
				else if ((query[i] == ']' || query[i] == ')') && in_backtick)
					value += query[i];
				
				else if (count == 0 && query[i] == ',' && !in_backtick)
				{
					pre_element.Add(value.Trim());
					value = "";
				}
				
				else value += query[i];
				
				if (i >= query.Length - 1)
				{
					pre_element.Add(value.Trim());
					value = "";
					break;
				}
			}
			
			return pre_element.ToArray<string>();
		}
		
		internal static string[] AliasGetter(string query)
		{
			List<string> pre_element = new List<string>();
			//this put the terminate symbol ';' in specific places
			string value = "", value2 = "", open_tick = "";
			int index = -1;
			int count = 0; bool in_backtick = false, alias_taken = false;
			
			for (int i = 0; i < query.Length; i++)
			{
				if (count < 0)
					throw new ISqlSyntaxException($"Error: unexpected token encountered '{query[i - 1]}'\nbrackets are placed in incorrect order.");
				
				if(query[i] == ':' && count == 0 && !in_backtick && !alias_taken)
				{
					index = i; alias_taken = true;
				}
				
				if((query[i] == '`' || query[i].ToString() == "'") && !in_backtick && open_tick == "")
				{
					in_backtick = true;
					open_tick = query[i].ToString();
				}
				
				else if ((query[i] == '`' || query[i].ToString() == "'") && in_backtick && open_tick == query[i].ToString())
				{
					in_backtick = false;
					open_tick = "";
				}
				
				
				if ((query[i] == '[' || query[i] == '(') && !in_backtick)
				{
					value += query[i];
					count++;
				}
				
				else if ((query[i] == '[' || query[i] == '(') && in_backtick)
					value += query[i];
	
				else if ((query[i] == ']' || query[i] == ')') && !in_backtick)
				{
					value += query[i];
					count--;
				}
				
				else if ((query[i] == ']' || query[i] == ')') && in_backtick)
					value += query[i];
				
				else if (count == 0 && query[i] == ',' && !in_backtick)
				{
					pre_element.Add(value.Trim());
					value = "";
				}
				
				else value += query[i];
				
				if (i >= query.Length - 1)
				{
					pre_element.Add(value.Trim());
					value = "";
					break;
				}
			}
			
			if(index > 0)
			{
				value = string.Join("", pre_element);
                value2 = value.Substring(index + 1).Trim();
				value = value.Substring(0, index).Trim();

                if (value2 == "" && index > 0)
                    throw new ISqlException("Error: invalid alias");
				
				return new string[] { value.Trim(), value2.Trim() };
			}
			
			return new string[] { string.Join("", pre_element) };
		}
		
		internal bool HasLogic(List<dynamic> args)
		{
			//&& || ! != == > >= < <= <>
			if(args.Contains("&&") || args.Contains("||") || args.Contains("!") || args.Contains("!=") 
			|| args.Contains("==") || args.Contains(">") || args.Contains(">=") || args.Contains("<")
			|| args.Contains("<=") || args.Contains("<>"))
				return true;
			
			return false;
		}
		
		internal bool HasMath(List<dynamic> args)
		{
			// + - / * % ^ ( )
			if(args.Contains("+") || args.Contains("-") || args.Contains("/") || args.Contains("*")
			|| args.Contains("%") || args.Contains("^") || args.Contains("(") || args.Contains(")"))
				return true;
				
			return false;
		}
		
		internal bool StrHasLogic(string args)
		{
			if(args.Contains("&&") || args.Contains("||") || args.Contains("!") || args.Contains("!=") 
			|| args.Contains("==") || args.Contains(">") || args.Contains(">=") || args.Contains("<")
			|| args.Contains("<=") || args.Contains("<>"))
				return true;
			
			return false;
		}
		
		internal bool StrHasMath(string args)
		{
			if(args.Contains("+") || args.Contains("-") || args.Contains("/") || args.Contains("*")
			|| args.Contains("%") || args.Contains("^") || args.Contains("(") || args.Contains(")"))
				return true;
				
			return false;
		}
		
		#region Expression Table
		internal Dictionary<string, string[]> ExpressionTable = new Dictionary<string, string[]>()
		{
			{"@",new string[] {"_abcdefghijklmnopqrstuvwxyz0123456789", "."} },
			{"name", new string[] { "_abcdefghijklmnopqrstuvwxyz0123456789", "+-*%^/()" } },
			{"number", new string[] {"1234567890.","+-*/^)%"} },
			{"+", new string[] { "1234567890", "@_abcdefghijklmnopqrstuvwxyz+-()^" } },
			{"-", new string[] { "1234567890", "@_abcdefghijklmnopqrstuvwxyz+-()^" } },
			{"*", new string[] {"1234567890", "@_abcdefghijklmnopqrstuvwxyz+-()^" } },
			{"/", new string[] {"1234567890", "@_abcdefghijklmnopqrstuvwxyz+-(^)" } },
			{"%", new string[] {"1234567890", "@_abcdefghijklmnopqrstuvwxyz+-()^" } },
			{".", new string[] { "_abcdefghijklmnopqrstuvwxyz1234567890" , "^+-*/%)&|!><=" } },
			{"(", new string[] { "@_abcdefghijklmnopqrstuvwxyz12345678909(+-", "+-*/%^" } },
			{")", new string[] { "+-*/%^)", "@_abcdefghijklmnopqrstuvwxyz1234567890+-()^" } },
			{"^",new string[] {"1234567890", "@_abcdefghijklmnopqrstuvwxyz+-()"} }
		};
		#endregion

		//SELECT GETNEXT METHOD, THE GENERAL OVERSER
		internal object[] GetNext(int initial_index, string data_check, string parent, Dictionary<string, string[]> sym_table)
		{
			if(parent.ToCharArray().Length == 0)
			{
				if(initial_index >= data_check.Length - 1)
					throw new ISqlException($"Error: input error");
					
				parent = data_check[++initial_index].ToString();
			}
			
			int charCount = 0, pointCount = 0;
			string value = "", nextValue = "", annotation = "", data_to_check = $@"{data_check}";
			bool getDone = false;
			string[] check = sym_table[parent];
			

			for (int i = initial_index; i < data_to_check.Length; i++)
			{
				if ((parent == "+" || parent == "-" || parent == "/" || parent == "*" || parent == "%") == true && i >= data_to_check.Length - 1)
					throw new Exception($"Error: no uprand spotted '{data_to_check}'");

				//REAL REFERENCE GETS CHILD TWO STEP OPTION
				if (parent == "@" && getDone == false)
				{
					i++;
					if (i > data_to_check.Length - 1 || check[0].Contains(data_to_check[i].ToString().ToLower()) == false)
						throw new ISqlSyntaxException($"Error: '@' unknown expression, on identifier found\nCause of Production: '@'");

					annotation = GetType(data_to_check[i]);
					getDone = true;

					//annotation from '@' must be a name or value for columns with space
					if (annotation != "name")
						throw new ISqlSyntaxException($"Error: unexpected token or identifier encountered \nToken: '{data_to_check[i]}'\nExpected: an dentifier (name)\nCause of Production:'@'");

					goto NAME;
				}

				//REAL ALIAS GETS CHILD TWO STEP OPTION
				if (parent == ":" && getDone == false)
				{
					getDone = true;

					if (i >= data_to_check.Length - 1)
						throw new ISqlSyntaxException($"Error: unkown expression, no alias identifier found.\nCause of Production: ':'");

					i++;
					
					if(data_to_check[i] == ' ')
						i++;
						
					if (i >= data_to_check.Length)
						throw new ISqlSyntaxException($"Error: unkown expression, no alias identifier found.\nCause of Production: ':'");
					
					if (GetType(data_to_check[i]) != "name" && data_to_check[i].ToString() != "`")
						throw new ISqlSyntaxException($"Error: unknown token encountered '{data_to_check[i]}'\nCause of Production: ':'");

					annotation = GetType(data_to_check[i]);
					
					if (check[0].Contains(data_to_check[i].ToString().ToLower()) && annotation == "name")
						goto NAME;

					else if (check[0].Contains(data_to_check[i].ToString().ToLower()) && annotation == "value")
						goto ANNOVALUE;

					else
						throw new ISqlSyntaxException($"Error: unknown token encountered '{data_to_check[i]}'\nCause of Production: ':'");

				}

				//REAL TOKEN GETS CHILD TWO STEP OPTION
				if ((parent == "name" || parent == "number") && getDone == false)
				{
					getDone = true;

					annotation = GetType(data_to_check[i]);

					if (annotation == "name" && annotation == parent)
						goto NAME;

					else if (annotation == "number" && annotation == parent)
						goto NUMBER;

					else
						throw new ISqlException($"Error: unknown token encountered \nToken: '{data_to_check[i]}' ");
				}
				
				//REAL DOT GETS CHILD, TWO STEP OPTION 
				if (parent == "." && getDone == false)
				{
					getDone = true;
					i++;
					
					if(data_to_check[i] == ' ')
						i++;
						
					if (i >= data_to_check.Length)
						throw new ISqlException($"Error: identifier expected, no uprand");
						
					if (!check[0].Contains(data_to_check[i].ToString().ToLower()))
						throw new ISqlException($"Error: identifier expected ");
						
					else if (data_to_check[i] == '*' && parent == "." && i == data_to_check.Length - 1)
						return new object[] { "*", "*", "all", (i > data_to_check.Length - 1) ? -1 : i };
						
					annotation = GetType(data_to_check[i]);

					if (annotation != "name")
						throw new ISqlException($"Error: unexpected token encountered.\nData: '{data_to_check}'\nToken: '{data_to_check[i]}'\nFrom Parent: {parent}\nExpected: column name");

					goto NAME;
				}
				
				//REAL MATHS ONE STEP FORWARD ONE STEP OPTION			
				if ((parent == "+" || parent == "-" || parent == "*" || parent == "/" || parent == "%" || parent == "^") && getDone == false)
				{
					getDone = true;
					i++;//check this expression 2 + 1
					
					if (i >= data_to_check.Length)
						throw new ISqlSyntaxException($"Error: no uprand spotted");
						
					if(data_to_check[i] == ' ')
						i++;
					
					if (i >= data_to_check.Length)
						throw new ISqlSyntaxException($"Error: no uprand spotted");

					if ((parent == "*" || parent == "/" || parent == "%" || parent == "^") && data_to_check.StartsWith(parent))
						throw new ISqlSyntaxException($"Error: no previous uprand spotted");

					if (data_to_check.EndsWith(parent))
						throw new ISqlException($"Error: no uprand spotted '{data_to_check} ~'");
						
					if (i < data_to_check.Length)
					{
						if (!check[0].Contains(data_to_check[i].ToString().ToLower()) && i >= data_to_check.Length)
							throw new ISqlException($"Error: no uprand spotted");

						return new object[] { parent, data_to_check[i].ToString().ToLower(), GetType(data_to_check[i]), i };
					}
					
					throw new ISqlException($"Error: no uprand spotted");
				}
				
				//REAL TOKEN ONE STEP FORWARD ONE STEP OPTION
				if ((parent == "[" || parent == "]" || parent == "," || parent == "(" || parent == ")") && getDone == false)
				{
					getDone = true;
					
					//if ((i == data_to_check.Length) && (parent == ")" || parent == "]"))
					//	return new object[] { parent, "", "", -1 };
					string err = (parent == ",") ? "identifier list expected" : "identifier expected";
					i++;
					
					if (i > data_to_check.Length - 1 && (parent != "]" && parent != ")"))
						throw new ISqlSyntaxException($"Error: identifier expected\nIndex: '{i}' Length: '{data_to_check.Length}'");
					
					if ((i > data_to_check.Length - 1) && (parent == ")" || parent == "]"))
						return new object[] { parent, "", "", -1 };
					
					else if(i > data_to_check.Length - 1 && !(parent == ")" || parent == "]"))
						throw new ISqlSyntaxException($"Error: unrecognized token encountered, {err}\nToken: '{data_to_check[i]}'");
						
					if(data_to_check[i] == ' ')
						i++;
						
					if (i > data_to_check.Length - 1 && (parent != "]" && parent != ")"))
						throw new ISqlSyntaxException($"Error: identifier expected\nIndex: '{i}' Length: '{data_to_check.Length}'");	
					
					value = parent;
					
					//nextValue = data_to_check[i].ToString();
					//annotation = GetType(data_to_check[i]);
					
					if ((i == data_to_check.Length) && (parent == ")" || parent == "]"))
						return new object[] { parent, "", "", -1 };
					
					
					else if (check[0].Contains(data_to_check[i].ToString().ToLower()))
						return new object[] { parent, data_to_check[i].ToString(), GetType(data_to_check[i]), i };
						
					else throw new ISqlSyntaxException($"Error: unrecognized token encountered, {err}\nToken: '{data_to_check[i]}'");
				}

				//REAL TOKEN GETS CHILD OR ONE STEP FORWARD
				if (parent == "all" && getDone == false)
				{
					getDone = true; i++;
					if (i > data_to_check.Length - 1)
						return new object[] { data_to_check[i - 1], "", "", -1 };
					
					if(data_to_check[i] == ' ')
						i++;
					
					if (i > data_to_check.Length - 1)
						return new object[] { data_to_check[i - 1], "", "", -1 };
						
					if (!check[0].Contains(data_to_check[i].ToString().ToLower()))
						throw new ISqlException($"Error: unexpected token uncountered '{data_to_check[i]}'");

					if (check[0].Contains(data_to_check[i].ToString().ToLower()))
						return new object[] { data_to_check[i - 1], data_to_check[i], GetType(data_to_check[i]), i };
					else
						throw new ISqlException($"Error: unknown error occured");
				}
				
				//CONSIDER ADDING ! TO THIS GROUP
				//REAL RELS GETS SINGLE CHILD OR ONE STEP FORWARD
				if (parent == ">" || parent == "<" && getDone == false)
				{
					getDone = true;
					if (data_to_check.StartsWith(parent))
						throw new ISqlException($"Error: Error: no previous uprand spotted\nData: '{data_to_check}' \nIndex: '{i}'");
						
					i++;
					if (i >= data_to_check.Length)
						throw new ISqlException($"Error: no uprand spotted");

					if (data_to_check[i] == ' ')
					{
						i++;
						if (i >= data_to_check.Length)
							throw new ISqlException($"Error: no uprand spotted");

						else if (check[1].Contains(data_to_check[i].ToString().ToLower()))
							return new object[] { parent, data_to_check[i], GetType(data_to_check[i]), i };

						throw new ISqlException($"Error: unknown token encountered Token: '{data_to_check[i]}'");
						
					}

					if (!check[0].Contains(data_to_check[i].ToString().ToLower()) && !check[1].Contains(data_to_check[i].ToString().ToLower()))
						throw new ISqlException($"Error: unknown token encountered Token: '{data_to_check[i]}'");

					value = parent;
					if ((data_to_check[i].ToString() == "=" || data_to_check[i].ToString() == "<" || data_to_check[i].ToString() == ">" || data_to_check[i].ToString() == " ") && i >= data_to_check.Length - 1)
						throw new ISqlException($"Error: no uprand spotted");

					//if ((data_to_check[i] == '=') || parent == "<" && data_to_check[i] == '>' && check[0].Contains(data_to_check[i].ToString().ToLower()))	
					if((parent == "<" && (data_to_check[i] == '=' || data_to_check[i] == '>') && check[0].Contains(data_to_check[i].ToString().ToLower())) || 
						(parent == ">" && data_to_check[i] == '=' && check[0].Contains(data_to_check[i].ToString().ToLower())))
					{
						value += data_to_check[i];
						parent = value;
						check = STable[parent];	getDone = false;
						goto Equals;
					}

					else if (check[1].Contains(data_to_check[i].ToString().ToLower()))
						return new object[] { parent, data_to_check[i], GetType(data_to_check[i]), i };

					throw new ISqlException($"Error: unknown error occured Index: '{i}' DataIndex: '{data_to_check[i]}' Data: '{data_to_check}'");
				}

			//REAL EQLS ONE STEP FORWARD
			Equals:
				if ((parent == "=" || parent == "<=" || parent == ">=" || parent == "<>" || parent == "!=" ) && getDone == false)
				{
					getDone = true;
					value = parent;
					
					//for ==
					if (parent == "=")
					{
						i++;
						if (i >= data_to_check.Length - 1)
							throw new ISqlException($"Error: no uprand spotted");

						if (data_to_check[i].ToString() != parent)
							throw new ISqlException($"Error: invalid operator for '{parent}{parent}'");

						value += data_to_check[i];

						if(check[0].Contains(data_to_check[i + 1].ToString().ToLower()) && data_to_check[i + 1] != ' ')
							return new object[] { value, data_to_check[i + 1], GetType(data_to_check[i + 1]), i + 1 };
						
						if (data_to_check[i + 1] == ' ')
						{
							i++; 
							if (i >= data_to_check.Length - 1)
							throw new ISqlException($"Error: no uprand spotted");
							
							else if(check[0].Contains(data_to_check[i].ToString().ToLower()))
								return new object[] { value, data_to_check[i], GetType(data_to_check[i]), i };
						}
						
						else throw new ISqlSyntaxException($"Error: invalid uprand");
					}
					
					if(parent != "=")
						i++;
						
					if (i >= data_to_check.Length)
						throw new ISqlException($"Error: no uprand spotted {value}");

					if (data_to_check.StartsWith(parent))
						throw new ISqlException($"Error: no previous uprand spotted\nData: '{data_to_check}' \nIndex: '{i}'");

					if (data_to_check[i] == ' ')
					{
						i++;
						if (i >= data_to_check.Length)
							throw new ISqlException($"Error: no uprand spotted");

						if (check[0].Contains(data_to_check[i].ToString().ToLower()))
							return new object[] { value, data_to_check[i], GetType(data_to_check[i]), i };
						
						throw new ISqlException($"Error: unknown token encountered Token: '{data_to_check[i]}'");
					}
					if (!check[0].Contains(data_to_check[i].ToString().ToLower()))
						throw new ISqlException($"Error: unknown token encountered Token: '{data_to_check[i]}'");

					if (check[0].Contains(data_to_check[i].ToString().ToLower()))
						return new object[] { value, data_to_check[i], GetType(data_to_check[i]), i };

					else
						throw new ISqlException($"Error: unknown error occured Index: '{i}' DataIndex: '{data_to_check[i]}' Data: '{data_to_check}'");
				}
			
			//REAL GETS CHILD
			VALUE:
				if (parent == "value" && getDone == false)
				{
					getDone = true;
					string token = data_to_check[i].ToString();
					value = data_to_check[i].ToString();
					i++;
					if(i > data_to_check.Length - 1)
						throw new ISqlSyntaxException($"Error: this is an invalid value expression");
						
					if (i == data_to_check.Length - 1 && data_to_check[i].ToString() != token)
						throw new ISqlSyntaxException($"Error: this is an invalid value expression");

					for (int j = i; j < data_to_check.Length; j++)
					{
						
						if (((byte)data_to_check[j]) == ((byte)92))
						{
							if(data_to_check[j + 1].ToString() == "'")
								value += "'";
								
							j = j + 2;
							
							if(j > data_to_check.Length - 1)
								throw new ISqlSyntaxException($"Error: this is an invalid value expression");
							
							continue;
						}
						if (data_to_check[j].ToString() != token)
						{
							value += data_to_check[j];
							continue;
						}
						if (data_to_check[j].ToString() == token)
						{
							value += data_to_check[j];
							i = j;
							break;
						}
						if (j >= data_to_check.Length - 1)
							throw new ISqlSyntaxException($"Error: this is an invalid value expression");
					}

					i++;
					if (!value.StartsWith(token) || !value.EndsWith(token))
						throw new ISqlException($"Error: the value is invalid\nVALUE: '{data_to_check}'");

					if (i >= data_to_check.Length)
						return new object[] { value, "", "", -1 };

					if (check[0].Contains(data_to_check[i].ToString().ToLower()) == true)
					{
						if (i >= data_to_check.Length)
							return new object[] { value, "", "", -1 };

						return new object[] { value, data_to_check[i], GetType(data_to_check[i]), i };
					}

					else if (data_to_check[i] == ' ')
					{
						i++;
						if (check[0].Contains(data_to_check[i].ToString().ToLower()) == true)
						{
							if (i >= data_to_check.Length)
								return new object[] { value, "", "", -1 };
						
							return new object[] { value, data_to_check[i], GetType(data_to_check[i]), i };
						}
						else
							throw new ISqlException($"Error: unknown token encountered '{data_to_check[i]}' {value}");
					}

					else throw new ISqlException($"Error: unknown error or token encountered '{i}' '{data_to_check[i]}' {data_to_check}");

				}
			
			//GETS ONE CHILD ONE STEP OPTION
				if ((parent == "&" || parent == "|") && getDone == false)
				{
					getDone = true;
					i++;
					
					if (i >= data_to_check.Length || data_to_check.EndsWith(parent))
						throw new ISqlException($"Error: no uprand spotted");
					else if (data_to_check.StartsWith(parent))
						throw new ISqlException($"Error: no previous uprand spotted\nData: '{data_to_check}' \nIndex: '{i}'");	
						
					else if (data_to_check[i].ToString() != parent)
						throw new ISqlException($"Error: invalid operator for '{parent + parent}'");
					
					value = parent + parent;
					
					//CHECKING PREVIOUS DATA (UPRAND)
					try
					{
						if (data_to_check[i - 2] != ' ')
						{
							if (GetType(data_to_check[i - 2]) != "number" && GetType(data_to_check[i - 2]) != "value" && GetType(data_to_check[i - 2]) != "name" && GetType(data_to_check[i - 2]) != ")")
							{
								throw new ISqlException($"Error: invalid previous uprand detected");
							}
						}
						
						if (data_to_check[i - 2] == ' ')
						{
							if (GetType(data_to_check[i - 3]) != "number" && GetType(data_to_check[i - 3]) != "value" && GetType(data_to_check[i - 3]) != "name" && GetType(data_to_check[i - 3]) != ")")
							{
								throw new ISqlException($"Error: invalid previous uprand detected");
							}
						}

					}
					catch (Exception)
					{
						throw new ISqlException($"Error: invalid previous uprand detected");
					}

					i++;
					if (i >= data_to_check.Length - 1)
						throw new ISqlException($"Error: no uprand spotted");
					
					if (data_to_check[i] == ' ')
					{
						i++;
						if (i >= data_to_check.Length)
							throw new ISqlException($"Error: no uprand spotted");

						if (check[0].Contains(data_to_check[i].ToString().ToLower()))
							return new object[] { value, data_to_check[i], GetType(data_to_check[i]), i };

						throw new ISqlException($"Error: unknown token encountered Token: '{data_to_check[i]}'");
					}
					
					if (check[0].Contains(data_to_check[i].ToString().ToLower()))
							return new object[] { value, data_to_check[i], GetType(data_to_check[i]), i };
					throw new ISqlException($"Error: unknown error occured Index: '{i}' DataIndex: '{data_to_check[i]}' Data: '{data_to_check}'");
					 
				}
				
			//GETS ONE CHILD OR ONE STEP FORWARD ONE STEP OPTION
				if (parent == "!" && getDone == false)
				{
					getDone = true;
					value = "!";
					i++;
					if (i >= data_to_check.Length)
						throw new ISqlException($"Error: no uprand spotted");

					if (data_to_check[i] == ' ')
					{
						i++;
						if (i >= data_to_check.Length)
							throw new ISqlException($"Error: no uprand spotted");

						if (GetType(data_to_check[i]) == "number")
							throw new ISqlException($"Error: cannot apply ! on uprand int");

						if (check[0].Contains(data_to_check[i].ToString().ToLower()))
							return new object[] { parent, data_to_check[i], GetType(data_to_check[i]), i };
						
						throw new ISqlSyntaxException($"Error: unknown error occured.\nData: '{data_to_check}'\nToken: '{data_to_check[i]}'\nIndex: '{i}'");
					}
					
					else if (data_to_check[i] == '=')
					{
						value += data_to_check[i];
						parent = value;
						check = STable[parent];
						getDone = false;
						goto Equals;
					}
					
					if (GetType(data_to_check[i]) == "number")
						throw new ISqlException($"Error: cannot apply ! on uprand int");

					else if (!check[0].Contains(data_to_check[i].ToString().ToLower()) && !check[1].Contains(data_to_check[i].ToString().ToLower()))
						throw new ISqlException($"Error: unknown token encountered Token: '{data_to_check[i]}'");

					else if (check[1].Contains(data_to_check[i].ToString().ToLower()))
						return new object[] { parent, data_to_check[i], GetType(data_to_check[i]), i };

					else
						throw new ISqlException($"Error: unknown error occured Index: '{i}' DataIndex: '{data_to_check[i]}' Data: '{data_to_check}'");
				}
			
			//GETS CHILD
			ANNOVALUE:
				if(annotation == "value")
				{
					value = data_to_check[i].ToString();
					i++;
					if (i >= data_to_check.Length - 1 && data_to_check[i] != '`')
						throw new ISqlSyntaxException($"Error: this is an invalid value expression");

					for (int j = i; j < data_to_check.Length; j++)
					{
						if (data_to_check[j].ToString() != "`")
						{
							value += data_to_check[j];
							continue;
						}
						if (data_to_check[j].ToString() == "`")
						{
							value += data_to_check[j];
							i = j;
							break;
						}
						if (j >= data_to_check.Length - 1)
							throw new ISqlSyntaxException($"Error: this is an invalid value expression");
					}

					i++;
					if (!value.StartsWith("`") || !value.EndsWith("`"))
						throw new ISqlException($"Error: the value is invalid");
					
					if(!(i >= data_to_check.Length - 1))
						throw new ISqlSyntaxException($"Error: an alias cannot have a preceding expression, identifier or production. Index: '{i}'\nLength: '{data_to_check.Length}'");
						
					if (i >= data_to_check.Length )
						return new object[] { value, "", "", -1 };
					
					else if (i < data_to_check.Length && data_to_check[i] == ']')
						return new object[] { value, data_to_check[i], GetType(data_to_check[i]), i };

					else throw new ISqlException($"Error: unknown error or token encountered '{data_to_check[i]}' {value}");
				}
			
			//REAL GETS CHILD
			NAME:
				if (annotation == "name")
				{
					for (int j = i; j < data_to_check.Length; j++)
					{
						if (check[0].Contains(data_to_check[j].ToString().ToLower()) && data_to_check[j] != ' ')
						{
							++charCount;
							value += data_to_check[j];

							if (j == data_to_check.Length - 1)
							{
								nextValue = ""; annotation = ""; i = -1; break;
							}
						}
						
						else if (check[1].Contains(data_to_check[j].ToString().ToLower()) && data_to_check[j] != ' ' && charCount > 0)
						{
							nextValue = data_to_check[j].ToString(); annotation = GetType(data_to_check[j]); i = j;
							break;
						}

						else if (data_to_check[j] == ' ')
						{
							j++;
							if (j >= data_to_check.Length && charCount > 0 && check[1].Contains(data_to_check[j].ToString().ToLower()))
							{ i = j; break; }

							else if (check[1].Contains(data_to_check[j].ToString().ToLower()) && charCount > 0)
							{
								nextValue = data_to_check[j].ToString(); annotation = GetType(data_to_check[j]); i = j;
								break;
							}
							else throw new ISqlSyntaxException($"Error: expression expected\nData: '{data_to_check}'\nToken: '{data_to_check[j]}'\nIndex: '{j}'");
						}
						else throw new ISqlSyntaxException($"Error: invalid token enncountered. Token: '{data_to_check[j]}' Token Index: '{j}'\nData: '{data_to_check}'");
					}

					IsValidate(value, "name");
					if (i == data_to_check.Length - 1)
					{
						if (check[0].Contains(data_to_check[i].ToString().ToLower()))
						{
							nextValue = ""; annotation = ""; i = -1;
						}
						else if (check[1].Contains(data_to_check[i].ToString().ToLower()))
						{
							nextValue = data_to_check[i].ToString(); annotation = GetType(data_to_check[i]);
						}
						else throw new ISqlSyntaxException($"Error: unknown token encountered.\tToken: '{data_to_check[i]}'");
					}
					else if (i >= data_to_check.Length - 1 && check[1].Contains(data_to_check[data_to_check.Length - 1]))
					{
						nextValue = data_to_check[i].ToString(); annotation = GetType(data_to_check[i]);
					}
					
					return new object[] { value, nextValue, annotation, i };
				}

			//REAL GETS CHILD
			NUMBER:
				if (annotation == "number")
				{
					for (int j = i; j < data_to_check.Length; j++)
					{
					
						if (check[0].Contains(data_to_check[j].ToString().ToLower()) && data_to_check[j] != ' ')
						{
							++charCount;
							value += data_to_check[j];
							if (data_to_check[j] == '.')
								pointCount++;

							if (j == data_to_check.Length - 1)
							{
								nextValue = ""; annotation = ""; i = -1; break;
							}
						}
						else if (!check[0].Contains(data_to_check[j].ToString().ToLower()) && check[1].Contains(data_to_check[j].ToString().ToLower()) && data_to_check[j] != ' ' && charCount > 0)
						{
							nextValue = data_to_check[j].ToString(); annotation = GetType(data_to_check[j]); i = j;
							break;
						}

						else if (data_to_check[j] == ' ')
						{
							j++;
							if (j >= data_to_check.Length && charCount > 0)
							{ i = j; break; }

							else if (!check[0].Contains(data_to_check[j].ToString().ToLower()) && check[1].Contains(data_to_check[j].ToString().ToLower()) && charCount > 0)
							{
								nextValue = data_to_check[j].ToString(); annotation = GetType(data_to_check[j]); i = j;
								break;
							}
							else throw new ISqlSyntaxException($"Error: expression expected\nData: '{data_to_check}'\nToken: '{data_to_check[j]}'\nIndex: '{j}'");
						}
						else throw new ISqlSyntaxException($"Error: invalid token enncountered. Token: '{data_to_check[j]}' Token Index: '{j}'\nData: '{data_to_check}'");
					}

					IsValidate(value, (pointCount == 0) ? "number" : "float");
					if (i == data_to_check.Length - 1)
					{
						if (check[0].Contains(data_to_check[i].ToString().ToLower()))
						{
							nextValue = ""; annotation = ""; i = -1;
						}
						else if (check[1].Contains(data_to_check[i].ToString().ToLower()))
						{
							nextValue = data_to_check[i].ToString(); annotation = GetType(data_to_check[i]);
						}
						//else throw new ISqlSyntaxException($"Error: unknown token encountered.\tToken: '{data_to_check[i]}'");
					}
					else if (i >= data_to_check.Length - 1 && check[1].Contains(data_to_check[data_to_check.Length - 1]))
					{
						nextValue = data_to_check[i].ToString(); annotation = GetType(data_to_check[i]);
					}

					if (pointCount > 1)
						throw new ISqlSyntaxException($"Error: this is a unrecognized float number");

					return new object[] { value, nextValue, annotation, i };
				}
			}

			throw new ISqlException($"Error: unknown error occured\nValue: '{value}'\nNextValue: '{nextValue}'\nAnnote: '{annotation}'\nIndex: '{initial_index}'\nparent: '{parent}'");
			//return new object[] { initial_index };
		}
		
		internal object[] GetNextExpression(int initial_index, string data_check, string parent, Dictionary<string, string[]> sym_table)
		{
			return GetNext(initial_index, data_check, parent, sym_table);
		}

		internal string GetType(char character)
		{
			if ("_abcdefghijklmnopqrstuvwxyz".Contains(character.ToString().ToLower()))
				return "name";

			else if ("1234567890".Contains(character))
				return "number";

			else if ("_abcdefghijklmnopqrstuvwxyz1234567890".Contains(character.ToString().ToLower()) == false && "[]@(),.:+^-*/%".Contains(character) == true)
				return character.ToString();

			else if (character.ToString().ToLower() == "`" || character.ToString().ToLower() == "'")
				return "value";

			else if (character.ToString() == "&" || character.ToString() == "|" || character.ToString() == "!" || character.ToString() == "=" || character.ToString() == ">" || character.ToString() == "<")
				return character.ToString();

			else
				throw new Exception($"Error: unknown token encountered '{character}'");
		}

		/*internal string GetType(string character)
		{
			if (character == "&&")
				return "**";

			else if (character == "||")
				return "||";

			else if (character == "!")
				return "!";

			else if (character == "=" || character == "!=")
				return character;

			else
				throw new Exception($"Error: unknown token encountered '{character}'");
		}*/
		
		public string SpaceRemover(string element, string valid_chars = null)
		{
			element = element.Trim(); char an = new char();
			if (element.Contains("  "))
			{
				bool isTrue = false;

				string @new = "";

				for (int i = 0; i < element.Length; i++)
				{
					if (element[i] == '`' || element[i].ToString() == "'")
					{
						an = element[i];
						@new += element[i]; i++;
						for (int j = i; j < element.Length; j++)
						{
							if (element[j] != an)
								@new += element[j];
							else if (element[j] == an)
							{
								@new += element[j];
								an = new char();
								i = j;
								break;
							}
						} isTrue = false; continue;
					}
					
					else if (element[i] == ' ' && i == 0)
					{
						isTrue = true;
						continue;
					}
					
					else if (element[i] == ' ' && isTrue == true)
					{
						continue;
					}
					
					else if (element[i] == ' ' && isTrue == false)
					{
						isTrue = true;
						if (valid_chars != null && string.IsNullOrEmpty(valid_chars) == false && string.IsNullOrWhiteSpace(valid_chars) == false)
						{
							if (valid_chars.ToLower().Contains(element[i].ToString().ToLower()))
								@new += element[i];
							else
								throw new ISqlException($"Error: invalid character '{element[i]}'");
						}
						else
							@new += element[i];
							continue;
					}
					
					else if (element[i] != ' ')
					{
						isTrue = false;
						if (valid_chars != null && string.IsNullOrEmpty(valid_chars) == false && string.IsNullOrWhiteSpace(valid_chars) == false)
						{
							if (valid_chars.ToLower().Contains(element[i].ToString().ToLower()))
								@new += element[i];
							else
								throw new ISqlException($"Error: invalid character '{element[i]}'");
						}
						else
							@new += element[i];
						continue;
					}
				}
				return @new;
			}
			return element;
		}

		internal void IsValidate(string data_to_check, string annotation)
		{
			string num = "1234567890";
			string alpha = "_abcdefghijklmnopqrstuvwxyz";
			string all = num + alpha;

			if (annotation == "name")
			{
				int co = 0;
				foreach (char c in data_to_check)
				{

					if (!alpha.Contains(c.ToString().ToLower()) && co == 0)
						throw new Exception($"Error! unknown token encountered, it is not a proper name \t '{data_to_check}'");
					else if (!all.Contains(c.ToString().ToLower()))
						throw new Exception($"Error! unknown token encountered, it is not a proper name \t '{data_to_check}'");
					co++;
				}
			}
			else if (annotation == "number")
			{
				try
				{
					long number = Convert.ToInt64(data_to_check);
				}
				catch (FormatException ex)
				{
					throw new Exception($"Error! unknown token encountered, it is not a proper number '{data_to_check}' '{data_to_check[0]}'\n {ex.Message}");
				}

			}
			else if (annotation == "float")
			{
				try
				{
					Convert.ToDouble(data_to_check);
				}
				catch (FormatException ex)
				{
					throw new Exception($"Error! unknown token encountered, it is not a proper number '{data_to_check}' '{data_to_check[0]}'\n {ex.Message}");
				}
			}
			else throw new ISqlException($"Error: unknown annotation passed\nAnnotation: '{annotation}'");
			
		}

		internal static bool CheckNum(string value)
		{
			try
			{
				double m = Convert.ToDouble(value);
				return true;
			}
			catch (Exception)
			{
				try
				{
					long l = Convert.ToInt64(value);
					return true;
				}
				catch (Exception)
				{
					return false;
				}
			}
		}

		internal bool IsNumber(string value)
		{
			string first_test = "^[0-9]+";
			string second_test = "[[+-]?]^[0-9]+";
			Regex first_rex = new Regex(first_test, RegexOptions.Compiled);
			Regex second_rex = new Regex(second_test, RegexOptions.Compiled);
			List<bool> test = new List<bool>()
			{
				first_rex.IsMatch(value),
				second_rex.IsMatch(value)
			};
			if (test.Contains(true))
				return true;
			return false;
		}

		internal bool IsName(string value)
		{
			string[] name_alias = value.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);

			if (name_alias[0].StartsWith("@") && name_alias[0].Count(x => x == '.') == 1)
			{
				try
				{
					string alia = name_alias[0].Substring(1, name_alias[0].IndexOf(".") - 1);

					IsValidate(alia, "name");
					alia = name_alias[0].Substring(name_alias[0].IndexOf(".") + 1);
					IsValidate(alia, "name");
					if (name_alias.Length == 2)
						IsValidate(name_alias[1], "name");
				}
				catch (Exception)
				{
					return false;
				}
				return true;
			}
			else
			{
				try
				{
					string alia = name_alias[0];

					IsValidate(alia, "name");

					if (name_alias.Length == 2)
						IsValidate(name_alias[1], "name");
				}
				catch (Exception)
				{
					return false;
				}

				return true;
			}

		}

		public static Type TypeConverter(string type)
		{
			string t = type.ToLower().Trim();
			Type type1 = typeof(ISqlException);

			if (t == "int32" || t == "int")
				type1 = typeof(int);

			else if (t == "object")
				type1 = typeof(object);

			else if (t == "float" || t == "single")
				type1 = typeof(float);

			else if (t == "double")
				type1 = typeof(double);

			else if (t == "byte" || t == "int16")
				type1 = typeof(byte);

			else if (t == "long" || t == "int64")
				type1 = typeof(long);

			else if (t == "decimal")
				type1 = typeof(decimal);

			else if (t == "bool" || t == "boolean")
				type1 = typeof(bool);

			else if (t == "char")
				type1 = typeof(char);

			else if (t == "string")
				type1 = typeof(string);

			else if (t == "integer")
				type1 = typeof(Integer);

			else if (t == "bit")
				type1 = typeof(Bit);

			else if (t == "tinyint")
				type1 = typeof(TinyInt);

			else if (t == "bigint")
				type1 = typeof(BigInt);

			else if (t == "point")
				type1 = typeof(Point);

			else if (t == "floatpoint")
				type1 = typeof(FloatPoint);

			else if (t == "varchar")
				type1 = typeof(Varchar);

			else if (t == "character")
				type1 = typeof(Character);

			else if (t == "text")
				type1 = typeof(Text);

			else if (t == "datetime")
				type1 = typeof(DateTime);

			else if (t == "date")
				type1 = typeof(Date);

			else if (t == "time")
				type1 = typeof(Time);

			else if (t == "timespan")
				type1 = typeof(TimeSpan);

			else if (t == "choice")
				type1 = typeof(Choice);

			else
				throw new ISqlException($"Error: '{type}' is not a recognized type");

			return type1;
		}

		public static string[] StringTypeConverter(params Type[] types)
		{
			List<string> type1 = new List<string>();
			foreach (Type t in types)
			{
				if (t == typeof(Int32))
					type1.Add("int");

				else if (t == typeof(Single))
					type1.Add("float");

				else if (t == typeof(object))
					type1.Add("object");

				else if (t == typeof(double))
					type1.Add("double");

				else if (t == typeof(Int16))
					type1.Add("byte");

				else if (t == typeof(Int32))
					type1.Add("long");

				else if (t == typeof(decimal))
					type1.Add("decimal");

				else if (t == typeof(bool))
					type1.Add("bool");

				else if (t == typeof(char))
					type1.Add("char");

				else if (t == typeof(string))
					type1.Add("string");

				else if (t == typeof(Integer))
					type1.Add("integer");

				else if (t == typeof(Bit))
					type1.Add("bit");

				else if (t == typeof(TinyInt))
					type1.Add("tinyint");

				else if (t == typeof(BigInt))
					type1.Add("bigint");

				else if (t == typeof(Point))
					type1.Add("point");

				else if (t == typeof(FloatPoint))
					type1.Add("floatpoint");

				else if (t == typeof(Varchar))
					type1.Add("varchar");

				else if (t == typeof(Character))
					type1.Add("character");

				else if (t == typeof(Text))
					type1.Add("text");

				else if (t == typeof(DateTime))
					type1.Add("datetime");

				else if (t == typeof(Date))
					type1.Add("date");

				else if (t == typeof(Time))
					type1.Add("time");

				else if (t == typeof(TimeSpan))
					type1.Add("timespan");

				else if (t == typeof(Choice))
					type1.Add("choice");

				else if (!DataTypes.Contains(t))
					throw new ISqlException($"Error: '{t}' is not a recognized type");
				else
					throw new ISqlException($"Error: unknown error occured \nInputType:'{t}'");
			}

			return type1.ToArray<string>();

		}

		internal static string[] ArrayToString(object[] obj)
		{
			List<string> objstr = new List<string>();
			foreach (var o in obj)
			{
				objstr.Add(o.ToString());
			}
			return objstr.ToArray<string>();
		}

		public static dynamic DataConverter(Type type, dynamic objData)
		{
			dynamic value = "";
			string data = objData.ToString().Trim();

			Type type1 = typeof(ISqlException);

			if (type == typeof(int))
			{
				try { value = int.Parse(data); }
				catch (FormatException)
				{
					if (data.ToLower() == "null")
						value = default(int);
					else
						throw new ISqlFormatException($"Error: '{data}' is not in a correct format\nExpecting type of '{type.ToString()}");
				}
			}

			else if (type == typeof(object))
			{
				value = objData;
			}
			
			else if (type == typeof(float))
			{
				try { value = float.Parse(data); }
				catch (FormatException)
				{
					if (data.ToLower() == "null")
						value = default(float);
					else
						throw new ISqlFormatException($"Error: '{data}' is not in a correct format\nExpecting type of '{type.ToString()}");
				}
			}

			else if (type == typeof(double))
			{
				try { value = double.Parse(data); }
				catch (FormatException)
				{
					if (data.ToLower() == "null")
						value = default(double);
					else
						throw new ISqlFormatException($"Error: '{data}' is not in a correct format\nExpecting type of '{type.ToString()}");
				}
			}

			else if (type == typeof(char))
			{
				try { value = char.Parse(data); }
				catch (FormatException)
				{
					if (data.ToLower() == "null")
						value = default(char);
					else
						throw new ISqlFormatException($"Error: '{data}' is not in a correct format\nExpecting type of '{type.ToString()}");
				}
			}

			else if (type == typeof(bool))
			{
				try { value = bool.Parse(data); }
				catch (FormatException)
				{
					if (data.ToLower() == "null")
						value = default(bool);
					else
						throw new ISqlFormatException($"Error: '{data}' is not in a correct format\nExpecting type of '{type.ToString()}");
				}
			}

			else if (type == typeof(decimal))
			{
				try { value = decimal.Parse(data); }
				catch (FormatException)
				{
					if (data.ToLower() == "null")
						value = default(decimal);
					else
						throw new ISqlFormatException($"Error: '{data}' is not in a correct format\nExpecting type of '{type.ToString()}");
				}

			}

			else if (type == typeof(long))
			{
				try { value = long.Parse(data); }
				catch (FormatException)
				{
					if (data.ToLower() == "null")
						value = default(long);
					else
						throw new ISqlFormatException($"Error: '{data}' is not in a correct format\nExpecting type of '{type.ToString()}'");
				}

			}

			else if (type == typeof(Int16))
			{
				try { value = Int16.Parse(data); }
				catch (FormatException)
				{
					if (data.ToLower() == "null")
						value = default(Int16);
					else
						throw new ISqlFormatException($"Error: '{data}' is not in a correct format\nExpecting type of '{type.ToString()}'");
				}

			}
			
			else if (type == typeof(byte))
			{
				try { value = byte.Parse(data); }
				catch (FormatException)
				{
					if (data.ToLower() == "null")
						value = default(byte);
					else
						throw new ISqlFormatException($"Error: '{data}' is not in a correct format\nExpecting type of '{type.ToString()}'");
				}

			}

			else if (type == typeof(string))
			{
				value = data;
			}

			else if (type == typeof(Integer))
			{
				try { value = Integer.Parse(data); }
				catch (FormatException)
				{
					if (data.ToLower() == "null")
						value = default(Integer);
					else
						throw new ISqlFormatException($"Error: '{data}' is not in a correct format\nExpecting type of '{type.ToString()}");
				}
			}

			else if (type == typeof(Varchar))
			{
				value = Varchar.ToVarchar(data);
			}

			else if (type == typeof(Text))
			{
				try { value = Text.ToText(data); }
				catch (ISqlException)
				{
					throw new ISqlFormatException($"Error: '{data}' is not in a correct format\nExpecting type of '{type.ToString()}'");
				}
			}

			else if (type == typeof(DateTime))
			{
				try
				{
					value = DateTime.Parse(data);
				}
				catch (FormatException)
				{
					if (data.ToLower() == "null")
						value = default(DateTime);
					else
						throw new ISqlFormatException($"Error: '{data}' is not in a correct format\nExpecting type of '{type.ToString()}'");
				}
			}

			else if (type == typeof(TimeSpan))
			{
				try { value = TimeSpan.Parse(data); }
				catch (FormatException)
				{
					if (data.ToLower() == "null")
						value = default(TimeSpan);
					else
						throw new ISqlFormatException($"Error: '{data}' is not in a correct format\nExpecting type of '{type.ToString()}'");
				}
			}

			else if (type == typeof(Date))
			{
				try { value = Date.Parse(data); }
				catch (FormatException)
				{
					if (data.ToLower() == "null")
						value = default(Date);
					else
						throw new ISqlFormatException($"Error: '{data}' is not in a correct format\nExpecting type of '{type.ToString()}'");
				}
			}

			else if (type == typeof(Time))
			{
				try { value = Time.Parse(data); }
				catch (FormatException)
				{
					if (data.ToLower() == "null")
						value = default(Time);
					else
						throw new ISqlFormatException($"Error: '{data}' is not in a correct format\nExpecting type of '{type.ToString()}'");
				}
			}

			else if (type == typeof(Character))
			{
				try { value = Character.Parse(data); }
				catch (FormatException)
				{
					throw new ISqlFormatException($"Error: '{data}' is not in a correct format\nExpecting type of '{type.ToString()}'");
				}
			}

			else if (type == typeof(BigInt))
			{
				try { value = BigInt.Parse(data); }
				catch (FormatException)
				{
					if (data.ToLower() == "null")
						value = default(BigInt);
					else
						throw new ISqlFormatException($"Error: '{data}' is not in a correct format\nExpecting type of '{type.ToString()}");
				}
			}

			else if (type == typeof(FloatPoint))
			{
				try { value = FloatPoint.Parse(data); }
				catch (FormatException)
				{
					if (data.ToLower() == "null")
						value = default(FloatPoint);
					else
						throw new ISqlFormatException($"Error: '{data}' is not in a correct format\nExpecting type of '{type.ToString()}");
				}
			}

			else if (type == typeof(Point))
			{
				try { value = Point.Parse(data); }
				catch (FormatException)
				{
					if (data.ToLower() == "null")
						value = default(Point);
					else
						throw new ISqlFormatException($"Error: '{data}' is not in a correct format\nExpecting type of '{type.ToString()}");
				}
			}

			else if (type == typeof(TinyInt))
			{
				try { value = TinyInt.Parse(data); }
				catch (FormatException)
				{
					if (data.ToLower() == "null")
						value = default(TinyInt);
					else
						throw new ISqlFormatException($"Error: '{data}' is not in a correct format\nExpecting type of '{type.ToString()}");
				}
			}

			else if (type == typeof(Bit))
			{
				try { value = Bit.Parse(data); }
				catch (FormatException)
				{
					if (data.ToLower() == "null")
						value = default(Bit);
					else
						throw new ISqlFormatException($"Error: '{data}' is not in a correct format\nExpecting type of '{type.ToString()}");
				}
			}

			else if (type == typeof(Choice))
			{
				try { value = Choice.Parse(data); }
				catch (FormatException)
				{
					if (data.ToLower() == "null")
						value = default(Choice);
					else
						throw new ISqlFormatException($"Error: '{data}' is not in a correct format\nExpecting type of '{type.ToString()}");
				}
			}
			
			else if (type == typeof(Set))
			{
				
				
				Set set = new Set();
				if(objData.GetType().IsArray || objData.GetType() == typeof(Set))
				{
					foreach(var it in objData)
						set.Add(it);
						
					value = set;
				}
				
				else if(!objData.GetType().IsArray && objData.GetType() != typeof(Set))
				{
					set.Add(new dynamic[] { objData });
					value = set;
				}
				
			}
			
			else throw new ISqlTypeException($"Error: this is an unsupported type\nType: '{type.ToString()}'\nexptecting: '{data.GetType()}'\nvalue: '{data}'");
			
			return value;
		}

		public static string[] ColumnDetector(string column)
		{
			column = column.Trim();
			string[] name = new string[2];
			if (column.StartsWith("@") && column.Count(x => x == '.') == 1)
			{
                //Console.WriteLine($"-------'{column}'");
				name[0] = column.Substring(1, column.IndexOf(".") - 1);
				name[1] = column.Substring(column.IndexOf(".") + 1);
			}
			else
			{
				name[0] = "`";
				name[1] = column;
			}
			return name;
		}

		public static bool IsStringType(object data)
		{
			if (data.GetType() == typeof(string) || data.GetType() == typeof(Varchar) || data.GetType() == typeof(Text))
				return true;

			return false;
		}

		public static Encoding GetCharEncoding(CharEncoding charSet)
		{
			switch (charSet)
			{
				case CharEncoding.UTF7:
					return Encoding.UTF7;
					break;
				case CharEncoding.UTF8:
					return Encoding.UTF8;
					break;
				case CharEncoding.UTF32:
					return Encoding.UTF32;
					break;
				case CharEncoding.ASCII:
					return Encoding.ASCII;
					break;
				case CharEncoding.Unicode:
					return Encoding.Unicode;
					break;
				case CharEncoding.BigEndianUnicode:
					return Encoding.BigEndianUnicode;
					break;
				default:
					return Encoding.Default;
			}
		}
	}
}