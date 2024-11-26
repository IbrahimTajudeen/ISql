using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


namespace Isac.Isql.Logistics
{
	/// <summary>
	/// Represents the ISql Arithmentic Expression Engine
	///	use for mathematical calculations.
	/// </summary>
	public sealed class ExpressionEngine
	{
		private Dictionary<string, string[]> SymTable = new Dictionary<string, string[]>()
		{
			{"@",new string[] {"_abcdefghijklmnopqrstuvwxyz0123456789", ".()+-/*,^%]"} },
			{"name", new string[] { "_abcdefghijklmnopqrstuvwxyz0123456789", "+-*%^,/()]" } },
			{"number", new string[] {"1234567890.", "+-*/^)%],"} },
			{"+", new string[] { "1234567890@_abcdefghijklmnopqrstuvwxyz-(^`'"  } },
			{"-", new string[] { "1234567890@_abcdefghijklmnopqrstuvwxyz+(^'`" } },
			{"*", new string[] {"1234567890@_abcdefghijklmnopqrstuvwxyz+-`'(" } },
			{"/", new string[] {"1234567890@_abcdefghijklmnopqrstuvwxyz+-('`" } },
			{"%", new string[] {"1234567890@_abcdefghijklmnopqrstuvwxyz+-`'(" } },
			{"^",new string[] {"1234567890@_abcdefghijklmnopqrstuvwxyz+-(`'"} },
			{".", new string[] { "_abcdefghijklmnopqrstuvwxyz1234567890" , "^+-,*/%)" } },
			{"(", new string[] { "@_abcdefghijklmnopqrstuvwxyz12345678909(+-`')["} },
			{")", new string[] { "],+-)^*/%" } },
			//Newly Added
			{"value",new string[] {">=<!+]-*/^%&|),"} },
			{",", new string[] { "@_abcdefghijklmnopqrstuvwxyz0123456789([+-'`"} },
			{"[",new string[] { "@_abcdefghijklmnopqrstuvwxyz0123456789('`[]@-+" } },
			{"]", new string[] {",])"} },
		};
		private dynamic result = "";
		
		Parser p = new Parser();
		Fundamentals fundas = new Fundamentals();
		
		//for the epression been passed i.e.  new List<dynamic> { 2, "+", 5 or a, "-", b};
		private List<string> Exp_tree = new List<string>();
		private List<string> Exp_tree1 = new List<string>();
		private List<dynamic> broken_data = new List<dynamic>();

		private Dictionary<string, object[]> variable_names = new Dictionary<string, object[]>();
		private string math_Expression = "";

		/// <summary>
		/// initialize a new instance of the  Expression class
		/// </summary>
		public ExpressionEngine() { }
		
		//optional constructor
		/// <summary>
		/// initialize a new instance of the  Expression class, with a default argument.
		/// </summary>
		/// <param name="math_Expression">
		/// the default arguement passed to the instance
		/// </param>
		public ExpressionEngine(string math_Expression)
		{
			this.math_Expression = math_Expression;
			Break_Expression(this.math_Expression);
		}

		/// <summary>
		/// checks if the instance have any variable
		/// </summary>
		public bool HasVariables()
		{
			return variable_names.Count > 0;
		}
		
		public bool HasVariable(string name)
		{
			return variable_names.Keys.Contains(name);
		}
		
		/// <summary>
		/// to set or override the former arugumement passed
		/// </summary>
		/// <param name="math_Expression">the default arguement passed</param>
		public void Arguements(string math_Expression)
		{
			this.math_Expression = math_Expression;
			Break_Expression(this.math_Expression);
		}
		/// <summary>
		/// to add more arugumement to the already existing arguements passed
		/// </summary>
		/// <param name="math_Expression">mathematical arguements to pass</param>
		public void AddArguments(string math_Expression)
		{

			if ((math_Expression.StartsWith("+") || math_Expression.StartsWith("-") || math_Expression.StartsWith("*") || math_Expression.StartsWith("/") || math_Expression.StartsWith("^") || math_Expression.StartsWith("%")) && this.math_Expression != "")
			{
				this.math_Expression += " " + math_Expression;
				this.Break_Expression(this.math_Expression);
				return;
			}
			else if ((math_Expression.StartsWith("+") || math_Expression.StartsWith("-") || math_Expression.StartsWith("*") || math_Expression.StartsWith("/") || math_Expression.StartsWith("^") || math_Expression.StartsWith("%")) && this.math_Expression == "")
			{
				this.math_Expression = math_Expression;
				Break_Expression(this.math_Expression);
				return;
			}
			throw new ISqlException($"Error: the new arguement can not be joined to the existing one");
		}

