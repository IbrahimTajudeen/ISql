using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Isac;

namespace Isac.Isql.Collections
{
	public sealed class Column : IComparable, IComparable<Column>, ICloneable, IEquatable<Column>
	{
		private object parent = null;
        internal object Parent
        {
        	get { return parent; }
        	set
        	{
        		parent = value;
        	}
        }
		
		private string name = "";
		private string alias = "";
		private Type dataType;
		private bool acceptNull = true;
		private int size = -1;
		private Default defaultValue = new Default();
		private bool isUnique = false;
		private Check check = "`true`";
		private ForeignKey foreignKey;
		private AutoIncreament increament;
		private PrimaryKey primaryKey;
		private bool isprimaryKey = false;
		private Cell columnDefault = new Cell("", "null");

		private bool isautoIncreament = false;
		
		private bool nameDefined = false;
		private bool typeDefined = false;
		private bool isDefined = false;
		private int col_index = 0;
		
		public int CompareTo(Column other)
		{
			return Name.CompareTo(other.Name);
		}
		
		public int CompareTo(object obj)
		{
			if(obj.GetType() == this.GetType())
			{
				Column col1 = obj as Column;
				return this.CompareTo(col1);
			}
			
			return -1;
		}
		
		public int Compare(object x, object y)
		{
			if(x.GetType() == y.GetType() && x.GetType() == this.GetType())
			{
				Column column1 = x as Column;
				Column column2 = y as Column;
				column1.CompareTo(column2);
			}
			return -1;
		}
		
        public bool Equals(Column other)
        {
        	return (this.Name.ToLower() == other.Name.ToLower() && this.DataType == other.DataType);
        }
        
        public override bool Equals(object obj)
        {
        	if(obj.GetType() == typeof(Column))
        	{
        		return this.Equals((Column)obj);
        	}
        	
        	return false;
        }

		internal Column()
		{
			name = "null";
			alias = "";
			this.DataType = typeof(Varchar);
			this.AcceptNull = true;
			this.Size = -1;
			this.DefaultValue = new Default();
			this.IsUnique = false;
			this.Check = "`true`";
			this.ForeignKey = new ForeignKey("null", new Reference( "", new string[] { "" }, OnDelete.None));
			this.IsPrimaryKey = false;
			this.IsAutoIncreament = false;
		}
		
		internal Column(string name)
		{
			this.Name = name;
			this.DataType = typeof(Varchar);
			
			this.AcceptNull = true;
			this.DefaultValue = "null";
			this.IsUnique = false;
			this.Check = "`true`";
			this.ForeignKey = new ForeignKey("null", new Reference( "", new string[] { "" }, OnDelete.None));
			this.IsPrimaryKey = false;
			this.ColumnDefault = new Cell(name, "null");
			this.IsAutoIncreament = false;
			
		}
		
		public Column(string name, Type dataType)
		{
			this.Name = name;
			this.DataType = dataType;
			
			this.AcceptNull = true;
			this.Size = -1;
			//this.DefaultValue = new Default();
			this.IsUnique = false;
			this.Check = "`true`";
			this.ForeignKey = new ForeignKey("null", new Reference( "", new string[] { "" }, OnDelete.None));
			this.IsPrimaryKey = false;
			this.ColumnDefault = new Cell(name, "null");
			this.IsAutoIncreament = false;
			
		}
		
		public Column(string name, Type dataType, int size)
		{
			this.Name = name;
			this.DataType = dataType;
			this.Size = size;

			this.AcceptNull = true;
			this.DefaultValue = "null";
			this.IsUnique = false;
			this.Check = "`true`";
			this.ForeignKey = new ForeignKey("null", new Reference( "", new string[] { "" }, OnDelete.None));
			this.IsPrimaryKey = false;
			this.ColumnDefault = new Cell(name, "null");
			this.IsAutoIncreament = false;
			
		}
		
