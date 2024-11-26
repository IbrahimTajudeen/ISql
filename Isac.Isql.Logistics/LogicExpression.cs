using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Isac.Isql.Logistics
{
	public sealed class LogicExpressionEngine
	{
		
		#region LogicExpression Table
		public Dictionary<string, string[]> SymTable = new Dictionary<string, string[]>()
		{
			{">",new string[] {"=", "(_abcdefghijklmnopqrstuvwxyz0123456789+-@'`"} },
			{"<",new string[] {"=>", "(_abcdefghijklmnopqrstuvwxyz0123456789+-@'`" } },
			{"=",new string[] {"(_abcdefghijklmnopqrstuvwxyz0123456789+-@`'" } },
			{"!=",new string[] {"(_abcdefghijklmnopqrstuvwxyz0123456789+-@`'"} },
			{">=",new string[] {"(_abcdefghijklmnopqrstuvwxyz0123456789+-@`'"} },
			{"<=",new string[] {"(_abcdefghijklmnopqrstuvwxyz0123456789+-@`'"} },
			{"<>",new string[] {"(_abcdefghijklmnopqrstuvwxyz0123456789+-@`'"} },
			{"&",new string[] {"(_abcdefghijklmnopqrstuvwxyz0123456789+-@`'!"} },
			{"|",new string[] {"(_abcdefghijklmnopqrstuvwxyz0123456789+-@`'!"} },
			{"!",new string[] {"=", "(_abcdefghijklmnopqrstuvwxyz0123456789+-@`!"} },
			{"value",new string[] {">=<!+]-*/^%&|),"} },
			{ "@",new string[] {"_abcdefghijklmnopqrstuvwxyz0123456789", ".()+-/*^%&>=!|<],"} },
			{"name", new string[] { "_abcdefghijklmnopqrstuvwxyz0123456789", "+-*%^,/()&|!><=]" } },
			{"number", new string[] {"1234567890.", "+-*/^)],%&|!><=" } },
			{"+", new string[] { "1234567890@_abcdefghijklmnopqrstuvwxyz-(`'"  } },
			{"-", new string[] { "1234567890@_abcdefghijklmnopqrstuvwxyz+(`'" } },
			{"*", new string[] {"1234567890@_abcdefghijklmnopqrstuvwxyz+-`'(" } },
			{"/", new string[] {"1234567890@_abcdefghijklmnopqrstuvwxyz+'-(`" } },
			{"%", new string[] {"1234567890@_abcdefghijklmnopqrstuvwxyz+-`'(" } },
			{"^",new string[] {"1234567890@_abcdefghijklmnopqrstuvwxyz+-(`'"} },
			{".", new string[] { "_abcdefghijklmnopqrstuvwxyz1234567890", "^+-,*]/%)&|!><=" } },
			{"(", new string[] { "@_abcdefghijklmnopqrstuvwxyz12345678909(+-`')[+-!"} },
			{")", new string[] { "],+-)^&|!><=*/%" } },
			//newly added
			{",", new string[] { "@_abcdefghijklmnopqrstuvwxyz0123456789!([+-`'"} },
			{"[",new string[] { "@_abcdefghijklmnopqrstuvwxyz0123456789(!`'[]@-+" } },
			{"]", new string[] {",])"} },
		};
		#endregion
		
		private Fundamentals funds = new Fundamentals();
		private Parser p = new Parser();
		
		private Collections.DataTable dt = null;
		
		internal event EventHandler<PortEvent> Port_Event;
		private void OnPortEvent(PortEvent e)
		{
			Port_Event?.Invoke(this, e);
		}
		
		//PORTS -		  <index>	-	<parameters>, <startIndex>, <endIndex>
		private Dictionary<int, dynamic[]> ports_funcs = new Dictionary<int, dynamic[]>();
		private Dictionary<string, object[]> variable_names = new Dictionary<string, object[]>();
		
		private List<dynamic> broken_data = new List<dynamic>();
		private List<string> logic_tree = new List<string>();
		private List<string> logic_tree1 = new List<string>();
		
		private dynamic rawres = "";
		private dynamic mathResult = "";
		private bool result = false;
		internal bool allow_port_func = false;
		private string arguement = "";
		
		public dynamic RawResult
		{
			get { return rawres; }
		}

		public bool Result
		{
			get { return result; }
		}

		public dynamic MathResult
		{
			get { return mathResult; }
			internal set { mathResult = value; }
		}
		
		private object[] GetNxtExp(int initialIndex, string data_to_check, string Parent, Dictionary<string, string[]> symTable)
		{
			return p.GetNext(initialIndex, data_to_check, Parent, symTable);
		}
		
		public LogicExpressionEngine()
		{
			variable_names = new Dictionary<string, object[]>();
			broken_data = new List<dynamic>();
			logic_tree = new List<string>();
			result = false;
			arguement = "";
		}
		
		public LogicExpressionEngine(string arguement)
		{
			this.arguement = arguement;
			Break_Expression(arguement);
		}
		
		internal LogicExpressionEngine(string arg, Collections.DataTable tb, bool allow_port)
		{
			this.arguement = arg; this.dt = tb;
			this.allow_port_func = allow_port;
			Break_Expression(arg);
		}

		public void LogicArguement(string arguement)
		{
			this.arguement = arguement;
			Break_Expression(arguement);
		}
		
		private void Break_Expression(string logic_Exp)
		{
			variable_names.Clear();
			broken_data.Clear();
			logic_tree.Clear();
			logic_tree1.Clear();
			ports_funcs.Clear();
			result = false;
			rawres = "";
			mathResult = "";
			string valid_chars = "@_abcdefghijklmnopqrstuvwxyz0123456789.([,] )+-*/%^&|!=<>`";
			
			logic_Exp = logic_Exp.Trim();
			logic_Exp = funds.SpaceRemover(logic_Exp, valid_chars);
			
			int operCount = 0, brkCount = 0, square_brackets = 0, func_braces_count = 0, port_brk = 0, port_comma = 0, port_count = 0;
			logic_Exp = logic_Exp.Trim();
			dynamic[] obj = new dynamic[4];
			string parent = "";
			int prev_index = -1;//, opb_Count = logic_Exp.Count(x => (x == '(')), clb_Count = logic_Exp.Count(x => (x == ')'));
			string holder = "";
			bool in_function = false, in_array = false, in_dot = false, in_port = false;

			string[] str = { "Values:=> ", "NextValue:=> ", "Anno:=> ", "Index:=> " };
			
			for (int i = 0; i < logic_Exp.Length;)
			{
				//for  first start
				if (parent == "")
				{
					if(((byte)logic_Exp[i]) == ((byte)15))
						i++;
					
					parent =p.GetType(logic_Exp[i]);
					prev_index = i;
					operCount = 0;
				}
				
			_AT:
				if (parent == "@")
				{
					//0     1      2      3
					//val,nxtVal,anno, index
					obj = GetNxtExp(i, logic_Exp, parent, SymTable);
					
					parent = obj[2].ToString();
					prev_index = i;
					i = Convert.ToInt32(obj[3]);
					operCount = 0;
					
					if(parent == "(")
					{
						obj[0] = "@" + obj[0];
						parent = "func";
						goto _FUNC;
					}
					
					//solve user defined keywords
					if(")+-&|!=<>/*^%],".Contains(parent))
					{
						logic_tree.Add("USR");
						
						broken_data.Add(ObjectLoader.Caller(obj[0].ToString(),new object[] {}, As.KeywordObject, "custom"));
						
						if ((parent == "+" || parent == "-" || parent == "*" || parent == "/" || parent == "%" || parent == "^") && i != -1)
							goto _MATH;
						else if (parent == ")" || (parent == "]" && in_function && in_array))
							goto _BRACKETS;
						else if (parent == ">" || parent == "<" || parent == "<=" || parent == ">=" || parent == "=" || parent == "!=")
							goto _RELATIONALS;
						else if (parent == ",")
							goto _COMMA;
						else if ((parent == "|" || parent == "!" || parent == "&") && i != -1)
							goto _LOGICS;
						else if(i == -1 || i >= logic_Exp.Length)
							break;
						else
							throw new ISqlSyntaxException($"Error: error syntax '@'");
					
					}
					
					holder += "@" + obj[0].ToString();
					
					if (parent == "." && i != -1)
						goto _DOT;
					else
						throw new ISqlSyntaxException($"Error: error syntax for maths operation\nvariables name syntax: @<aliasName>.<name>");

				}

			_DOT:
				if (parent == ".")
				{
					logic_tree.Add("VAR");
					obj = GetNxtExp(i, logic_Exp,parent, SymTable);
					 
					holder += "." + obj[0].ToString();
					broken_data.Add(holder);
					
					if(!in_port)
					{
						if (!variable_names.ContainsKey(holder.ToString()))
						variable_names.Add(holder.ToString(), new object[] { string.Empty, false, "," + (broken_data.Count - 1).ToString() });
						else
						{
							object[] _prev_val = variable_names[holder];
							_prev_val[2] += "," + (broken_data.Count - 1).ToString();
							variable_names[holder] = _prev_val;
						}
					}
					
					if(in_port)
					{
						if(port_comma == 0)
						{
							dynamic[] port_val = ports_funcs[port_count - 1];
							port_val[0] = new dynamic[] { obj[0].ToString()};
							ports_funcs[port_count - 1] = port_val;
						}
						else if (port_comma > 0)
							throw new ISqlArguementException($"Error: port function cannot have a variable as a second parameter");
						else throw new ISqlSyntaxException($"Error: invalid port function parameter");
					}
					
					operCount = 0;
					holder = string.Empty;
					parent = obj[2].ToString();
					prev_index = i;
					i = Convert.ToInt32(obj[3]);

					if ((parent == "+" || parent == "-" || parent == "*" || parent == "/" || parent == "%" || parent == "^") && i != -1)
						goto _MATH;
					else if (parent == ")" || (parent == "]" && in_function && in_array))
						goto _BRACKETS;
					else if (parent == ">" || parent == "<" || parent == "<=" || parent == ">=" || parent == "=" || parent == "!=")
						goto _RELATIONALS;
					else if ((parent == "|" || parent == "!" || parent == "&") && i != -1)
						goto _LOGICS;
					else if(parent == "," && in_function)
						goto _COMMA;
					else if (i == -1)
						break;
					else
						throw new ISqlSyntaxException($"Error: error syntax");
				}
				
			_COMMA: //Done Cool
				if(parent == ",")
				{
					if(!in_function)
						throw new ISqlSyntaxException($"Error: invalid token ','");
						
					if(i == -1 || i >= logic_Exp.Length - 1)
						throw new ISqlSyntaxException($"Error: expecting a list");
					
					obj = GetNxtExp(i, logic_Exp, parent, SymTable);
					 
					logic_tree.Add("COM");
					broken_data.Add(obj[0]);
					parent = obj[2].ToString();
					i = int.Parse(obj[3].ToString());
					
					if(in_port) 
						++port_comma;
					
					if(port_comma > 1)
						throw new ISqlArguementException($"Error: port function can only have a minimum of 1 parameter or a maximum of 2 parameter");
						
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
						case "!":
							goto _LOGICS;
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
					
					string err = (obj[0].ToString().ToUpper() == "PORT" ||obj[0].ToString().ToUpper() == "SELF" ) ? "port function can not be nested" : "port function can not have a nested function";
					if(in_port)
						throw new ISqlSyntaxException($"Error: {err}");
					
					//PARAM - @t1.name,23	TO - @t1.name:23
					//		   [KEY]  -		[VALUES]
					//PORTS - <index>	<parameterString>, <startIndex>, <endIndex>
					if((obj[0].ToString().ToUpper() == "PORT" || obj[0].ToString().ToUpper() == "SELF") && !in_port && allow_port_func)
					{
						port_comma = 0;
						ports_funcs.Add(port_count, new dynamic[] { new dynamic[] {}, i, -1 });
						logic_tree.Add("PORT"); in_port = true;
						broken_data.Add(obj[0].ToString());
						port_count++;
					}
					else
					{
						logic_tree.Add("FUNC");
						broken_data.Add(obj[0].ToString());
					}
					
					in_function = true;
					//func_braces_count++;
					parent = p.GetType(logic_Exp[i]);
					
					if(parent == "(")
						goto _BRACKETS;
						
					throw new ISqlSyntaxException($"Error: error syntax Parent '{parent}'");
				}

			_RELATIONALS:
				if (parent == ">" || parent == "<" || parent == "<=" || parent == ">=" || parent == "=" || parent == "!=")
				{
					operCount = 0;
					obj = GetNxtExp(i, logic_Exp, parent, this.SymTable);
					 
					 if(in_port)
						throw new ISqlSyntaxException($"Error: invalid port function parameter '{obj[0].ToString()}'");
					 
					broken_data.Add(obj[0]);
					parent = obj[2].ToString();
					prev_index = i;
					i = Convert.ToInt32(obj[3]);

					if (obj[0].ToString() == "=" || obj[0].ToString() == "==" || obj[0].ToString() == "!=")
						logic_tree.Add("EQL");
					else
						logic_tree.Add("REL");

					if ((parent == "|" || parent == "!" || parent == "&") && i != -1)
						goto _LOGICS;
					else if (parent == "number")
						goto _NUMBER;
					else if (parent == "name")
						goto _NAME;
					else if (parent == "`")
						goto _VALUE;
					else if ((parent == "+" || parent == "-") && i != -1)
						goto _MATH;
					else if (parent == "(")
						goto _BRACKETS;
					else if (parent == "value")
						goto _VALUE;
					else if (parent == "@")
						goto _AT;
					else
						throw new ISqlSyntaxException($"Error: error syntax");
				}

			_VALUE:
				if (parent == "value")
				{
					operCount = 0;

					obj = GetNxtExp(i, logic_Exp, parent, this.SymTable);
					 
					Value ival = new Value(obj[0]);
					broken_data.Add(ival.Data);
					logic_tree.Add(ival.Anno);
					
					if(in_port)
					{
						if(port_comma == 0 && (Parser.IsStringType(ival.Data) || Parser.CheckNum(ival.Data.ToString())))
						{
							dynamic[] port_val = ports_funcs[port_count - 1];
							port_val[0] = new dynamic[] { ival.Data };
							ports_funcs[port_count - 1] = port_val;
						}
						else if (port_comma > 0 && Parser.CheckNum(ival.Data.ToString()))
						{
							dynamic[] port_val = ports_funcs[port_count - 1];
							dynamic[] p_val = port_val[0];
							
							if(p_val.Length > 1)
								throw new ISqlArguementException($"Error: port function can only have a minimum of 1 parameter or a maximum of 2 parameter");
							
							p_val = new dynamic[] { p_val[0], obj[0].ToString() };
							
							port_val[0] = p_val;
							ports_funcs[port_count - 1] = port_val;
						}
						else throw new ISqlSyntaxException($"Error: invalid port function parameter");
					}
					
					parent = obj[2].ToString();
					prev_index = i;
					i = Convert.ToInt32(obj[3]);

					if ((parent == "|" || parent == "!" || parent == "&") && i != -1)
						goto _LOGICS;
					if ((parent == "+" || parent == "-" || parent == "*" || parent == "/" || parent == "%" || parent == "^") && i != -1)
						goto _MATH;
					if (parent == ">" || parent == "<" || parent == "<=" || parent == ">=" || parent == "=" || parent == "!=")
						goto _RELATIONALS;
					else if (parent == ")" || (parent == "]" && in_function && in_array))
						goto _BRACKETS;
					else if (parent == "," && in_function)
						goto _COMMA;
					else if (parent == "&" || parent == "|" || parent == "!")
						goto _LOGICS;
					else if (i == -1)
						break;
					else
						throw new ISqlSyntaxException($"Error: error syntax");
				}

			_LOGICS:
				if (parent == "&" || parent == "|" || parent == "!")
				{
					operCount = 0;
					obj = GetNxtExp(i, logic_Exp, parent, this.SymTable);
					
					if(in_port)
						throw new ISqlSyntaxException($"Error: invalid port function parameter '{obj[0].ToString()}'");
						
					if (obj[0].ToString() == "!=")
						logic_tree.Add("EQL");
					else if (obj[0].ToString() == "!")
						logic_tree.Add("NOT");
					else
						logic_tree.Add("LOG");

					broken_data.Add(obj[0]);
					parent = obj[2].ToString();
					prev_index = i;
					i = Convert.ToInt32(obj[3]);

					if (parent == "number")
						goto _NUMBER;
					else if (parent == "!")
						goto _LOGICS;
					else if (parent == "name")
						goto _NAME;
					else if (parent == "'")
						goto _VALUE;
					else if ((parent == "+" || parent == "-") && i != -1)
						goto _MATH;
					else if (parent == "(")
						goto _BRACKETS;
					else if (parent == "@")
						goto _AT;
					else if (parent == "value")
						goto _VALUE;
					else
						throw new ISqlSyntaxException($"Error: error syntax");
				}

			_NAME:
				if (parent == "name")
				{
					obj = GetNxtExp(i, logic_Exp, parent, SymTable);
					 
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
						if(in_port)
							throw new ISqlArguementException($"Error: can not use a keyword as a port parameter");
							
						var m = ObjectLoader.Caller(obj[0].ToString().ToUpper(), null, As.KeywordObject);
						Value value = new Value(m);
						logic_tree.Add(value.Anno);
						broken_data.Add(m);
					}
					else
					{
						logic_tree.Add("VAR");
						broken_data.Add(obj[0].ToString());
						
						if(!in_port)
						{
							if (!variable_names.ContainsKey(obj[0].ToString()))
							variable_names.Add(obj[0].ToString(), new object[] { string.Empty, false, broken_data.Count - 1 });
							else
							{
								object[] _prev_val = variable_names[obj[0].ToString()];
								_prev_val[2] += "," + (broken_data.Count - 1).ToString();
								variable_names[obj[0].ToString()] = _prev_val;
							}
						}
						
						if(in_port)
						{
							if(port_comma == 0)
							{
								dynamic[] port_val = ports_funcs[port_count - 1];
								port_val[0] = new dynamic[] { obj[0].ToString()};
								ports_funcs[port_count - 1] = port_val;
							}
							else if (port_comma > 0)
								throw new ISqlArguementException($"Error: port function cannot have a variable as a second parameter");
							else throw new ISqlSyntaxException($"Error: invalid port function parameter");
						}
					}

					if ((parent == "+" || parent == "-" || parent == "*" || parent == "/" || parent == "%" || parent == "^") && i != -1)
						goto _MATH;
					else if (parent == ")" || (parent == "]" && in_function && in_array))
						goto _BRACKETS;
					else if (parent == "&" || parent == "|" || parent == "!")
						goto _LOGICS;
					else if (parent == ">" || parent == "<" || parent == "<=" || parent == ">=" || parent == "=" || parent == "!=")
						goto _RELATIONALS;
					else if (parent == "," && in_function)
						goto _COMMA;
					else if (i == -1)
						break;
					else
						throw new ISqlSyntaxException($"Error: error syntax 'name'");
				}

			_NUMBER:
				if (parent == "number")
				{
					logic_tree.Add("NUM");
					obj = GetNxtExp(i, logic_Exp, parent, this.SymTable);
					 
					broken_data.Add((obj[0].ToString().Contains(".")) ? Convert.ToDecimal(obj[0]) : Convert.ToInt32(obj[0]));
					parent = obj[2].ToString();
					prev_index = i;
					i = Convert.ToInt32(obj[3]);
					operCount = 0;
					
					if(in_port)
					{
						if(port_comma == 0)
						{
							dynamic[] port_val = ports_funcs[port_count - 1];
							port_val[0] = new dynamic[] { obj[0].ToString() };
							ports_funcs[port_count - 1] = port_val;
						}
						else if (port_comma > 0)
						{
							dynamic[] port_val = ports_funcs[port_count - 1];
							dynamic[] p_val = port_val[0];
							
							if(p_val.Length > 1)
								throw new ISqlArguementException($"Error: port function can only have a minimum of 1 parameter or a maximum of 2 parameter");
							
							p_val = new dynamic[] { p_val[0], obj[0].ToString() };
							
							port_val[0] = p_val;
							ports_funcs[port_count - 1] = port_val;
						}
						else throw new ISqlSyntaxException($"Error: invalid port function parameter");
					}
					
					if ((parent == "+" || parent == "-" || parent == "*" || parent == "/" || parent == "%" || parent == "^") && i != -1)
						goto _MATH;
					else if (parent == ")" || (parent == "]" && in_function && in_array))
						goto _BRACKETS;
					else if (parent == "&" || parent == "|" || parent == "!")
						goto _LOGICS;
					else if (parent == ">" || parent == "<" || parent == "<=" || parent == ">=" || parent == "=" || parent == "!=")
						goto _RELATIONALS;
					else if (parent == "," && in_function)
						goto _COMMA;
					else if (i == -1)
						break;
					else
						throw new ISqlSyntaxException($"Error: error syntax");
				}

			_MATH:
				if (parent == "+" || parent == "-" || parent == "*" || parent == "/" || parent == "%" || parent == "^")
				{
					logic_tree.Add("ART");
					obj = GetNxtExp(i, logic_Exp, parent, this.SymTable);
					
					if(in_port)
						throw new ISqlSyntaxException($"Error: invalid port function parameter '{obj[0].ToString()}'");
					
					if (operCount > 1)
						throw new ISqlException($"Error: can not join multiple arithmetic symbols together highest is two '{string.Join(" ", broken_data)}'");
						
					
					if(parent == "+" || parent == "-")
						++operCount;

					broken_data.Add(obj[0].ToString());
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
					else if (parent == "value")
						goto _VALUE;
					else if (i == -1)
						throw new ISqlSyntaxException($"Error: uprand needed");
					else
						throw new ISqlSyntaxException($"Error: error syntax");
				}

			_BRACKETS:
				if (parent == "(" || parent == ")" || parent == "["|| parent == "]")
				{
					if (parent == "(")
						logic_tree.Add("OB");
					else if(parent == ")")
						logic_tree.Add("CB");
					else if(parent == "[")
						logic_tree.Add("OSB");
					else 
						logic_tree.Add("CSB");

					operCount = 0;
					string prev_parent = parent;
					
					if (parent == "(")
						++brkCount;
					else if(parent == ")")
						--brkCount;
					
					if(in_port)
					{
						if(parent == "(")
							++port_brk;
						else if(parent == ")")
							--port_brk;
						if(port_brk > 1 || port_brk < 0)
							throw new ISqlSyntaxException($"Error: a port function parameter can not have an expression");
					}
					
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
						
						if(port_brk == 0 && in_port)
						{
							if(port_comma > 1)
								throw new ISqlArguementException($"Error: port function can only have a minimum of 1 parameter or a maximum of 2 parameter");
							
							dynamic[] port_val = ports_funcs[port_count - 1];
							port_val[2] = i;
							ports_funcs[port_count - 1] = port_val;
							
							port_comma = 0;
							in_port = false;
						}
							
						if(func_braces_count < 0 || square_brackets < 0)
							throw new ISqlSyntaxException($"Error: brackets are placed in incorret order\nOSB: '{square_brackets}'\nFUNCB: '{func_braces_count}'");
						
					}
					
					if (brkCount < 0 || square_brackets < 0)
						throw new ISqlException($"Error: brackets are placed in incorret order\nOB: '{brkCount}'\nOSB: '{square_brackets}'");
					obj = GetNxtExp(i, logic_Exp, parent, this.SymTable);
					 
					parent = obj[2].ToString();
					prev_index = i;
					i = Convert.ToInt32(obj[3]);
					
					if(parent == ")" && prev_parent == "(" && !in_function)
						throw new ISqlSyntaxException($"Error: can not have empty brackets");
					
					else if(parent == ")" && prev_parent == "(" && p.GetType(logic_Exp[i - 2]) != "name" && p.GetType(logic_Exp[i - 2]) != "number")
						throw new ISqlSyntaxException($"Error: can not have empty brackets");
					else if (parent == ")" && prev_parent == "(" && in_port)
						throw new ISqlSyntaxException($"Error: port function can not have empty arguements");
					broken_data.Add(obj[0]);
					
					if((prev_parent == "[" || prev_parent == ",") && !in_function)
						throw new ISqlException($"Error: can not have an array outside function parameter {in_function}");
						
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
					else if (parent == "|" || parent == "&" || parent == "!")
						goto _LOGICS;
					else if (parent == ">" || parent == "<" || parent == "<=" || parent == ">=" || parent == "=" || parent == "!=")
						goto _RELATIONALS;
					else if (parent == "value" && (prev_parent == "(" || prev_parent == "["))
						goto _VALUE;
					else if (parent == "," && in_function && (prev_parent == ")" || prev_parent == "]"))
						goto _COMMA;
					else if (i == -1)
						break;// throw new ISqlSyntaxException($"Error: uprand needed");
					else
						throw new ISqlSyntaxException($"Error: error syntax '{parent} {i} {logic_Exp}'");
				}
				
				throw new ISqlException($"Error: unknown error occured");
			}
			
			if (brkCount != 0)
				throw new ISqlException($"Error: brakets are not opened or closed properly");
			
		}
		
		public void Solve()
		{
			bool can_Go_on = variable_names.Values.All(x => x[0].ToString().Trim() != string.Empty);

            var vars = string.Join(", ",
                        variable_names.Where(x => x.Value[0].ToString() == string.Empty).Select(x => x.Key).ToArray());

						//.GroupBy(x => x.Value[0].ToString() == string.Empty || x.Value[0].GetType() == typeof(None))
						//.Select(x => x.Key).ToArray());
						
			if (!can_Go_on)
				throw new ISqlException($"Error: not all variable have a value\nVariabel(s): '{vars}'");
			
			if(logic_tree.Count != broken_data.Count || logic_tree1.Count != broken_data.Count || logic_tree1.Count == 0)
				RearrangeLogicTree(broken_data, ref logic_tree);
			
			result = false;		//for bool result
			mathResult = "";	//for math result
			rawres = "";		//for anykind of result ie. bool, string, number and or objects
			
			List<dynamic> range = new List<dynamic>();
			if(broken_data.Count < 1)
				return;
			logic_tree1.Clear();
			range.AddRange(broken_data.GetRange(0, broken_data.Count));
			logic_tree1.AddRange(logic_tree.GetRange(0, logic_tree.Count));
            //Console.WriteLine(string.Join("\t", logic_tree1));
			//Solving for Ports before function
			if(logic_tree1.Contains("PORT"))
			{
				var port_event = new PortEvent();
				port_event.Data = range;		//for the copied data fetched
				port_event.Tree = logic_tree1;	//for the copied tree fetched
				port_event.Ports = ports_funcs;	//for the ports funcs, actualy no use for this
				port_event.DTable = dt;			//for the dataTable to use, this brings subquery alive
				OnPortEvent(port_event);		//this rises the event to inform the subscribers
			}
            
			//Solve For Function Before anything
			FUNTION_START:
			if(logic_tree1.Contains("FUNC"))
			{
				int index = logic_tree1.LastIndexOf("FUNC");
				List<dynamic> args = new List<dynamic>();
				List<string> args_anno = new List<string>();
				int end = range.GetEnclose("(", ")", index + 2, 1);
				int arg_index = index + 2;
				
				args_anno.AddRange(logic_tree1.GetRange(arg_index, end - arg_index));
				
				args.AddRange(range.GetRange(arg_index, end - arg_index));
				
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
				string function = range[index];
				
				if(function.Trim().StartsWith("@"))
				{
					function = function.Remove(0, 1);
					
					var val = ObjectLoader.Caller(function, fargs, As.MethodObject, "custom");
					range.MyRemoveRange(index, end - index + 1);
					range.Insert(index, val);
					logic_tree1.MyRemoveRange(index, end - index);
					Value value_con = new Value(val);
					logic_tree1.Insert(index, value_con.Anno);
				}
				
				else if(!function.Trim().StartsWith("@"))
				{
					function = function.ToUpper();
					
					var val = ObjectLoader.Caller(function, fargs, As.MethodObject);
					
					range.MyRemoveRange(index, end - index + 1);
					range.Insert(index, val);
					logic_tree1.MyRemoveRange(index, end - index);
					Value value_con = new Value(val);
					logic_tree1.Insert(index, value_con.Anno);
				}
				
				if(logic_tree1.Contains("FUNC"))
					goto FUNTION_START;
			}
			
			if(range.Count == 0)
			{
				result = false;
				mathResult = "";
				rawres = "";
				return;
			}
			
			//for mathematics evaluation
			if (p.HasMath(range) || (range.Count == 1 && range[0].GetType() != typeof(bool)))
			{
				MathRessolver(ref range);
				
				if (range[0].GetType() != typeof(bool) && range.Count == 1)
				{
					MathResult = range[0];
					result = false;
					rawres = range[0];
					return;
				}
			}
			
			//for logic and relational expression
			if (range.Contains("("))
				range = Brks(range);

			if (range.Contains("<"))
				range = LessThan(range);

			if (range.Contains("<="))
				range = LessOrEqual(range);

			if (range.Contains(">"))
				range = GreaterThan(range);

			if (range.Contains(">="))
				range = GreaterOrEqual(range);

			if (range.Contains("<>"))
				range = LessOrGreaterThan(range);

			if (range.Contains("=="))
				range = EqualTO(range);

			if (range.Contains("!="))
				range = NotEqual(range);

			if (range.Contains("!"))
				range = Not(range);

			if (range.Contains("&&"))
				range = And(range);

			if (range.Contains("||"))
				range = Or(range);

			if (range[0].GetType() == typeof(bool) && range.Count == 1)
			{
				result = bool.Parse(range[0].ToString());
				MathResult = "";
			}

			rawres = range[0];
				
			if (range.Count > 1)
				throw new ISqlException($"Eigne Error: could not solve properly\n'{string.Join(" ", range.ToArray())}'");
			
		}
		
		private void MathRessolver(ref List<dynamic> data)
		{
				bool end_reached = false;
				
				int start = 0, end = -1, nxt_srt = 0;
				
			Continue:

				List<dynamic> data_grp = new List<dynamic>();
				
				if (logic_tree1.Count != data.Count)
				{
					RearrangeLogicTree(data, ref logic_tree1);
					if (logic_tree1.Count != data.Count)
						throw new ISqlException("Error: Rearrangement error");
				}
				
				int i = start; bool brk = false;
				while (i < data.Count)
				{
					if(logic_tree1[i].ToLower() == "ob" || logic_tree1[i].ToLower() == "cb")
						brk = true;
						
					if (logic_tree1[i] == "EQL" || logic_tree1[i] == "REL" || logic_tree1[i] == "LOG" || logic_tree1[i] == "NOT")
					{
						end = i;
						nxt_srt = i + 1;
						break;
					}

					else if (i == data.Count - 1)
					{
						data_grp.Add(data[i]);
						end_reached = true;
						end = data.Count - 1;
						nxt_srt = end;
						
						break;
					}
					data_grp.Add(data[i]);
					i++;
				}
				
				if (brk)
				{
				Recheck: //Adjusting open brackets
					if (data_grp.Count(x => x.ToString() == "(") > data_grp.Count(x => x.ToString() == ")"))
					{
						int index = data_grp.IndexOf("(");

						data_grp.MyRemoveRange(0, index + 1);

						start += index + 1;
						if (data_grp.Count(x => x.ToString() == "(") > data_grp.Count(x => x.ToString() == ")"))
							goto Recheck;
					}
					
					//come fix this potential error can occur
					if (data_grp.Count(x => x.ToString() == ")") > data_grp.Count(x => x.ToString() == "("))
					{
						int index = 0, point = 0;
						foreach (var id in data_grp)
						{
							if (id.ToString() == ")")
								index = point;
							point++;
						}
						nxt_srt -= index + 1;

						data_grp.MyRemoveRange(index, start - end + index + 1);
						end = nxt_srt - 1;

						if (data_grp.Count(x => x.ToString() == ")") > data_grp.Count(x => x.ToString() == "("))
							goto Recheck;
					}
				}
				
				nxt_srt = end + 1;

				List<dynamic> d_grp = new List<dynamic>();
				d_grp.AddRange(data_grp.ToArray());
				//d_grp.RemoveAll(x => x.)
				
				if((data_grp.Count <= 1 && end_reached) || (data_grp.Count == data.Count && !p.HasMath(data_grp) && end_reached) || (!p.HasMath(data_grp) && !end_reached))
					goto DONE;
					
				if(!p.HasMath(data_grp) && !end_reached)
				{
					start = nxt_srt; goto Continue;
				}
				
				
				if (p.HasMath(data_grp) && data_grp.Count > 1)
				{
					int length = data_grp.Count;
					ExpressionEngine exp = new ExpressionEngine();

					data_grp = exp.Brks(data_grp);
					data_grp = exp.PreAdd(data_grp);
					data_grp = exp.PreSub(data_grp);
					data_grp = exp.Pow(data_grp);
					data_grp = exp.Multi(data_grp);
					data_grp = exp.Div(data_grp);
					data_grp = exp.Mod(data_grp);
					data_grp = exp.Add(data_grp);
					data_grp = exp.Sub(data_grp);
					
					if(data_grp.Count != 1)
						throw new ISqlArithemeticException($"Error: could not solve properly");
					
					dynamic value = data_grp[0];
					data.MyRemoveRange(start, length);
					if (data.Count != 0)
					{
						if (end_reached && data[data.Count - 1].ToString() != ")")
						{
							data.Add(value);
						}
						else if (end_reached && data[data.Count - 1].ToString() == ")")
						{
							data.Insert(start, value);
						}
						else
							data.Insert(start, value);
					}
					
					else if (data.Count == 0)
					{
						if (!end_reached)
						{
							data.Insert(start, value);
						}
						else if (end_reached)
						{
							data.Add(value);
						}
					}

					start = (nxt_srt - length) + 1;

					#region Rearranging logic tree
					/*
					logic_tree1.MyRemoveRange(start, length);
					
					if(Parser.CheckNum(value.ToString()))
						logic_tree1.Insert(start, "NUM");
					else if (value.GetType() == typeof(string) || value.GetType() == typeof(Varchar) || value.GetType() == typeof(Text) || value.GetType() == typeof(Character))
						logic_tree1.Insert(start, "STR");
					else if(value.GetType() == typeof(DateTime) || value.GetType() == typeof(Date) || value.GetType() == typeof(Time))
						logic_tree1.Insert(start, "DAT");
					else if(value.GetType() == typeof(bool))
						logic_tree1.Insert(start, "BOL");*/
					RearrangeLogicTree(data, ref logic_tree1);
					#endregion

					if (!end_reached)
					{
						start = (nxt_srt - length) + 1;
						goto Continue;
					}
					else goto DONE;
				}
				
				else
				{
					start = nxt_srt;
					if (!end_reached)
						goto Continue;
				}
				DONE:
				if (logic_tree1.Contains("ART"))
					throw new ISqlArithemeticException($"Error: could not solve properly '{string.Join(" ", data)}'");
			
		}
		
		//logic_tree fixing
		internal void RearrangeLogicTree(List<dynamic> data, ref List<string> tree)
		{
			var ntree = new string[tree.Count];
			tree.CopyTo(ntree);
			tree.Clear();
			int c = -1;
			foreach (var item in data)
			{
				c++;
				if(ntree.Length > 0)
				{
					if(ntree[c].ToUpper() == "NEVL" || ntree[c].ToUpper() == "EVL" || ntree[c].ToUpper() == "STR")
					{
						tree.Add(ntree[c]);
						continue;
					}
				}
				
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
						if((data[c].ToString().ToLower() == "port" || data[c].ToString().ToLower() == "self") 
							&& allow_port_func && data[c + 1].ToString() == "(")
							{
								tree.Add("PORT");
							}
							
							else if(data[c + 1].ToString() == "(")
							{
								tree.Add("FUNC");
							}
							
							else tree.Add("STR");
					}
					else tree.Add("STR");
					
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
		
		// returns a selectblock object
		internal SelectBlock GetSelectBlock()
		{
			SelectBlock selectBlock = new SelectBlock();
			selectBlock.Query = broken_data;
			selectBlock.QueryVariables = variable_names;
			selectBlock.Tree = logic_tree;
			
			return selectBlock;
		}

        internal static LogicExpressionEngine GenerateLogicExpressionEngine(SelectBlock block)
        {
            LogicExpressionEngine expEngine = new LogicExpressionEngine();
            expEngine.broken_data = block.Query;
            expEngine.variable_names = block.QueryVariables;
            expEngine.logic_tree.AddRange(block.Tree);
            expEngine.logic_tree1.AddRange(block.Tree);
            return expEngine;
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
				if(logic_tree.Count != broken_data.Count)
				{
					RearrangeLogicTree(broken_data, ref logic_tree1);
				}
					
				logic_tree[i] = value1.Anno;
			}
		}

		/// <summary>
		/// use to set or override multiple variables in the arguement tree,5 it throws an ISqlException if the variable the value or do not exists
		/// </summary>
		/// <param name="name_values">the key of the dictionary represent the variable names, while the values of the dictionary represent the values given to that variable</param>
		public void ChangeValues(Dictionary<string, object> name_values)
		{
			foreach (var key in name_values.Keys)
				ChangeValue(key, name_values[key]);
		}
		
		public void SetValue(string var_name, object value)
		{
			if (variable_names.ContainsKey(var_name))
			{
				object[] prev_val = variable_names[var_name];
				if (Convert.ToBoolean(prev_val[1]) == false)
				{
					ChangeValue(var_name, value);
				}
				else
					throw new ISqlException($"Error: the variable '{var_name}' already have a value");
			}
			else
				throw new ISqlException($"Error: no variable have the name '{var_name}'");

			/*string exp = string.Join(" ",broken_data);
            broken_data.Clear();
            Break_Expression(exp);*/
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

        public string[] GetVariables()
		{
			List<string> names = new List<string>();
			foreach (string var_names in variable_names.Keys)
			{
				object[] value = variable_names[var_names];
				names.Add(var_names);
			}
			return names.ToArray<string>();
		}

		public void SetVariables(string[] names, object[] values)
		{
			if (names.Length == values.Length && names.Length <= variable_names.Count)
			{
				int index = 0;
				foreach (string name in names)
				{
					if (!variable_names.Keys.Contains(name))
						throw new ISqlException($"Error: variable '{name}' does not exist.");
					
					ChangeValue(name, values[index]);
					index++;
				}

			}
			else throw new ISqlException($"Error: length issues\nnames Length: '{names.Length}'\nvalues Length: '{values.Length}'\nvariable_name Length: '{variable_names.Count}");
		}

		public bool HasVariables()
		{
			return variable_names.Count > 0;
		}

		public bool HasVariable(string name)
		{
			return variable_names.Keys.Contains(name);
		}

		public object GetValue(string variable)
		{
			if (variable_names.ContainsKey(variable))
				return variable_names[variable][0];

			throw new ISqlException($"Error: the variable '{variable}' is not define");
		}

		public object[] GetValues(params string[] variables)
		{
			List<object> objs = new List<object>();
			foreach (var v in variables)
			{
				objs.Add(GetValue(v));
			}
			return objs.ToArray<object>();
		}

		public object[] GetExpression()
		{
			List<object> gExp = new List<object>();
			int count = -1;
			foreach(var d in broken_data)
			{
				count++;
				if(logic_tree[count] == "EVL" || logic_tree[count] == "DAT" || logic_tree[count] == "BOL")
				{
					gExp.Add("`" + d + "`"); continue;
				}
				else if (logic_tree[count] == "STR" || logic_tree[count] == "NEVL")
				{
					gExp.Add("'" + d + "'"); continue;
				}
				
				gExp.Add(d);
			}
			
			return gExp.ToArray<object>();
		}
		
		public void SetExpression(object[] expression)
		{
			broken_data = expression.ToList<dynamic>();
			RearrangeLogicTree(broken_data, ref logic_tree);
		}
		
		public string GetStringExpression()
		{
			return string.Join(" ", GetExpression());
		}
		
		public string GetMainExpression()
		{
			return arguement;
		}
		
		internal List<dynamic> Brks(List<dynamic> data)
		{
		Brks:
			if (data.Contains("("))
			{
				int index = data.LastIndexOf("("), end = 0;
				if (data.Count(x => x.ToString() == "(") > data.Count(x => x.ToString() == ")"))
					data.Add(")");
				for (int i = index; i < data.Count; i++)
				{
					if (data[i].ToString() == ")")
					{
						end = i;
						break;
					}
				}
				
				List<dynamic> range = new List<dynamic>();
				range.AddRange(data.GetRange(index + 1, (end - index) - 1));
				
				if (range.Contains("<"))
					range = LessThan(range);
				if (range.Contains("<="))
					range = LessOrEqual(range);
				if (range.Contains(">"))
					range = GreaterThan(range);
				if (range.Contains(">="))
					range = GreaterOrEqual(range);
				if (range.Contains("=="))
					range = EqualTO(range);
				if (range.Contains("!="))
					range = NotEqual(range);
				if (range.Contains("<>"))
					range = LessOrGreaterThan(range);
				if (range.Contains("!"))
					range = Not(range);
				if (range.Contains("&&"))
					range = And(range);
				if (range.Contains("||"))
					range = Or(range);
				
				if (range.Count != 1)
					throw new ISqlArithemeticException($"Error: in brackets\nValues: '{string.Join(" ", range)}'");
				
				data.RemoveRange(index + 1, (end - index) - 1);
				data.Insert(data.LastIndexOf("(") + 1, range[0]);
				index = data.LastIndexOf("(");
				data.RemoveAt(index + 2);
				data.RemoveAt(index);
				
				goto Brks;
			}
			return data;
		}

		#region Relational Operators
		internal List<dynamic> LessThan(List<dynamic> data)
		{
		Less:
			if (data.Contains("<"))
			{
				int index = data.IndexOf("<");
				dynamic pre_value = data[index - 1];
				dynamic nxt_value = data[index + 1];
				dynamic value = "";
				
				value = pre_value < nxt_value;
				data.Insert(index - 1, value);
				data.RemoveRange(index, 3);

				goto Less;
			}
			return data;
		}

		internal List<dynamic> GreaterThan(List<dynamic> data)
		{
		GreaterThan:
			if (data.Contains(">"))
			{
				int index = data.IndexOf(">");
				dynamic pre_value = data[index - 1];
				dynamic nxt_value = data[index + 1];
				dynamic value = "";

				value = pre_value > nxt_value;
				data.Insert(index - 1, value);
				data.RemoveRange(index, 3);

				goto GreaterThan;
			}
			return data;
		}

		internal List<dynamic> LessOrEqual(List<dynamic> data)
		{
		LessOrEqual:
			if (data.Contains("<="))
			{
				int index = data.IndexOf("<=");
				dynamic pre_value = data[index - 1];
				dynamic nxt_value = data[index + 1];
				dynamic value = "";

				value = pre_value <= nxt_value;
				data.Insert(index - 1, value);
				data.RemoveRange(index, 3);

				goto LessOrEqual;
			}
			return data;
		}

		internal List<dynamic> GreaterOrEqual(List<dynamic> data)
		{
		GreaterOrEqual:
			if (data.Contains(">="))
			{
				int index = data.IndexOf(">=");
				dynamic pre_value = data[index - 1];
				dynamic nxt_value = data[index + 1];
				dynamic value = "";

				value = pre_value >= nxt_value;
				data.Insert(index - 1, value);
				data.RemoveRange(index, 3);

				goto GreaterOrEqual;
			}
			return data;
		}

		internal List<dynamic> EqualTO(List<dynamic> data)
		{
		EqualTo:
			if (data.Contains("=="))
			{
				int index = data.IndexOf("==");
				dynamic pre_value = data[index - 1];
				dynamic nxt_value = data[index + 1];
				dynamic value = "";
				
				value = pre_value.ToString() == nxt_value.ToString();

				data.Insert(index - 1, value);
				data.RemoveRange(index, 3);
				
				goto EqualTo;
			}
			return data;
		}

		internal List<dynamic> NotEqual(List<dynamic> data)
		{
		NotEqual:
			if (data.Contains("!="))
			{
				int index = data.IndexOf("!=");
				dynamic pre_value = data[index - 1];
				dynamic nxt_value = data[index + 1];
				dynamic value = "";
				
				value = pre_value != nxt_value;
				data.Insert(index - 1, value);
				data.RemoveRange(index, 3);

				goto NotEqual;
			}
			return data;
		}

		internal List<dynamic> LessOrGreaterThan(List<dynamic> data)
		{
		LessGreater:
			if (data.Contains("<>"))
			{
				int index = data.IndexOf("<>");
				dynamic pre_value = data[index - 1];
				dynamic nxt_value = data[index + 1];
				dynamic value = "";

				value = (pre_value != nxt_value);
				data.Insert(index - 1, value);
				data.RemoveRange(index, 3);

				goto LessGreater;
			}
			return data;
		}
		#endregion
		
		#region Logic Operators
		internal List<dynamic> And(List<dynamic> data)
		{
		And:
			if (data.Contains("&&"))
			{
				int index = data.IndexOf("&&");
				dynamic pre_value = data[index - 1];
				dynamic nxt_value = data[index + 1];
				dynamic value = "";
				
				if(pre_value.GetType() != typeof(bool) || nxt_value.GetType() != typeof(bool))
					throw new ISqlTypeException($"Error: invalid uprand '{pre_value}' && '{nxt_value}'");
				
				value = pre_value && nxt_value;
				data.Insert(index - 1, value);
				data.RemoveRange(index, 3);

				goto And;
			}
			return data;
		}

		internal List<dynamic> Or(List<dynamic> data)
		{
		Or:
			if (data.Contains("||"))
			{
				int index = data.IndexOf("||");
				dynamic pre_value = data[index - 1];
				dynamic nxt_value = data[index + 1];
				dynamic value = "";
				
				if(pre_value.GetType() != typeof(bool) || nxt_value.GetType() != typeof(bool))
					throw new ISqlTypeException($"Error: invalid uprand '{pre_value}' || '{nxt_value}'");
				
				value = pre_value || nxt_value;
				data.Insert(index - 1, value);
				data.RemoveRange(index, 3);

				goto Or;
			}
			return data;
		}

		internal List<dynamic> Not(List<dynamic> data)
		{
		Not:
			if (data.Contains("!"))
			{
				int index = data.MyLastIndexOf("!");
				if (index == data.Count - 1)
					throw new ISqlException($"Error: no uprand spotted");
				
				dynamic nxt_value = data[index + 1];
				
				if (nxt_value.GetType() != typeof(bool))
					throw new ISqlException($"Error: the uprand for '!' is not a type of 'bool'");
					
				dynamic value = "";

				value = !nxt_value;

				data.Insert(index, value);
				data.RemoveAt(index + 1);
				data.RemoveAt(index + 1);
				
				goto Not;
			}
			return data;
		}
		#endregion
	}
}


//me y