		/// <summary>
		/// use to parse the the argument passed
		/// </summary>
		/// <param name="math_Expression"></param>
		private void Break_Expression(string math_Expression)
		{
			variable_names.Clear(); broken_data.Clear(); result = ""; _result = "";
			string valid_chars = "@_abcdefghijklmnopqrstuvwxyz0123456789.[]`( )+-*/%^";
			
			math_Expression = math_Expression.Trim();
			math_Expression = fundas.SpaceRemover(math_Expression, valid_chars);
											
			int operCount = 0, brkCount = 0, square_brackets = 0, func_braces_count = 0;

			dynamic[] obj = new dynamic[4];
			string parent = "";
			int prev_index = -1, opb_Count = math_Expression.Count(x => (x == '(')), clb_Count = math_Expression.Count(x => (x == ')'));
			string holder = "";
			bool in_function = false, in_array = false, in_dot = false;

			//if (opb_Count != clb_Count)
			//throw new ISqlException($"Error: not all brackets have pairs");
			
			string[] str = { "Values:=> ", "NextValue:=> ", "Anno:=> ", "Index:=> " };
			
			for (int i = 0; i < math_Expression.Length;)
			{
				//for  first start
				if (parent == "")
				{
					if(((byte)math_Expression[i]) == ((byte)15))
						i++;
						
					parent = p.GetType(math_Expression[i]);
					prev_index = i; operCount = 0;
				}
				
			_AT:
				if (parent == "@")
				{
					//0     1      2      3
					//val,nxtVal,anno, index
					
					obj = p.GetNextExpression(i, math_Expression, parent, SymTable);
					parent = obj[2].ToString();
					prev_index = i;
					i = Convert.ToInt32(obj[3]);
					operCount = 0;
					
					
                    if(parent == "(")
					{
						parent = "func";
						goto _FUNC;
					}
					
					//solve user defined keywords
					if(")+-/*^%],".Contains(parent))
					{
						//Calling and Adding Keyword value
						broken_data.Add(ObjectLoader.Caller(obj[0].ToString(),new object[] {}, As.KeywordObject, "custom"));
						Exp_tree.Add("USR");
						if ((parent == "+" || parent == "-" || parent == "*" || parent == "/" || parent == "%" || parent == "^") && i != -1)
							goto _MATH;
						else if (parent == ")" || (parent == "]" && in_function && in_array))
							goto _BRACKETS;
						else if (parent == "," && in_function)
							goto _COMMA;
						else if(i == -1 || i >= math_Expression.Length)
							break;
						else
							throw new ISqlSyntaxException($"Error: error syntax '@'");
					
					}
                    
					holder += "@" + obj[0].ToString();
					
					if (parent == "." && i != -1)
						goto _DOT;
					else throw new ISqlSyntaxException($"Error: error syntax for maths operation\nvariables name syntax: @<aliasName>.<name>");

				}
				
			_DOT:
				if (parent == ".")
				{
					obj = p.GetNextExpression(i, math_Expression, parent, SymTable);

					holder += "." + obj[0].ToString();
					broken_data.Add(holder);
					Exp_tree.Add("VAR");
					if (!variable_names.ContainsKey(holder.ToString()))
						variable_names.Add(holder.ToString(), new object[] { string.Empty, false, "," + (broken_data.Count - 1).ToString() });
					else
					{
						object[] _prev_val = variable_names[holder];
						_prev_val[2] += "," + (broken_data.Count - 1).ToString();
						variable_names[holder] = _prev_val;
					}
					operCount = 0;
					holder = string.Empty;
					parent = obj[2].ToString();
					prev_index = i;
					i = Convert.ToInt32(obj[3]);
					 
					if ((parent == "+" || parent == "-" || parent == "*" || parent == "/" || parent == "%" || parent == "^") && i != -1)
						goto _MATH;
					else if (parent == ")")
						goto _BRACKETS;
					else if (parent == "," && in_function)
						goto _COMMA;
					else if (i == -1)
						break;
					else throw new ISqlSyntaxException($"Error: error syntax");
				}
				
				
			_COMMA: //Done Cool
				if(parent == ",")
				{
					if(!in_function)
						throw new ISqlSyntaxException($"Error: invalid token ','");
						
					if(i == -1 || i >= math_Expression.Length - 1)
						throw new ISqlSyntaxException($"Error: expecting a list");
					
					obj = p.GetNextExpression(i, math_Expression, parent, SymTable);
					
					broken_data.Add(obj[0]);
					Exp_tree.Add("COM");
					parent = obj[2].ToString();
					i = int.Parse(obj[3].ToString());
					
					switch(parent)
					{
						case "@":
							goto _AT;
							break;
						case "name":
							goto _NAME;
							break;
						case "number":
							goto _NUMBER;
							break;
						case "+":
						case "-":
							goto _MATH;
							break;
						case "value":
						case "`":
							goto _VALUE;
							break;
						case "(":
							goto _BRACKETS;
							break;
						case "[":
							if(in_function)
								goto  _BRACKETS;
							throw new ISqlSyntaxException($"ERROR: can not declear an array outside a function parameter");
						default:
							throw new ISqlSyntaxException($"Error: error syntax Parent 'COMMA'");
					}
				}
						
			_FUNC: //Comming Soon DOne Cool
				if(parent == "func")
				{
					
					Exp_tree.Add("FUNC");
					broken_data.Add(obj[0].ToString());
					
					in_function = true;
					//func_braces_count++;
					parent = p.GetType(math_Expression[i]);
					
					if(parent == "(")
						goto _BRACKETS;
						
					throw new ISqlSyntaxException($"Error: error syntax Parent '{parent}'");
				}
			
			_VALUE:
				if (parent == "value")
				{
					operCount = 0;

					obj = p.GetNextExpression(i, math_Expression, parent, this.SymTable);
					
					Value ival = new Value(obj[0]);
					broken_data.Add(ival.Data);
					Exp_tree.Add(ival.Anno);

					parent = obj[2].ToString();
					prev_index = i;
					i = Convert.ToInt32(obj[3]);

					if ((parent == "+" || parent == "-" || parent == "*" || parent == "/" || parent == "%" || parent == "^") && i != -1)
						goto _MATH;
					else if (parent == ")" || (parent == "]" && in_function && in_array))
						goto _BRACKETS;
					else if (parent == "," && in_function)
						goto _COMMA;
					else if (i == -1)
						break;
					else
						throw new ISqlSyntaxException($"Error: error syntax");
				}
				
			_NAME:
				if (parent == "name")
				{
					obj = p.GetNextExpression(i, math_Expression, parent, SymTable);

					parent = obj[2].ToString();
					prev_index = i;
					i = Convert.ToInt32(obj[3]);
					operCount = 0;
					 
					if(parent == "(")
					{
						parent = "func";
						goto _FUNC;
					}
					
					if(BuiltIns.KEYWORDLIST.ToList<string>().Contains(obj[0].ToString().ToUpper()))
					{
						var m = ObjectLoader.Caller(obj[0].ToString().ToUpper(), null, As.KeywordObject);
						broken_data.Add(m);
						Value val = new Value(m);
						Exp_tree.Add(val.Anno);
					}
					else
					{
						//logic_tree.Add("VAR");
						broken_data.Add(obj[0].ToString());
						Exp_tree.Add("VAR");
						if (!variable_names.ContainsKey(obj[0].ToString()))
							variable_names.Add(obj[0].ToString(), new object[] { string.Empty, false, broken_data.Count - 1 });
						else
						{
							object[] _prev_val = variable_names[obj[0].ToString()];
							_prev_val[2] += "," + (broken_data.Count - 1).ToString();
							variable_names[obj[0].ToString()] = _prev_val;
						}
					}

					if ((parent == "+" || parent == "-" || parent == "*" || parent == "/" || parent == "%" || parent == "^") && i != -1)
						goto _MATH;
					else if (parent == ")")
						goto _BRACKETS;
					else if (parent == "," && in_function)
						goto _COMMA;
					else if (i == -1)
						break;
					else throw new ISqlSyntaxException($"Error: error syntax");
				}
				
			_NUMBER:
				if (parent == "number")
				{
					obj = p.GetNextExpression(i, math_Expression, parent, this.SymTable);
					broken_data.Add((obj[0].ToString().Contains(".")) ? Convert.ToDouble(obj[0]) : Convert.ToInt64(obj[0]));
					Exp_tree.Add("NUM");
					parent = obj[2].ToString();
					prev_index = i;
					i = Convert.ToInt32(obj[3]);
					operCount = 0;
					 
					if ((parent == "+" || parent == "-" || parent == "*" || parent == "/" || parent == "%" || parent == "^") && i != -1)
						goto _MATH;
					else if (parent == ")" && i != -1)
						goto _BRACKETS;
					else if (parent == "," && in_function)
						goto _COMMA;
					else if (i == -1)
						break;
					else throw new ISqlSyntaxException($"Error: error syntax");
				}
				
			_MATH:
				if (parent == "+" || parent == "-" || parent == "*" || parent == "/" || parent == "%" || parent == "^")
				{
					obj = p.GetNextExpression(i, math_Expression, parent, this.SymTable);
					if (operCount > 1)
						throw new ISqlException($"Error: can not join multiple arithmetic symbols together highest is two '{string.Join(" ", broken_data)}'");
					
					if(parent == "+" || parent == "-")
						++operCount;
						
					broken_data.Add(obj[0].ToString());
					Exp_tree.Add("ART");
					parent = obj[2].ToString();
					prev_index = i;
					i = Convert.ToInt32(obj[3]);
					 
					if ((parent == "+" || parent == "-") && i != -1)
						goto _MATH;
					else if (parent == "name" && i != -1)
						goto _NAME;
					else if (parent == "number" && i != -1)
						goto _NUMBER;
					else if (parent == "(" && i != -1)
						goto _BRACKETS;
					else if (parent == "@" && i != -1)
						goto _AT;
					else if (parent == "value" && i != -1)
						goto _VALUE;
					else if (i == -1)
						throw new ISqlSyntaxException($"Error: uprand needed");
					else throw new ISqlSyntaxException($"Error: error syntax");

				}
				
			_BRACKETS:
				if (parent == "(" || parent == ")" || parent == "["|| parent == "]")
				{
					
					obj = p.GetNextExpression(i, math_Expression, parent, this.SymTable);
					operCount = 0;
					string prev_parent = parent;
					
					if (parent == "(")
						Exp_tree.Add("OB");
					else if(parent == ")")
						Exp_tree.Add("CB");
					else if(parent == "[")
						Exp_tree.Add("OSB");
					else 
						Exp_tree.Add("CSB");
					
					if (parent == "(")
						++brkCount;
					else if (parent == ")")
						--brkCount;
						
					if(in_function)
					{
						if (parent == "(")
							++func_braces_count;
							
						else if (parent == ")")
							--func_braces_count;
							
						if(parent == "[")
						{
							in_array = true;
							++square_brackets;
							++func_braces_count;
						}
						else if (parent == "]")
						{
							--square_brackets;
							--func_braces_count;
							if(square_brackets == 0)
								in_array = false;
						}
						
						if(func_braces_count == 0 && !in_array)
							in_function = false;
							
						if(func_braces_count < 0 || square_brackets < 0)
							throw new ISqlSyntaxException($"Error: brackets are placed in incorrect order\nOSB: '{square_brackets}'\nFUNCB: '{func_braces_count}'");
						
						if(func_braces_count == 0 && in_array)
							throw new ISqlSyntaxException($"Error: function closed array open");
					}
						
					if (brkCount < 0) throw new ISqlException($"Error: brackets are placed in incorrect order");

					//broken_data.Add(obj[0]);
					parent = obj[2].ToString();
					prev_index = i;
					i = Convert.ToInt32(obj[3]);
					 
					if(parent == ")" && prev_parent == "(" && !in_function)
						throw new ISqlSyntaxException($"Error: can not have empty brackets");
					
					else if(parent == ")" && prev_parent == "(" && p.GetType(math_Expression[i - 2]) != "name" && p.GetType(math_Expression[i - 2]) != "number")
						throw new ISqlSyntaxException($"Error: can not have empty brackets");
					
					if((prev_parent == "[" || prev_parent == ",") && !in_function)
						throw new ISqlException($"Error: can not have an array outside function parameter");
					
					broken_data.Add(obj[0]);
					
					if (((parent == "+" || parent == "-") && i != -1) || (parent == "*" || parent == "/" || parent == "%" || parent == "^" && prev_parent == ")" && i != -1))
						goto _MATH;
					else if (parent == "name" && i != -1)
						goto _NAME;
					else if (parent == "number" && i != -1)
						goto _NUMBER;
					else if (parent == "(" || parent == ")" && i != -1)
						goto _BRACKETS;
					else if (parent == ")" || (parent == "[" && in_function))
						goto _BRACKETS;
					else if (parent == "]" && in_array)
						goto _BRACKETS;
					else if (parent == "@" && i != -1)
						goto _AT;
					else if (parent == "value" && (prev_parent == "(" || prev_parent == "["))
						goto _VALUE;
					else if (parent == "," && in_function && (prev_parent == ")" || prev_parent == "]"))
						goto _COMMA;
					else if (i == -1)
						break;// throw new ISqlSyntaxException($"Error: uprand needed");
					else throw new ISqlSyntaxException($"Error: error syntax '{parent} {i} {math_Expression}'");

				}
				
				throw new ISqlException($"Error: unknown error occured...\nParser Error");
			}
			
			if (brkCount != 0)
				throw new ISqlException($"Error: unknown error occured");
			
		}
		