		public Column(string name, Type dataType,
					  bool acceptNull = true, int size = -1,
					  Default defaultValue = null, bool isUnique = false,
					  Check check = null, ForeignKey foreignKey = null,
					  bool autoIncreament = false, bool isPrimaryKey = false)
		{
			this.Name = name;
			this.DataType = dataType;
			this.AcceptNull = acceptNull;
			this.Size = size;
			this.DefaultValue = (defaultValue == null) ? new Default() : defaultValue;
			this.IsUnique = isUnique;
			this.Check = (check == null) ? new Check("`true`") : check;
			this.ForeignKey = new ForeignKey("null", new Reference( "", new string[] { "" }, OnDelete.None));
			this.IsAutoIncreament = autoIncreament;
			this.IsPrimaryKey = isPrimaryKey;
			
		}
		
		/*public Column(string name, Type dataType, int size = -1, Null isNull = true, 
						Default defaultValue = null, Unique isUnique = false,
						Check check = null, AutoIncreament increament = false, 
						PrimaryKey PK = false, Choice choice = null, Reference reference = null)
		{
			
		}*/
		
		private Column(string name, string alias, Type dataType, bool acceptNull, 
						int size, Default defaultValue, bool isUnique, Check check, 
						ForeignKey foreignKey, bool increament, bool isPrimakey, 
						Cell columnDefault, int colIndex)
		{
			this.Name = name;
			this.Alias = alias;
			this.DataType = dataType;
			this.acceptNull = acceptNull;
			this.size = size;
			
			this.defaultValue = defaultValue;	//Create Clone() and Copy()
			this.check = check;					//Create Clone() and Copy()
			this.foreignKey = foreignKey;		//Create Clone() and Copy()
			
			this.columnDefault = columnDefault.Copy();
			this.isUnique = isUnique;
			this.isautoIncreament = increament;
			this.isprimaryKey = isPrimakey;
			this.col_index = colIndex;
		}
		

		#region Encapsulations
		public string Name
		{
			get
			{
				
				return name;
			}

			set
			{
				value = value.Trim();
				if (value.ToLower() != "null" && value != "")
				{
					if(value.Contains("`") && value.StartsWith("`") && value.EndsWith("`") && value.Count(x => x == '`') == 2)
					{
						this.name = value.Remove(value.Length - 1, 1);
						this.name = this.name.Remove(0, 1);
					}
					
					else this.name = value;
				}
				else throw new ISqlException($"Error: column name cannot be null");

				this.ColumnDefault.Name = this.name;
				
				NameDefined = true;
				if (NameDefined && TypeDefined)
					IsDefined = true;
			}
		}

		public string Alias
		{
			get
			{
				return Alias;
			}
			set
			{		
				if (Name.Trim() != "" && NameDefined)
				{
					value = value.Trim();
					if (value.ToLower() != "null" && value != "")
					{
						if(value.Contains("`") && value.StartsWith("`") && value.EndsWith("`") && value.Count(x => x == '`') == 2)
						{
							this.alias = value.Remove(value.Length - 1, 1);
							this.alias = this.alias.Remove(0, 1);
						}
						
						else this.alias = value;
					}
					else throw new ISqlException($"Error: column name cannot be null");
				}
				else throw new ISqlException($"Error: can not set an alias name for a column without name");
			}
		}

		public string RealName
		{
			get 
			{
				return (Alias.Trim() == "") ? Name : Alias; 
			}
		}

		public Cell ColumnDefault
		{
			get
			{
				return this.columnDefault;
			}
			internal set
			{
				this.columnDefault = value;
			}
		}

		public Type DataType
		{
			get
			{
				return dataType;
			}

			set
			{
				TypeDefined = true;
				if (NameDefined && TypeDefined)
					IsDefined = true;
				
				if(!Parser.DataTypes.Contains(value) && value.IsArray == false)
					throw new ISqlTypeException($"Error: this is not a recognized type\nType: '{value}'");
				
				dataType = value;

			}
		}

		public bool AcceptNull
		{
			get
			{
				return acceptNull;
			}

			set
			{
				acceptNull = value;
			}
		}

		public int Size
		{
			get
			{		
				return size;
			}

			set
			{
				if (!IsDefined)
					throw new ISqlException($"Error: column name and type must be define before size");

				if (value < 0 && value == -1)
					size = -1;

				else if (value > 0)
					size = value;

				else throw new ISqlArguementException($"Error: the size of a column can not be zero or less than -1 '{value}'");

			}
		}