        /// <summary>
		/// use to override a variable's values if it has one or give it a new value., it throws ISqlException if the variable do not exists
		/// </summary>
		/// <param name="var_name">the variable name to set a value for</param>
		/// <param name="value">the value to replace the variable</param>
		public void ChangeValue(string var_name, object value)
		{
			if (!variable_names.ContainsKey(var_name))
				throw new ISqlException($"Error: no variable have the name '{var_name}'");

			object[] prev_val = variable_names[var_name];
			//this list holds the indexs of the variables 
			List<int> indes = new List<int>();	
			variable_names[var_name] = new object[] { value, true, prev_val[2] };
			foreach (var i in prev_val[2].ToString().Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries))
				indes.Add(int.Parse(i));
			
			foreach (var i in indes)
			{
				Value value1 = new Value(value);
				broken_data[i] = value1.Data;
				Exp_tree[i] = value1.Anno;
			}
		
		}
		
		/// <summary>
		/// use to set or override multiple variables in the arguement tree, it throws an ISqlException if the variable the value or do not exists
		/// </summary>
		/// <param name="name_values">the key of the dictionary represent the variable names, while the values of the dictionary represent the values given to that variable</param>
		public void ChangeValues(Dictionary<string, object> name_values)
		{
			foreach (var key in name_values.Keys)
				ChangeValue(key, name_values[key]);
		}
		
		/// <summary>
		/// use to set variable value, it throws an ISqlException if the variable already have a value or do not exists
		/// </summary>
		/// <param name="var_name">the variable name to set a value for</param>
		/// <param name="value">the value to replace the variable</param>
		public void SetValue(string var_name, object value)
		{
			List<object> new_obj = new List<object>();
			if (variable_names.ContainsKey(var_name))
			{
				object[] prev_val = variable_names[var_name];
				if (Convert.ToBoolean(prev_val[1]) == false)
				{
					ChangeValue(var_name, value);
				}
				else throw new ISqlException($"Error: the variable '{var_name}' already have a value");
			}
			else throw new ISqlException($"Error: no variable have the name '{var_name}'");
		}

		/// <summary>
		/// use to set multiple variables in the arguement tree, it throws an ISqlException if the variable already have a value or do not exists
		/// </summary>
		/// <param name="name_values">the key of the dictionary represent the variable names, while the values of the dictionary represent the values given to that variable</param>
		public void SetValues(Dictionary<string, object> name_values)
		{
			foreach (var key in name_values.Keys)
				SetValue(key, name_values[key]);
		}
		
		public object GetValue(string variable)
		{
			if(variable_names.ContainsKey(variable))
				return variable_names[variable][0];
			
			throw new ISqlException($"Error: the variable '{variable}' is not define");
		}
		
		public object[] GetValues(params string[] variables)
		{
			List<object> objs = new List<object>();
			foreach(var v in variables)
			{
				objs.Add(GetValue(v));
			}
			return objs.ToArray<object>();
		}
		
        public string[] GetVariables()
        {
        	List<string> names = new List<string>();
        	foreach(string var_names in variable_names.Keys)
        	{
        		object[] value = variable_names[var_names];

        			names.Add(var_names);
        	}
        	return names.ToArray<string>();
        }
        
        public void SetVariables(string[] names, object[] values)
        {
        	if(names.Length == values.Length && names.Length == variable_names.Count)
        	{
        		Dictionary<string, object> dic = new Dictionary<string, object>();
        		int index = 0;
        		foreach(string name in names)
        		{
        			ChangeValue(name, values[index]);
        			index++;
        		}
        	}
        	else throw new ISqlException($"Error: length issues\nnames Length: '{names.Length}'\nvalues Length: '{values.Length}'\nvariable_name Length: '{variable_names.Count}");
        }
		
		public object[] GetExpression()
		{
			List<object> gExp = new List<object>();
			int count = -1;
			foreach(var d in broken_data)
			{
				count++;
				if(Exp_tree[count] == "EVL" || Exp_tree[count] == "DAT" || Exp_tree[count] == "BOL")
				{
					gExp.Add("`" + d + "`"); continue;
				}
				else if (Exp_tree[count] == "STR" || Exp_tree[count] == "NEVL")
				{
					gExp.Add("'" + d + "'"); continue;
				}
				
				gExp.Add(d);
			}
			
			return gExp.ToArray<object>();
		}
		
		public string GetStringExpression()
		{
			return string.Join(" ", GetExpression());
		}
		
		public void SetExpression(object[] expression)
		{
			broken_data = expression.ToList<dynamic>();
		}
		
		private void RearrangeLogicTree(List<dynamic> data, ref List<string> tree)
		{
			tree.Clear();
			int c = -1;
			foreach (var d in data)
			{
                var item = d;
				c++;
				
				if (item.ToString() == "(")
				{
					tree.Add("OB");
					continue;
				}
				
				else if (item.ToString() == ")")
				{
					tree.Add("CB");
					continue;
				}
				
				else if (item.ToString() == "[")
				{
					tree.Add("OSB");
					continue;
				}
				
				else if (item.ToString() == "]")
				{
					tree.Add("CSB");
					continue;
				}
				
				else if (item.ToString() == "+" || item.ToString() == "-" || item.ToString() == "*" || item.ToString() == "/" || item.ToString() == "%" || item.ToString() == "^")
				{
					tree.Add("ART");
					continue;
				}
				
				else if (item.ToString() == ">" || item.ToString() == ">=" || item.ToString() == "<" || item.ToString() == "<=" || item.ToString() == "<>")
				{
					tree.Add("REL");
					continue;
				}
				
				else if (item.ToString() == "==" || item.ToString() == "!=")
				{
					tree.Add("EQL");
					continue;
				}
				
				else if (item.ToString() == "&&" || item.ToString() == "||")
				{
					tree.Add("LOG");
					continue;
				}
				
				else if (item.ToString() == "!")
				{
					tree.Add("NOT");
					continue;
				}
				
				else if (item.GetType() == typeof(bool))
				{
					tree.Add("BOL");
					continue;
				}
				
				else if (item.GetType() == typeof(DateTime) || item.GetType() == typeof(Date) || item.GetType() == typeof(Time))
				{
					tree.Add("DAT");
					continue;
				}
				
				else if (item.ToString().StartsWith("`") && item.ToString().EndsWith("`"))
				{
					tree.Add("EVL");
					continue;
				}
				
				else if (item.ToString().StartsWith("'") && item.ToString().EndsWith("'"))
				{
					tree.Add("NEVL");
					continue;
				}
				
				else if (item.ToString() == ",")
				{
					tree.Add("COM"); continue;
				}
				
				else if (item.GetType() == typeof(string) && variable_names.ContainsKey(item.ToString()))
				{
					tree.Add("VAR");
					continue;
				}
				
				else if (item.GetType() == typeof(string) && !variable_names.ContainsKey(item.ToString()))
				{
					if(c <= data.Count - 3)
					{
                        if (data[c + 1].ToString() == "(")
                        {
                            tree.Add("FUNC");
                        }
						continue;
					}
					
					tree.Add("STR");
					continue;
				}
				
				else if (item.GetType() != typeof(string))
				{
					Value v = new Value(item);
					tree.Add(v.Anno);
					continue;
				}
			}
		}
		