		public Default DefaultValue
		{
			get
			{
				return defaultValue;
			}

			set
			{
	        		
				if (TypeDefined && NameDefined)
				{
					if (DataType == Parser.DataConverter(dataType, value.GetDefault().ToString()).GetType() && value.ToString().ToLower() != "null")
						defaultValue = Default.Parse(value.ToString());

					else throw new ISqlTypeException($"Error: type of a  defaultvalue must be the same with the column type\nColumn Type: '{DataType}");
				}
				else throw new ISqlTypeException($"Error: column datatype should be defined first before setting a default value");

				this.ColumnDefault.Value = (defaultValue.GetDefault().ToString().ToLower().Trim() == "null") ? "null" : Parser.DataConverter(this.DataType, defaultValue.GetDefault().ToString());
			}
		}

//Continue from here with the parent inspecting
		public bool IsUnique
		{
			get { return this.isUnique; }
			set
			{
				this.isUnique = value;
			}
		}

		public Check Check
		{
			get { return this.check; }
			set
			{
				if (value == null || value.ToString().Trim().ToLower() == "null")
					this.check = "`true`";

				else
					this.check = value;
			}
		}

		public ForeignKey ForeignKey
		{
			get { return this.foreignKey; }
			set
			{
				this.foreignKey = value;
			}
		}

		public bool IsAutoIncreament
		{
			get { return isautoIncreament; }
			set
			{
				if (!IsDefined)
					throw new ISqlException($"Erorr: both column name and column type must be defined before autoincreament");

				isautoIncreament = value;
				if (isautoIncreament)
					this.Increament = new AutoIncreament(this.Name, 0, 1);
				else
					this.Increament = new AutoIncreament("null", 0, 1);
			}
		}

		public AutoIncreament Increament
		{
			get
			{
				return increament;
			}
			set
			{
				increament = new AutoIncreament(value.Name, value.GetValue(), value.GetIncreament());

				if (increament.Name.ToLower() == this.Name.ToLower())
					isautoIncreament = true;
				else
					isautoIncreament = false;
			}
		}

		public bool IsPrimaryKey
		{
			get
			{
				return isprimaryKey;
			}

			set
			{
				isprimaryKey = value;
				if (isprimaryKey)
					this.PrimaryKey = new PrimaryKey(this.Name, isprimaryKey);
				else
					this.PrimaryKey = new PrimaryKey("null", isprimaryKey);
			}
		}

		public PrimaryKey PrimaryKey
		{
			get { return primaryKey; }
			internal set
			{
				primaryKey = value; 
				if(primaryKey.Name.ToLower() == this.name.ToLower() && primaryKey.Key == true)
					this.isprimaryKey = true;
				else
					this.isprimaryKey = false;
			}
		}

		public int ColumnIndex
		{
			get
			{
				return col_index;
			}

			internal set
			{
				col_index = value;
			}
		}

		public bool IsDefined
		{
			get
			{
				return this.isDefined;
			}

			internal set
			{
				this.isDefined = value;
			}
		}

		public bool TypeDefined
		{
			get
			{
				return this.typeDefined;
			}

			internal set
			{
				typeDefined = value;
			}
		}

		public bool NameDefined
		{
			get
			{
				return nameDefined;
			}

			internal set
			{
				nameDefined = value;
			}
		}

		#endregion
		
		public object Clone()
        {
        	return this;
        }
        
        public Column Copy()
        {
        	return new Column(this.name, this.alias, this.dataType, this.acceptNull, this.size, this.defaultValue, this.isUnique,
        						this.check, this.foreignKey, this.isautoIncreament, this.isprimaryKey, this.columnDefault, this.col_index);
        }
		
		public override string ToString()
		{
			return $"ColumnName: '{this.Name}' ColumnType: '{this.DataType.ToString()}'";
		}
		
		public string ToHTML()
        {
        	return $"<td>{Name}</td>";
        }
	}
}