		#region mathematical functions

		internal List<dynamic> PreAdd(List<dynamic> data)
		{
			int index = -1;
			if (data.Contains("+"))
				index = data.IndexOf("+");
			
			ReTry:
			if (data.Contains("+"))
			{

				dynamic value = "";
				if (index == 0)
				{
					if (Parser.CheckNum(data[index + 1].ToString()))
					{
						value = (double) + (Parser.DataConverter(data[index + 1].GetType(), data[index + 1]));
						data.Insert(index, (double)value);
						data.MyRemoveRange(index + 1, 2); goto Go;
					}
					else if (data[index + 1].ToString() == "+" || data[index + 1].ToString() == "-")
					{
						data.RemoveAt(index); goto Go;
					}
				}
				else
				{
					dynamic pre_value = data[index - 1], nxt_value = data[index + 1];
					if (Parser.CheckNum(pre_value.ToString()) && Parser.CheckNum(nxt_value.ToString()))
						goto Go;

					else if (!Parser.CheckNum(pre_value.ToString()) && Parser.CheckNum(nxt_value.ToString()) && (pre_value.ToString() == "+" || pre_value.ToString() == "-" || pre_value.ToString() == "*" || pre_value.ToString() == "/"))
					{
						value = (double) + (Parser.DataConverter(nxt_value.GetType(), nxt_value));
						data.Insert(index, (double)value);
						data.MyRemoveRange(index + 1, 2); goto Go;
					}

					else if (Parser.CheckNum(pre_value.ToString()) && nxt_value.ToString() == "+")
					{
						data.RemoveAt(index); goto Go;
					}
					
					else if (pre_value.ToString() == "-" && nxt_value.ToString() == "+")
					{
						data.RemoveAt(index); data.RemoveAt(index); goto Go;
					}
					
					else if ((pre_value.ToString() == "+" && nxt_value.ToString() == "+") || (pre_value.ToString() == "+" && nxt_value.ToString() == "-") || (pre_value.ToString() == "-" && nxt_value.ToString() == "+"))
					{
						data.RemoveAt(index - 1); data.RemoveAt(index); goto Go;
					}
					
					else if ((pre_value.ToString() == "-" && nxt_value.ToString() == "-"))
					{
						data.RemoveAt(index - 1); data.RemoveAt(index); goto Go;
					}
					
					else if (!Parser.CheckNum(pre_value.ToString()) && (nxt_value.ToString() == "-" || nxt_value.ToString() == "+"))
					{
						data.RemoveAt(index); goto Go;
					}
					
					else if (Parser.CheckNum(pre_value.ToString()) && nxt_value.ToString() == "-")
					{
						data.RemoveAt(index); goto Go;
					}
				}
			Go: 
				++index;
				for (int i = index; i < data.Count; i++)
				{
					if (data[i].ToString() == "+")
					{
						index = i; goto ReTry;
					}
				}
			}
			
			return data;
		}

		internal List<dynamic> PreSub(List<dynamic> data)
		{
			if (data.Contains("-"))
			{
				int index = data.IndexOf("-");
			ReTry:
				dynamic value = "";
				if (index == 0)
				{	
					if (Parser.CheckNum(data[index + 1].ToString()))
					{
						value = (double) - (Parser.DataConverter(data[index + 1].GetType(), data[index + 1]));
						data.Insert(index, (double)value);
						data.MyRemoveRange(index + 1, 2); goto Go;
					}
					if (data[index + 1].ToString() == "-")
					{
						data.Insert(0, "+");
						data.RemoveAt(index + 1); data.RemoveAt(index + 1); goto Go;
					}
				}
				else
				{
					dynamic pre_value = data[index - 1], nxt_value = data[index + 1];
					if (Parser.CheckNum(pre_value.ToString()) && Parser.CheckNum(nxt_value.ToString()))
						goto Go;
					if (!Parser.CheckNum(pre_value.ToString()) && Parser.CheckNum(nxt_value.ToString()) && (pre_value.ToString() == "+" || pre_value.ToString() == "-" || pre_value.ToString() == "*" || pre_value.ToString() == "/"))
					{
						value = -0 - (double)nxt_value;
						data.Insert(index, (double)value);
						data.MyRemoveRange(index + 1, 2); goto Go;
					}

					if ((pre_value.ToString() == "-" && nxt_value.ToString() == "-") || (pre_value.ToString() == "+" && nxt_value.ToString() == "+") || (pre_value.ToString() == "+" && nxt_value.ToString() == "-"))
					{
						data.Insert(index - 1, "+"); data.RemoveRange(index, 2); goto Go;
					}
					if (pre_value.ToString() == "-" && nxt_value.ToString() == "+")
					{
						data.MyRemoveRange(index, 2); goto Go;
					}
					if (!Parser.CheckNum(pre_value.ToString()) && nxt_value.ToString() == "-")
					{
						data.RemoveAt(index); data.RemoveAt(index); goto Go;
					}

				}
			Go: 
				++index;
				for (int i = index; i < data.Count; i++)
				{
					if (data[i].ToString() == "-")
					{
						index = i; goto ReTry;
					}
				}
			}
			
			return data;
		}

		internal List<dynamic> Brks(List<dynamic> data)
		{
			Console.WriteLine("BRKSEXP: \n" + string.Join(" ", data));
			Brks:
			if (data.Contains("("))
			{
				
				int index = data.MyLastIndexOf("("), end = 0;
				//end = data.GetEnclose("(", ")", index, 0);
				
				for (int i = index; i < data.Count; i++) { if (data[i].ToString() == ")") { end = i; break; } }
				
				List<dynamic> range = new List<dynamic>();
				range.AddRange(data.GetRange(index + 1, (end - index) - 1));
				Console.WriteLine("CalBrks: \n" + string.Join(" ", range));
				if(range.Contains("+"))
					range = PreAdd(range);
				if(range.Contains("-"))
					range = PreSub(range);
				if(range.Contains("^"))
					range = Pow(range);
				if(range.Contains("*"))
					range = Multi(range);
				if(range.Contains("/"))
					range = Div(range);
				if(range.Contains("%"))
					range = Mod(range);
				if(range.Contains("+"))
					range = Add(range);
				if(range.Contains("-"))
					range = Sub(range);
			
				if (range.Count != 1)
					throw new ISqlArithemeticException($"Error: in brackets\nExpression: '{string.Join(" ", range)}'");
				
				data.MyRemoveRange(index + 1, (end - index) - 1);
				
				data.Insert(data.LastIndexOf("(") + 1, range[0]);
				index = data.LastIndexOf("(");
				data.RemoveAt(index + 2); data.RemoveAt(index);
				
				goto Brks;
			}
		
			return data;
		}

		internal List<dynamic> Pow(List<dynamic> data)
		{
			
			Pow:
			if (data.Contains("^"))
			{
				
				int index = data.IndexOf("^");
				dynamic pre_value = data[index - 1];
				dynamic nxt_value = data[index + 1];
				dynamic value = "";
				if (Parser.CheckNum(pre_value.ToString()) && Parser.CheckNum(nxt_value.ToString()))
				{
					unchecked
					{
						value = (double)Math.Pow((double)pre_value, (double)nxt_value);
					}
					data.Insert(index - 1, (double)value);
					data.MyRemoveRange(index, 3);
				}
				
				goto Pow;
			}
			
			return data;
		}

		internal List<dynamic> Div(List<dynamic> data)
		{
			
			Div:
			if (data.Contains("/"))
			{
			
				int index = data.IndexOf("/");
				dynamic pre_value = data[index - 1];
				dynamic nxt_value = data[index + 1];
				dynamic value = "";
				if (Parser.CheckNum(pre_value.ToString()) && Parser.CheckNum(nxt_value.ToString()))
				{
					if (nxt_value != 0 || nxt_value != 0.0)
					{
						unchecked
						{
							value = (double)pre_value / (double)nxt_value;
						}
					}
					else
						throw new ISqlArithemeticException($"Error: divide by zero occured in your expression\nExpression: '{string.Join(" ", data)}'");
				}
				else throw new ISqlException($"Error: both uprands of '/' operator are not integers or floating points. '{pre_value}' '{nxt_value}'\nEngine error!!!");
				data.Insert(index - 1, (double)value);
				data.MyRemoveRange(index, 3);
			
				goto Div;
			}
			
			return data;
		}

		internal List<dynamic> Mod(List<dynamic> data)
		{
			
			Mod:
			if (data.Contains("%"))
			{
			
				int index = data.IndexOf("%");
				dynamic pre_value = data[index - 1];
				dynamic nxt_value = data[index + 1];
				dynamic value = "";

				unchecked
				{
					value = (double)pre_value % (double)nxt_value;
				}
				data.Insert(index - 1, (double)value);
				data.MyRemoveRange(index, 3);
			
				goto Mod;
			}
			
			return data;
		}

		internal List<dynamic> Multi(List<dynamic> data)
		{
			Multi:
			if (data.Contains("*"))
			{
				
				int index = data.IndexOf("*");
				dynamic pre_value = data[index - 1];
				dynamic nxt_value = data[index + 1];
				dynamic value = "";
				unchecked
				{
					value = pre_value * nxt_value;
				}
				data.Insert(index - 1, value);
				data.MyRemoveRange(index, 3);
				
				goto Multi;
			}
			
			return data;
		}

		internal List<dynamic> Add(List<dynamic> data)
		{
			
			Add:
			dynamic value = "";
			if (data.Contains("+"))
			{
				
				int index = data.MyIndexOf("+");

				try
				{
					//for adding + and a number i.e + 3
					if (index == 0)
					{
						value = (double)+(data[index + 1]);
						data.Insert(index, value);
						data.MyRemoveRange(index + 1, 2);
					}
					
					//for adding numbers
					else if (Parser.CheckNum(data[index - 1].ToString()) && Parser.CheckNum(data[index + 1].ToString()))
					{
						dynamic pre_value = data[index - 1];
						dynamic nxt_value = data[index + 1];
						int rancount = 3;
						
						if(index - 2 < 0 == false)
						{
							if(data[index - 2] == "-")
							{
								pre_value = - 0 -pre_value;
								data.RemoveAt(index - 2);
								 index--;
							}
							else if (data[index - 2] == "+")
							{
								pre_value = +0 +pre_value;
							}
							else
								pre_value = +0 +pre_value;
						}
						
						unchecked
						{
							value = (double)pre_value + (double)nxt_value;
						}
						
						
						data.Insert(index - 1, value);
						
						data.MyRemoveRange(index, rancount);
					}
					
					else if(data.Count >= 3)
					{
						//for adding strings
						if (Parser.IsStringType(data[index - 1]) || Parser.IsStringType(data[index + 1]))
						{
							dynamic pre_value = data[index - 1];
							dynamic nxt_value = data[index + 1];
							value =  pre_value + nxt_value;
							data.Insert(index - 1, value);
							data.MyRemoveRange(index, 3);
						}
					
						//for adding dates
						else if (data[index - 1].GetType() == typeof(DateTime) && data[index + 1].GetType() == typeof(DateTime))
						{
							dynamic pre_value = data[index - 1];
							dynamic nxt_value = data[index + 1];
		
							value = pre_value.Add(((DateTime)nxt_value).Ticks);
							data.Insert(index - 1, value);
							data.MyRemoveRange(index, 3);
						}
						else throw new ISqlArguementException($"Error: incompactable uprands '{data[index - 1].GetType()} and {data[index + 1].GetType()}'");
					}
					
					else throw new ISqlArguementException($"Error: incompactable uprands '{data[index - 1].GetType()} and {data[index + 1].GetType()}'");
				}
				catch (Exception ex)
				{
					throw new ISqlArithemeticException($"Error: {ex.Message}\ncould not solve the expression: '{string.Join(" ", data)}' index: '{index}'");
				}
				
				goto Add;
			}
			else if (!data.Contains("+"))
			{
				
				for(var i = 0; i < data.Count; i++)
				{
					if(i > 0 && Parser.CheckNum(data[i].ToString()))
					{
						if(Parser.CheckNum(data[i - 1].ToString()))
						{
							dynamic cd = (data[i].ToString().Contains(".")) ? double.Parse(data[i].ToString()) : long.Parse(data[i].ToString());
							dynamic pd = (data[i - 1].ToString().Contains(".")) ? double.Parse(data[i - 1].ToString()) : long.Parse(data[i - 1].ToString());
							value = pd + cd;
							data.Insert(i - 1, value);
							data.RemoveAt(i);
							data.RemoveAt(i);
							i--;
						}
					}
				}
				
			}
			
			return data;
		}

		internal List<dynamic> Sub(List<dynamic> data)
		{
			
			Sub:
			int index = data.IndexOf("-");
			dynamic value = "";
			if (data.Contains("-"))
			{
				if (index == 0)
				{
					value = -0 - ((double)data[index + 1]);
					data.Insert(index, value);
					data.MyRemoveRange(index + 1, 2);
				}
				else if (Parser.CheckNum(data[index - 1].ToString()) && Parser.CheckNum(data[index + 1].ToString()))
				{
					dynamic pre_value = data[index - 1];
					dynamic nxt_value = data[index + 1];
					int rancount = 3;
					if(index - 2 < 0 == false)
					{
						if(data[index - 2] == "-")
						{
							pre_value = - 0 -pre_value;
							data.RemoveAt(index - 2);
							 index--;
						}
						else if (data[index - 2] == "+")
						{
							pre_value = +0 +pre_value;
						}
						else
							pre_value = +0 +pre_value;
					}
					
					unchecked
					{
						value = (double)pre_value - (double)nxt_value;
					}
					data.Insert(index - 1, value);
					data.MyRemoveRange(index, rancount);
				}
				else if ((data[index - 1].GetType() == typeof(string) && data[index + 1].GetType() == typeof(string)))
				{
					dynamic pre_value = data[index - 1];
					dynamic nxt_value = data[index + 1];
					if (pre_value.Contains(nxt_value))
						pre_value = pre_value.Replace(nxt_value, "");
					value = pre_value;
					throw new ISqlException($"Error: cannot subtract strings");
				}
				
				else if (data[index - 1].GetType() == typeof(DateTime) && data[index + 1].GetType() == typeof(DateTime))
				{
					dynamic pre_value = data[index - 1];
					dynamic nxt_value = data[index + 1];

					value = ((DateTime)pre_value).Subtract((DateTime)nxt_value);
					data.Insert(index - 1, value);
					data.MyRemoveRange(index, 3);
				}
				
				
				goto Sub;
			}
			else if (!data.Contains("-"))
			{
				
				for(var i = 0; i < data.Count; i++)
				{
					if(data[i].ToString().StartsWith("-") && i > 0)
					{
						dynamic pd = (data[i].ToString().Contains(".")) ? double.Parse(data[i].ToString().Substring(1)) : long.Parse(data[i].ToString().Substring(1));
						value = data[i - 1] - pd;
						data.Insert(i - 1, value);
						data.RemoveAt(i);
						data.RemoveAt(i);
						i--;
					}
				}
				
			}
			
			return data;
		}
        #endregion

        /// <summary>
        /// to rename a variable
        /// </summary>
        /// <param name="oldVar">the variable name to change</param>
        /// <param name="newVar">the new name to give the oldVar variable</param>
        public void RenameVariable(string oldVar, string newVar)
        {
            if (!variable_names.ContainsKey(oldVar))
                throw new ISqlException($"Error: no variable have the name '{oldVar}'");

            if (variable_names.ContainsKey(newVar))
                throw new ISqlException($"Error: {newVar} is an existing variable name");

            if (newVar.Contains("@") && newVar.Contains("."))
            {
                p.IsValidate(newVar.Substring(newVar.IndexOf(".")), "name");
                p.IsValidate(newVar.Substring(1, newVar.LastIndexOf(".") - 1), "name");
            }
            else if (!newVar.Contains("@") && !newVar.Contains("."))
                p.IsValidate(newVar, "name");
            else
                throw new ISqlException($"Error: new variable error");

            variable_names.Add(newVar, variable_names[oldVar]);

            //this list holds the indexs of the variables 
            if (bool.Parse(variable_names[oldVar][1].ToString()) == true)
                return;

            int[] var_indexs = variable_names[oldVar][2].ToString().Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList<string>().ConvertAll<int>(x => int.Parse(x)).ToArray();

            foreach (var i in var_indexs)
                broken_data[i] = newVar;
        }

        // returns a selectblock object
        internal SelectBlock GetSelectBlock()
		{
			SelectBlock selectBlock = new SelectBlock();
			selectBlock.Query = broken_data;
			selectBlock.QueryVariables = variable_names;
			selectBlock.Tree = Exp_tree;
			
			return selectBlock;
		}

        internal static ExpressionEngine GenerateExpressionEngine(SelectBlock block)
        {
            ExpressionEngine expEngine = new ExpressionEngine();
            expEngine.broken_data = block.Query;
            expEngine.variable_names = block.QueryVariables;
            expEngine.Exp_tree.AddRange(block.Tree);
            expEngine.Exp_tree1.AddRange(block.Tree);
            return expEngine;
        }
		
		/// <summary>
		/// to calculate the given arguement of the Expression class
		/// </summary>          
		public void Calculate()
		{
			bool can_Go_on = variable_names.Values.All(x => x[0].ToString().Trim() != string.Empty);
			
			string v_n = "";
			foreach(var vn in variable_names.Keys)
				v_n += vn +", ";
						
			if (!can_Go_on)
				throw new ISqlException($"Error: not all variable have a value\nVariable(s): '{v_n}'");
			
			if(Exp_tree.Count != broken_data.Count || Exp_tree1.Count != broken_data.Count || Exp_tree1.Count == 0)
				RearrangeLogicTree(broken_data, ref Exp_tree);
			
				List<dynamic> data = new List<dynamic>();
				Exp_tree1.Clear();
				data.AddRange(broken_data.GetRange(0, broken_data.Count));
				Exp_tree1.AddRange(Exp_tree.GetRange(0, Exp_tree.Count));
				
				FUNTION_START:
				if(Exp_tree1.Contains("FUNC"))
				{
					int index = Exp_tree1.LastIndexOf("FUNC");
					List<dynamic> args = new List<dynamic>();
					List<string> args_anno = new List<string>();
					int end = data.GetEnclose("(", ")", index + 2, 1);
					int arg_index = index + 2;
					args_anno.AddRange(Exp_tree1.GetRange(arg_index, end - arg_index));
					args.AddRange(data.GetRange(arg_index, end - arg_index));
					
					if(args_anno.Contains("STR") || args_anno.Contains("DAT") || args_anno.Contains("BOL") || args_anno.Contains("EVL") ||
					args_anno.Contains("NEVL"))
					{
						int co = 0;
						foreach(string anno in args_anno)
						{
							if(anno == "EVL" || anno == "DAT" || anno == "BOL")
								args[co] = "`" + args[co] + "`";
							else if (anno == "STR" || anno == "NEVL")
								args[co] = "'" + args[co] + "'";
								
							co++;
						}
					}
					
					object[] fargs = p.MethodArgsBuilder(args);
					
					
					string function = data[index];
					if(function.StartsWith("@"))
					{
						
						function = function.Remove(0, 1);
						
						var val = ObjectLoader.Caller(function, fargs, As.MethodObject, "custom");
						data.MyRemoveRange(index, end - index + 1);
						data.Insert(index, val);
						Exp_tree1.MyRemoveRange(index, end - index + 1);
						Exp_tree1.Insert(index, "VAL");
					}
					
					else if(!function.StartsWith("@"))
					{
						function = function.ToUpper();
					
						var val = ObjectLoader.Caller(function, fargs, As.MethodObject);
					
						data.MyRemoveRange(index, end - index + 1);
						data.Insert(index, val);
						Exp_tree1.MyRemoveRange(index, end - index + 1);
						Exp_tree1.Insert(index, "VAL");
					}
					goto FUNTION_START;
				}
				
				//BODMAS
				
				if (data.Contains("("))
					data = Brks(data);
				
				if(data.Contains("+"))
					data = PreAdd(data);
					
				if(data.Contains("-"))
					data = PreSub(data);
					
				if(data.Contains("^"))
				{
					if(data.Contains("+"))
						data = PreAdd(data);
					if(data.Contains("-"))
						data = PreSub(data);
					data = Pow(data);
				}
					
				if(data.Contains("*"))
				{
					if(data.Contains("+"))
						data = PreAdd(data);
					if(data.Contains("-"))
						data = PreSub(data);
					data = Multi(data);
				}
				if(data.Contains("/"))
				{
					if(data.Contains("+"))
						data = PreAdd(data);
					if(data.Contains("-"))
						data = PreSub(data);
					data = Div(data);
				}
					
				if(data.Contains("%"))
				{
					if(data.Contains("+"))
						data = PreAdd(data);
					if(data.Contains("-"))
						data = PreSub(data);
					data = Mod(data);
				}
					
				if(data.Contains("+"))
				{
					if(data.Contains("+"))
						data = PreAdd(data);
					if(data.Contains("-"))
						data = PreSub(data);
					data = Add(data);
				}
					
				if(data.Contains("-"))
				{
					if(data.Contains("+"))
						data = PreAdd(data);
					if(data.Contains("-"))
						data = PreSub(data);
					data = Sub(data);
				}
				
				if (data.Count == 1)
				{
					this.result = data[0];
					this.RawResult = data[0];
				}
				else if (data.Count > 1)
					throw new ISqlArithemeticException($"Error: could not solve expression properly\nExpression: '{string.Join(" ~ ",data)}'");
			
		}
		
		private dynamic _result = "";
		public dynamic RawResult
		{
			get
			{
				return _result;
			}
			internal set
			{
				_result = value;
			}
		}

		/// <summary>
		/// get the result calculated
		/// </summary>
		/// <param name="cast">the type you want to cast the value to</param>
		/// <param name="round">the type of rounding of the value</param>
		/// <returns></returns>
		public dynamic Result(CastType cast = CastType.Double, RoundType round = RoundType.None)
		{
			switch (cast)
			{
				case CastType.Float:
					switch (round)
					{
						case RoundType.Ceilling:
							return (float)(Math.Ceiling((decimal)result));
							break;
						case RoundType.Floor:
							return (float)(Math.Floor((decimal)result));
							break;
						case RoundType.None:
							return (float)result;
							break;
						case RoundType.Round:
							return (float)(Math.Round((decimal)result));
							break;
					}
					break;
				case CastType.Int:
					switch (round)
					{
						case RoundType.Ceilling:
							return (int)(Math.Ceiling((decimal)result));
							break;
						case RoundType.Floor:
							return (int)(Math.Floor((decimal)result));
							break;
						case RoundType.None:
							return (int)result;
							break;
						case RoundType.Round:
							return (int)(Math.Round((decimal)result));
							break;
					}
					break;
				case CastType.Double:
					switch (round)
					{
						case RoundType.Ceilling:
							return (double)(Math.Ceiling((decimal)result));
							break;
						case RoundType.Floor:
							return (double)(Math.Floor((decimal)result));
							break;
						case RoundType.None:
							return (double)result;
							break;
						case RoundType.Round:
							return (double)(Math.Round((decimal)result));
							break;
					}
					break;
				case CastType.Decimal:
					switch (round)
					{
						case RoundType.Ceilling:
							return (decimal)(Math.Ceiling((decimal)result));
							break;
						case RoundType.Floor:
							return (decimal)(Math.Floor((decimal)result));
							break;
						case RoundType.None:
							return (decimal)result;
							break;
						case RoundType.Round:
							return (decimal)(Math.Round((decimal)result));
							break;
					}
					break;
				case CastType.Long:
					switch (round)
					{
						case RoundType.Ceilling:
							return (long)(Math.Ceiling((double)result));
						case RoundType.Floor:
							return (long)(Math.Floor((double)result));
						case RoundType.None:
							return (long)result;
						case RoundType.Round:
							return (long)(Math.Round((double)result));
					}
					break;
			}
			return result;
		}
	}

	/// <summary>
	/// the casting type of a value
	/// </summary>
	public enum CastType
	{
		/// <summary>
		/// To Cast to int type
		/// </summary>
		Int,
		/// <summary>
		/// To Cast to float type
		/// </summary>
		Float,
		/// <summary>
		/// To Cast to double type
		/// </summary>
		Double,
		/// <summary>
		/// To Cast to decimal type
		/// </summary>
		Decimal,
		/// <summary>
		/// To Cast to long type
		/// </summary>
		Long
	}

	/// <summary>
	/// the type of rounding of the value
	/// </summary>
	public enum RoundType
	{
		/// <summary>
		/// Rounds a value to the nearest integral value.
		/// </summary>
		Round,
		/// <summary>
		/// Returns the smallest integral value that is greater than or equal to the specified
		/// value-precision floating-point number.
		/// </summary>
		Ceilling,
		/// <summary>
		/// Returns the largest integer less than or equal to the specified float number.
		/// </summary>
		Floor,
		/// <summary>
		/// Retruns original value of the number
		/// </summary>
		None
	}
}
