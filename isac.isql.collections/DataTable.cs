using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Isac.Isql.Configurations;
using Isac.Isql.Permissions;


namespace Isac.Isql.Collections
{
	public class DataTable : IRowCollection, IComparable<DataTable>, ICloneable, IEquatable<DataTable>
	{
		
		#region FEILDS AND PROPERTIES
		private ColumnDefination colDef = new ColumnDefination();//Columns Collection
		private RowCollection body = new RowCollection();//Row Colloection	

		private string name = "";//table name
		private string alias = "`";//table alias name
		
		//for ignoring specific constraints
		public bool IgnoreAutoIncreament = false;
		public bool IgnoreNull = false;
		public bool IgnoreUnique = false;
		public bool IgnoreCheck = false;
		public bool IgnoreForeignKey = false;
		public bool IgnorePrimaryKey = false;
		
		//for ignoring all constraints
		private bool ingnoreAll = false;
		public bool IgnoreConstraints
		{
			get { return this.ingnoreAll; }
			set
			{
				ingnoreAll = value;
			}
		}
		
		//For setting and getting table name
		public string Name
		{
			get { return name; }
			set
			{
				Value v = new Value(value);
				if (v.Data.ToString().ToLower().Trim() == "null" || v.Data.ToString().ToLower().Trim() == "")
					throw new ISqlException($"Error: cannot name a table to 'null' or empty string");

                if (v.Data.ToString().Trim() == "`")
                    throw new ISqlException($"Error: prohibited symbol cannot name table");

				name = v.Data.ToString().Trim();
			}
		}
		
		//For setting and getting table alias name
		public string Alias
		{
			get { return alias; }
			set
			{
				Value v = new Value(value);
				if (v.Data.ToString().ToLower().Trim() == "null" || v.Data.ToString().ToLower().Trim() == "")
					throw new ISqlException($"Error: cannot alias table a name to 'null' or an empty string");
				
				if (name.Trim() == "")
					throw new ISqlException("Error: table name not declaered");
				
				alias = v.Data.ToString().Trim();
				
			}
		}
		
		//returns table name if no alias, else return alias name
		public string RealName
		{
			get { return (Alias == "`") ? Name : Alias; } 
		}
		
		//fix capacity of the table
		public int Capacity
		{
			get { return body.Capacity; }
		}
		
		//checks if row is empty
		public bool IsEmpty
		{
			get { return body.IsEmpty; }
		}
		
		//checks if column is empty
		public bool ColumnEmpty
		{
			get { return Head.IsEmpty; }
		}
		
		public int ColumnCount
		{
			get { return Head.Count; }
		}
		
		//to have access to the table's row collection
		internal RowCollection Body
		{
			get { return body.Copy(); }
			set
			{
				body = value;
			}
		}
		
		//to have access to the table's column defination
		internal ColumnDefination Head
		{
			get
			{
				return colDef.Copy();
			}
			set
			{
				colDef = value;
				body.Clear();
				colDef.can_modify = false;
			}
		}

		#endregion
		
		#region EVENTS
		//for adding of column
		public event EventHandler<ColumnAddEventArgs> ColumnAdd;
		protected virtual void OnColumnAdd(ColumnAddEventArgs e)
		{
			ColumnAdd?.Invoke(this, e);
		}
		
		//for column remove
		public event EventHandler<ColumnRemoveEventArgs> ColumnRemove;
		protected virtual void OnColumnRemove(ColumnRemoveEventArgs e)
		{
			ColumnRemove?.Invoke(this, e);
		}
		
		//for column modification
		public event EventHandler<ColumnSetEventArgs> ColumnSet;
		protected virtual void OnColumnSet(ColumnSetEventArgs e)
		{
			ColumnSet?.Invoke(this, e);
		}
		
		//for column retirval
		public event EventHandler<ColumnGetEventArgs> ColumnGet;
		protected virtual void OnColumnGet(ColumnGetEventArgs e)
		{
			ColumnGet?.Invoke(this, e);
		}
		#endregion

		#region CONSTRUCTORS
		//default constructor, for class inialization
		public DataTable()
		{
			colDef = new ColumnDefination();
			body = new RowCollection();
		}
		
		//for class initialization and fixed capacity
		public DataTable(int capacity)
		{
			colDef = new ColumnDefination();
			body = new RowCollection(capacity);
		}
		
		//for class initialization and defined column
		public DataTable(ColumnDefination defination)
		{
			Defined(defination);
			body = new RowCollection();
		}
		
		//for class initialization, defined columns and fixed capacity
		public DataTable(ColumnDefination defination, int capacity)
		{
			Defined(defination);
			body = new RowCollection(capacity);
		}
		
		internal DataTable(ColumnDefination defination, RowCollection collection)
		{
			this.colDef = defination; this.body = collection;
		}
		
		#endregion
		
		#region INDEXERS
		//to get specific row index
		public Row this[int row]
		{
			get
			{
				if ((row < 0) || (row >= body.Count()))
					throw new ISqlException($"Error: index is out of range\nIndex:'{row}'\nSize: '{this.Size()}'");

				return body[row];
			}
		}
		
		//to get a whole column with the rows under it
		public IEnumerable<object> this[string column]
		{
			get
			{
				if (colDef.IsEmpty)
					throw new ISqlException($"Error: collection is empty");

				if (!colDef.HasColumn(column))
					throw new ISqlColumnNotFoundException($"Error: column '{column}' can not be found");

				int index = colDef.GetColumn(column).ColumnIndex;
				foreach (var item in body)
				{
					yield return item.GetCell(index).Value;
				}
			}
			//set { /* set the specified index to value here */ }
		}
		
		//to get a whole column with a specified range of columns
		public IEnumerable<object> this[string column, int offset = 0, int limit = 5]
		{
			get
			{
				if (colDef.HasColumn(column) && !colDef.IsEmpty)
				{
					int index = colDef.GetColumn(column).ColumnIndex;
					foreach (Row row in body.Skip(offset).Take(limit))
					{
						yield return row[index].Value;
					}
				}
			}
		}
		#endregion
		
        public string ToHTML()
        {
            string head = Head.ToHTML();
            string body = Body.ToHTML();

            return "<table>" +
                "<thead>" + head + "</thead>" +
                "<tbody>" + body + "</tbody>" +
                "</table>";
        }

		internal void RepeatSelf(int times)
		{
			body.RepeatSelf(times);
		}
		
		public void DefinedColumn(params Column[] columns)
		{
			if (body.Size == 0)
			{
				colDef.can_modify = true;
				foreach (Column col in columns)
				{
					col.ColumnIndex = colDef.Count;
					colDef.AddColumn(col);
					
					var colAdd = new ColumnAddEventArgs();
					colAdd.Column = col;
					OnColumnAdd(colAdd);
				}
			}
			else if (body.Size > 0)
			{
				
				foreach (Column col in columns)
				{
					col.ColumnIndex = colDef.Count;
					col.Parent = this;
					var colAdd = new ColumnAddEventArgs();
					colAdd.Column = col;
					OnColumnAdd(colAdd);
					colDef.AddColumn(col);
				}
				
				for (int x = 0; x < body.Size; x++)
				{

					Cell[] cell = new Cell[columns.Length];
					Row r = body[x]; List<Cell> cell_list = r.GetCellArray().ToList<Cell>();
					int count = 0;
					foreach (Column col in columns)
					{
						cell_list.Add(col.ColumnDefault);
						cell_list[count].CellAddress = col.Name + ":" + x;
						cell_list[count].SelfIndex = col.ColumnIndex;
						count++;
					}
					body[x] = new Row(cell_list.ToArray<Cell>());

				}
			}
		}

		public void RemoveColumn(string column)
		{
			if (!this.Head.HasColumn(column))
				throw new ISqlColumnNotFoundException($"Error: column '{column}' could not be found");
		
			RowCollection nrc = this.body.Copy();

			int index = colDef.GetColumn(column).ColumnIndex;

			this.colDef.RemoveColumn(column);
			
			this.Clear();
			for (int i = 0; i < nrc.Size; i++)
			{
				List<Cell> lrow = nrc[i].GetCellArray().ToList<Cell>();
				lrow.RemoveAt(index);
				Row row = new Row(lrow.ToArray<Cell>());

				if (row.Length == 0)
				{
					this.Head.can_modify = false;
					break;
				}
				this.AddRow(row);
			}
			this.Head.can_modify = false;
			nrc.Clear();
		
		}

		public void RemoveColumns(params string[] columns)
		{
			foreach (var c in columns)
				RemoveColumn(c);
		}

		public bool HasRow(Row row)
		{
			return body.HasRow(row);
		}

		//Done
		public void AddRow(Row row)
		{
			if (!this.colDef.IsDefined)
				throw new ISqlException($"Error: no columns are defined");

			if (row.Length != colDef.Count)
				throw new ISqlException($"Error: {((row.Length > colDef.Count) ? "input row's length is greater than the specified column count" : "input row's length is lower than the specified column count")}");

			row.Index = body.Size;
			for (int i = 0; i < colDef.Count; i++)
			{
				colDef[i].ColumnIndex = i;

				//defaults
				if (row[i].Value.ToString().ToLower() == "null" && colDef[i].DefaultValue.GetDefault().ToString().ToLower() != "null")
					row[i].Value = Parser.DataConverter(colDef[i].DataType, colDef[i].DefaultValue.GetDefault());

				if (IgnoreConstraints)
					goto SKIP;

				//autoIncreament
				if (!IgnoreAutoIncreament)
				{
					if (colDef[i].IsAutoIncreament)
					{
						colDef[i].Increament.Increament();
						row[i].Value = colDef[i].Increament.GetValue();
					}
				}

				//column size
				if (colDef[i].Size > 0)
				{
					if (Parser.DataConverter(typeof(string), row[i].ToString()).Length > colDef[i].Size)
						throw new ISqlArguementException($"Error: '{row[i]}' length is too long. maximum character length is '{colDef[i].Size}'");
				}

				//nulls
				if (!IgnoreNull && colDef[i].AcceptNull == false && (row[i].Value.ToString() == "" || row[i].Value.ToString().ToLower() == "null"))
					throw new ISqlArguementException($"Error: column '{colDef[i].Name}' can not accept a null or empty data");

				//unique
				if (!IgnoreUnique && colDef[i].IsUnique) // && colDef[i].PrimaryKey.Name.ToLower() == colDef[i].Name.ToLower())
				{
					if (this[colDef[i].Name].Any(x => x.ToString().ToLower() == row[i].ToString().ToLower()))
						throw new ISqlException($"Error: the data '{row[i]}' exists in the table...\nthis column is Unique '{colDef[i].Name}'");

					else if (Parser.IsStringType(row[i].Value) && row[i].ToString().ToLower() == "null")
						throw new ISqlException($"Error: the column '{colDef[i].Name}' can not be left 'null', Unique is set on it");
				}

				//primary key
				if (!IgnorePrimaryKey && colDef[i].IsPrimaryKey && colDef[i].PrimaryKey.Name.ToLower() == colDef[i].Name.ToLower())
				{
					if (this[colDef[i].Name].Any(x => x.ToString().ToLower() == row[i].ToString().ToLower()))
						throw new ISqlException($"Error: the data '{row[i]}' exists in the table...\nPrimary Key is set on column '{colDef[i].Name}'\nData: '{string.Join("\t", this[colDef[i].Name])}'");

					else if (Parser.IsStringType(row[i].Value) && row[i].ToString().ToLower() == "null")
						throw new ISqlException($"Error: the column '{colDef[i].Name}' can not be left 'null', PrimaryKey is set on it");
				}

				//Check
				if (!IgnoreCheck && colDef.HasColumn(colDef[i].Check.Name))
				{
					var logic = new Logistics.LogicExpressionEngine(colDef[i].Check.Criteria);
					string[] variables = logic.GetVariables();

					List<int> co_index = new List<int>();
					foreach (var lv in variables)
						co_index.Add(colDef.GetColumn(lv).ColumnIndex);

					List<object> obj_data = new List<object>();
					foreach (var da in co_index)
						obj_data.Add(row[da]);

					logic.SetVariables(variables.ToArray<string>(), obj_data.ToArray<object>());
					logic.Solve();

					if (!logic.Result)
						throw new ISqlException($"Error: this row does not meet the criteria set on this '{colDef[i].Name}'\nCriteria: {colDef[i].Check.Criteria}");
				}

				//ForeignKey
				if (!IgnoreForeignKey && colDef[i].ForeignKey.Name.ToLower() == colDef[i].Name.ToLower()) ;

				SKIP:

				//converting tho the column datatype
				if (row[i].Value.ToString().ToLower() != "null")
				{
					if (colDef[i].DataType == typeof(Character))
					{
						//Console.WriteLine("Length========>>>>>>: " + row[i].Value.ToString().Length);
						row[i].Value = (colDef[i].Size <= 0) ? Character.Parse(row[i].Value.ToString()) : Character.Parse(row[i].Value.ToString(), colDef[i].Size);

					}

					else if (colDef[i].DataType == typeof(Choice))
					{
						Choice c = new Choice();
						//row[i].Value = colDef[i].ColumnDefault.Value.HasChoice()
						c.HasChoice(row[i].Value);
						row[i].Value = Parser.DataConverter(colDef[i].DataType, $@"{row[i].Value}");
					}
					else
						row[i].Value = Parser.DataConverter(colDef[i].DataType, row[i].Value);
				}

				row[i].CellAddress = colDef[i].Name + ":" + row.Index;
			}

			body.AddRow(row);
		}

		internal void AddRow(Row row, bool wait)
		{
			if (!wait)
				AddRow(row);

			else
			{
				for (int i = 0; i < colDef.Count; i++)
				{
					colDef[i].ColumnIndex = i;

					//Console.WriteLine(colDef[i] + " - " + colDef[i].IsAutoIncreament);
					//defaults
					if (row[i].Value.ToString().ToLower() == "null" && colDef[i].DefaultValue.GetDefault().ToString().ToLower() != "null")
						row[i].Value = Parser.DataConverter(colDef[i].DataType, colDef[i].DefaultValue.GetDefault().ToString());

					if (IgnoreConstraints)
						goto SKIP;

					//autoIncreament
					if (!IgnoreAutoIncreament)
					{
						if (colDef[i].IsAutoIncreament)
						{
							colDef[i].Increament.Increament();
							row[i].Value = colDef[i].Increament.GetValue();
						}
					}

					//column size
					if (colDef[i].Size > 0)
					{
						if (Parser.DataConverter(typeof(string), row[i].ToString()).Length > colDef[i].Size)
							throw new ISqlArguementException($"Error: '{row[i]}' length is too long. maximum character length is '{colDef[i].Size}'");
					}

					//nulls
					if (!IgnoreNull && colDef[i].AcceptNull == false && (row[i].Value.ToString() == "" || row[i].Value.ToString().ToLower() == "null"))
						throw new ISqlArguementException($"Error: column '{colDef[i].Name}' can not accept a null or empty data");

					//unique
					if (!IgnoreUnique && colDef[i].IsUnique) // && colDef[i].PrimaryKey.Name.ToLower() == colDef[i].Name.ToLower())
					{
						if (this[colDef[i].Name].Any(x => x.ToString().ToLower() == row[i].ToString().ToLower()))
							throw new ISqlException($"Error: the data '{row[i]}' exists in the table...\nthis column is Unique '{colDef[i].Name}'");

						else if (Parser.IsStringType(row[i].Value) && row[i].ToString().ToLower() == "null")
							throw new ISqlException($"Error: the column '{colDef[i].Name}' can not be left 'null', Unique is set on it");
					}

					//primary key
					if (!IgnorePrimaryKey && colDef[i].IsPrimaryKey && colDef[i].PrimaryKey.Name.ToLower() == colDef[i].Name.ToLower())
					{
						if (this[colDef[i].Name].Any(x => x.ToString().ToLower() == row[i].ToString().ToLower()))
							throw new ISqlException($"Error: the data '{row[i]}' exists in the table...\nPrimary Key is set on column '{colDef[i].Name}'");

						else if (Parser.IsStringType(row[i].Value) && row[i].ToString().ToLower() == "null")
							throw new ISqlException($"Error: the column '{colDef[i].Name}' can not be left 'null', PrimaryKey is set on it");
					}

					//Check
					if (!IgnoreCheck && colDef.HasColumn(colDef[i].Check.Name))
					{
						var logic = new Logistics.LogicExpressionEngine(colDef[i].Check.Criteria);
						string[] variables = logic.GetVariables();

						List<int> co_index = new List<int>();
						foreach (var lv in variables)
							co_index.Add(colDef.GetColumn(lv).ColumnIndex);

						List<object> obj_data = new List<object>();
						foreach (var da in co_index)
							obj_data.Add(row[da]);

						logic.SetVariables(variables.ToArray<string>(), obj_data.ToArray<object>());
						logic.Solve();

						if (!logic.Result)
							throw new ISqlException($"Error: this row does not meet the criteria set on this '{colDef[i].Name}'\nCriteria: {colDef[i].Check.Criteria}");
					}

					//ForeignKey
					if (!IgnoreForeignKey && colDef[i].ForeignKey.Name.ToLower() == colDef[i].Name.ToLower()) ;

					SKIP:
					if (row[i].Value.ToString().ToLower() == "null" && colDef[i].AcceptNull)
						goto ByPass;

					//Console.WriteLine("Is Unique: " + colDef[i].IsUnique);

					//converting tho the column datatype
					if (row[i].Value.ToString().ToLower() != "null")
					{
						row[i].Value = Parser.DataConverter(colDef[i].DataType, row[i].Value);
					}

				ByPass:
					row[i].CellAddress = colDef[i].Name + ":" + row.Index;
				}
			}

			body.AddRow(row);
		}

		public void AddRow(params object[] values)
		{
			if (!colDef.IsDefined)
				throw new ISqlException($"Error: no columns defined");

			if (values.Length != colDef.Count)
				throw new ISqlException($"Error: not all values have coresponding columns");

			Cell[] cells = new Cell[colDef.Count]; int count = 0;
			foreach (var val in values)
			{
				cells[count] = new Cell(val);
				count++;
			}
			AddRow(new Row(cells));
		}

		//Done
		public void AddRows(params Row[] rows)
		{
			foreach (Row row in rows)
			{
				if (row.Length != colDef.Count)
					throw new ISqlException($"Error: not all cells have coresponding columns");

				row.Index = body.Size;
				AddRow(row);
			}
		}

		//Done
		public void RemoveRow(int rowIndex)
		{
			body.RemoveRow(rowIndex);
		}

		public void RemoveIndex(params int[] index)
		{
			body.RemoveIndex(index);
		}

		//Done
		public void RemoveRange(int index, int count)
		{
			body.RemoveRange(index, count);
		}
		
		public object Clone()
        {
        	return this;
        }
        
        public DataTable Copy()
        {
        	return new DataTable(this.colDef.Copy(), this.body.Copy());
        }
		
		public static void GetCopy(DataTable sourceDataTable, DataTable destinationDataTable)
		{
			/*unsafe
			{
				TypedReference tr = _
				DataTable* srcDt = &sourceDataTable.Copy();
				DataTable* destDt = &destinationDataTable;
				destDt = srcDt;
			}*/
			
		}

		//this should return a datatable instead of IEnumerable<Row>
		public IEnumerable<Row> GetRange(int offset, int limit)
		{
			return body.GetRange(offset, limit);
		}

		public IEnumerable<string> ColumnNames()
		{
			foreach (var cols in colDef)
				yield return cols.Name;
		}

        public int ColumnIndex(string column)
        {
            return this.colDef.ToList<Column>().FindIndex(x => x.Name.ToLower() == column.ToLower());
        }


        public IEnumerable<int> GetColumnIndexes(params string[] columns)
        {
            int i = 0;
            List<int> indexes = Enumerable.Repeat(-1, columns.Length).ToList<int>();
            foreach (var col in colDef)
            {
                if (columns.Contains(col.Name.ToLower()))
                {
                    indexes[columns.ToList<string>().FindIndex(x => x.ToLower() == col.Name.ToLower())] = i;
                }
                i++;
            }

            return indexes.AsEnumerable<int>();
        }
        
        internal Dictionary<string, Type> GetNameDataType(string parent_name = null, params int[] columnIndexes)
        {
            Dictionary<string, Type> name_type = new Dictionary<string, Type>();
            foreach (var index in columnIndexes)
            {
                if (index < 0 || index >= colDef.Count)
                    throw new ISqlException($"Error: index out of bounds");

                var col = colDef[index];
                if (name_type.ContainsKey((parent_name != null) ? parent_name + col.Name.ToLower() : col.Name.ToLower()))
                    name_type[(parent_name != null) ? parent_name + col.Name.ToLower() : col.Name.ToLower()] = (col.TypeDefined) ? col.DataType : typeof(None);
                else
                    name_type.Add((parent_name != null) ? parent_name + col.Name.ToLower() : col.Name.ToLower(), (col.TypeDefined) ? col.DataType : typeof(None));
            }

            return name_type;
        }

		public void Defined(ColumnDefination newDefination)
		{
			colDef.can_modify = true;
			colDef = new ColumnDefination();

			body.Clear();

			foreach (var col in newDefination)
			{
				colDef.AddColumn(col);
			}
			colDef.can_modify = false;
		}

		public void SetColumn(Column column, params Cell[] cells)
		{
			//to increase the cell length to meet the table size
			if (cells.Length < body.Size)
			{
				int add = body.Size - cells.Length;
				//Cell[] nCells = ;
				List<Cell> ces = cells.ToList<Cell>();
				ces.AddRange(Enumerable.Repeat(new Cell(Parser.DataConverter(column.DataType, column.DefaultValue.GetDefault().ToString())), add).ToArray<Cell>());
				cells = ces.ToArray<Cell>();
			}
			
			column.ColumnIndex = colDef.Count;
			var colAdd = new ColumnAddEventArgs();
			colAdd.Column = column;
			OnColumnAdd(colAdd);
			colDef.AddColumn(column);
			
			if(body.Size > 0)
			{
				for (int x = 0; x < body.Size; x++)
				{
					cells[x].CellAddress = column.Name + ":" + x;
					cells[x].SelfIndex = column.ColumnIndex;
	
					Row r = body[x].Copy();
					List<Cell> cell_list = r.GetCellArray().ToList<Cell>();
					int count = 0;
	
					cell_list.Add(cells[x]);
					count++;
	
					body[x] = new Row(cell_list.ToArray<Cell>());
			}
			}
			else
			{
				foreach(var cell in cells)
				{
					body.AddRow(new Row(cell));
				}
			}
			
			colDef.can_modify = false;
		}

		public void OrderBy(params string[] columns)
		{
			int currIndex = 0, maxIndex = columns.Length - 1;
			for (int i = 0; i < body.Size; i++)
			{
				for (int j = 0; j < body.Size; j++)
				{
				Retry:
					if (!(j + 1 >= body.Size))
					{
						Row temprow = this[j];
						Cell cell1 = temprow.GetCell(colDef.GetColumn(columns[currIndex]).ColumnIndex);
						Cell cell2 = this[j + 1].GetCell((colDef.GetColumn(columns[currIndex]).ColumnIndex));
						int val = cell1.CompareTo(cell2);
						
						if (val >= 1)
						{
							body[j] = this[j + 1];
							body[j + 1] = temprow;
							currIndex = 0;
							continue;
						}
						else if (val <= -1)
						{
							currIndex = 0;
							continue;
						}
						else
						{
							currIndex++;
							if (!(currIndex > maxIndex))
								goto Retry;

							currIndex = 0;
							continue;
						}
					}
				}
			}
		}

		public void OrderByDes(params string[] columns)
		{
			int currIndex = 0, maxIndex = columns.Length - 1;// bool retry = false;
			for (int i = body.Size - 1; i >= 0; i--)
			{
				for (int j = body.Size - 1; j >= 0; j--)
				{
				Retry:
					if (!(j - 1 <= -1))
					{

						Row temprow = this[j];

						Cell cell1 = temprow.GetCell(Head.GetColumn(columns[currIndex]).ColumnIndex);

						Cell cell2 = this[j - 1].GetCell(Head.GetColumn(columns[currIndex]).ColumnIndex);

						int val = cell1.CompareTo(cell2);
						if (val >= 1)
						{
							body[j] = this[j - 1];
							body[j - 1] = temprow;
							currIndex = 0;
							continue;
						}
						else if (val <= -1)
						{
							currIndex = 0;
							continue;
						}
						else if (val == 0)
						{
							currIndex++;
							if (!(currIndex > maxIndex))
								goto Retry;
							currIndex = 0;
							continue;
						}

					}
				}
			}
		}
		
		//public void GroupBy(param)

		public Row FindRow(Func<Row, bool> filter)
		{
			return body.FindRow(filter);
		}

		public IEnumerable<Row> FindRows(Func<Row, bool> filter)
		{
			return body.FindRows(filter);
		}

		public void EachRow(Action<Row> action)
		{
			
		}

		//public IEnumerable<Cell> (string column)
		public Cell MapCell(string cellAddress)
		{
			string[] address = cellAddress.ToLower().Split(new char[] { ':' });
			if (address.Length != 2)
				throw new ISqlException($"Error: this is not a valid cellAddress\nSyntax: <column_name>:<row_index>");

			return MapCell(address[0].Trim(), int.Parse(address[1].Trim()));
		}

		public Cell MapCell(string columnName, int rowIndex)
		{
			//Console.WriteLine("Alias: "+ Alias+" ColName: " + columnName+" RowIndex: "+rowIndex +" Value: "+ body[rowIndex].GetCell(this.Head.GetColumn(columnName).ColumnIndex));
			if (!Head.HasColumn(columnName))
				throw new ISqlColumnNotFoundException($"Error: '{columnName}' could not be found\nColumns: {string.Join("\n", Head.NameList())}");

			if (rowIndex < 0 || rowIndex >= body.Size)
				throw new ISqlArguementException($"Error: index was out of boundary");

			return body[rowIndex].GetCell(this.Head.GetColumn(columnName).ColumnIndex);
		}

		public void SetCellValue(string cellAddress, object value)
		{
			string[] address = cellAddress.ToLower().Split(new char[] { ':' });
			if (address.Length != 2)
				throw new ISqlException($"Error: this is not a valid cellAddress\nSyntax: <column_name>:<row>");

			SetCellValue(address[0], int.Parse(address[1]), value);
		}

		public void SetCellValue(string columnName, int rowIndex, dynamic value)
		{
			
			if (this.Head.HasColumn(columnName))
			{
				object nval = Parser.DataConverter(Head.GetColumn(columnName).DataType, value);
				if (nval.GetType().ToString().ToLower() == this.Head.GetColumn(columnName).DataType.ToString().ToLower())
				{
					Row nrow = body[rowIndex];
					nrow.Index = rowIndex;
					int idx = this.Head.GetColumn(columnName).ColumnIndex;
					Cell cle = new Cell();
					cle.CellAddress = columnName + ":" + nrow.Index;
					cle.Value = nval;
					nrow[idx] = cle;
				}
				else throw new ISqlTypeException($"Error: the type of the value '{value}' does not correspond with the type of the column\nValueType: '{value.GetType()}'\nColumnName: '{this.Head.GetColumn(columnName).Name}'\nColumnType: '{this.Head.GetColumn(columnName).DataType}'");
			}
			else throw new ISqlColumnNotFoundException($"Error: the column '{columnName}' is not defined in this table");
		}

		public void Clear()
		{
			if (colDef == null || colDef.Count == 0)
				colDef = new ColumnDefination();

			body.Clear();
		}

		public int Size()
		{
			return body.Size;
		}

		public string Dimension()
		{
			return body.Size + "x" + colDef.Count;
		}

		public Detail GetDetail(int rowIndex, params string[] columns)
		{
			List<string> col_names = new List<string>();
			List<object> datas = new List<object>();
			List<Type> type = new List<Type>();
			List<bool> nulls = new List<bool>();
			List<int> size = new List<int>();
			List<Default> defaultVals = new List<Default>();
			List<bool> unique = new List<bool>();
			List<Check> checks = new List<Check>();
			List<ForeignKey> foreigns = new List<ForeignKey>();
			List<AutoIncreament> autos = new List<AutoIncreament>();
			List<PrimaryKey> primaryKeys = new List<PrimaryKey>();
			List<Column> columns1 = new List<Column>();
			List<Cell> columnDEF = new List<Cell>();

			foreach (string column in columns)
			{
				string[] coldet = Parser.ColumnDetector(column);

				if (coldet[0].Trim() == "`" && this.Alias.Trim() == "`")
				{
					if (coldet[1] == "*")
					{
						col_names.AddRange(this.ColumnNames());

						if (rowIndex < Size()) ;
						datas.AddRange(this[rowIndex].GetCurrentValues());

						columns1.AddRange(this.Head.GetColumns());
						foreach (var d in Head)
						{
							type.Add(d.DataType);
							defaultVals.Add(d.DefaultValue);
							nulls.Add(d.AcceptNull);
							size.Add(d.Size);
							unique.Add(d.IsUnique);
							checks.Add(d.Check);
							foreigns.Add(d.ForeignKey);
							autos.Add(d.Increament);
							primaryKeys.Add(d.PrimaryKey);
							columnDEF.Add(d.ColumnDefault);
						}
					}
					else if (this.Head.HasColumn(coldet[1]))
					{
						col_names.Add(coldet[1]);

						if (rowIndex < Size()) ;
						datas.Add(this.MapCell(coldet[1], rowIndex).Value);

						type.Add(Head.GetColumn(coldet[1]).DataType);
						columns1.Add(Head.GetColumn(coldet[1]));
						defaultVals.Add(Head.GetColumn(coldet[1]).DefaultValue);
						nulls.Add(Head.GetColumn(coldet[1]).AcceptNull);
						size.Add(Head.GetColumn(coldet[1]).Size);
						unique.Add(Head.GetColumn(coldet[1]).IsUnique);
						checks.Add(Head.GetColumn(coldet[1]).Check);
						foreigns.Add(Head.GetColumn(coldet[1]).ForeignKey);
						autos.Add(Head.GetColumn(coldet[1]).Increament);
						primaryKeys.Add(Head.GetColumn(coldet[1]).PrimaryKey);
						columnDEF.Add(Head.GetColumn(coldet[1]).ColumnDefault);
					}

				}
				else if (coldet[0].Trim() != "`" && (this.Alias == "`") ? "@" + this.Name.ToLower() == coldet[0].ToLower() : "@" + this.Alias.ToLower() == coldet[0].ToLower())
				{
					name = this.Name;
					alias = this.Alias;
					if (coldet[1] == "*")
					{
						col_names.AddRange(this.ColumnNames());

						if (rowIndex < Size()) ;
						datas.AddRange(this[rowIndex].GetCurrentValues());

						columns1.AddRange(this.Head.GetColumns());
						foreach (var d in Head)
						{
							type.Add(d.DataType);
							defaultVals.Add(d.DefaultValue);
							nulls.Add(d.AcceptNull);
							size.Add(d.Size);
							unique.Add(d.IsUnique);
							checks.Add(d.Check);
							foreigns.Add(d.ForeignKey);
							autos.Add(d.Increament);
							primaryKeys.Add(d.PrimaryKey);
							columnDEF.Add(d.ColumnDefault);
						}
					}
					else if (this.Head.HasColumn(coldet[1]))
					{
						col_names.Add(coldet[1]);

						if (rowIndex < Size())
							datas.Add(this.MapCell(coldet[1], rowIndex).Value);

						type.Add(Head.GetColumn(coldet[1]).DataType);
						columns1.Add(Head.GetColumn(coldet[1]));
						defaultVals.Add((Head.GetColumn(coldet[1]).DefaultValue));
						nulls.Add(Head.GetColumn(coldet[1]).AcceptNull);
						size.Add(Head.GetColumn(coldet[1]).Size);
						unique.Add(Head.GetColumn(coldet[1]).IsUnique);
						checks.Add(Head.GetColumn(coldet[1]).Check);
						foreigns.Add(Head.GetColumn(coldet[1]).ForeignKey);
						autos.Add(Head.GetColumn(coldet[1]).Increament);
						primaryKeys.Add(Head.GetColumn(coldet[1]).PrimaryKey);
						columnDEF.Add(Head.GetColumn(coldet[1]).ColumnDefault);
					}
				}
				else if (Head.HasColumn(column) && column.Contains("@") && Alias == "`")
				{
					col_names.Add(coldet[1]);

					if (rowIndex < Size())
						datas.Add(this.MapCell(column, rowIndex).Value);

					type.Add(Head.GetColumn(column).DataType);
					columns1.Add(Head.GetColumn(column));
					defaultVals.Add(Head.GetColumn(column).DefaultValue);
					nulls.Add(Head.GetColumn(column).AcceptNull);
					size.Add(Head.GetColumn(column).Size);
					unique.Add(Head.GetColumn(column).IsUnique);
					checks.Add(Head.GetColumn(column).Check);
					foreigns.Add(Head.GetColumn(column).ForeignKey);
					autos.Add(Head.GetColumn(column).Increament);
					primaryKeys.Add(Head.GetColumn(column).PrimaryKey);
					columnDEF.Add(Head.GetColumn(column).ColumnDefault);
					//Console.WriteLine("Yes: "+ MapCell(column, rowIndex).Value);
				}
			}
			return new Detail()
			{
				Name = this.Name,
				Alias = this.Alias,
				ColumnNames = col_names.ToArray<string>(),
				RowDatas = datas.ToArray<object>(),
				DataTypes = type.ToArray<Type>(),
				Columns = columns1.ToArray<Column>(),
				DefaultValues = defaultVals.ToArray<Default>(),
				AcceptNull = nulls.ToArray<bool>(),
				Size = size.ToArray<int>(),
				IsUnique = unique.ToArray<bool>(),
				Checks = checks.ToArray<Check>(),
				ForeignKeys = foreigns.ToArray<ForeignKey>(),
				AutoIncreaments = autos.ToArray<AutoIncreament>(),
				PrimaryKeys = primaryKeys.ToArray<PrimaryKey>(),
				ColumnDefault = columnDEF.ToArray<Cell>()
			};
		}

		public Detail GetDetail(params string[] columns)
		{
			List<string> col_names = new List<string>();
			List<object> datas = new List<object>();
			List<Type> type = new List<Type>();
			List<bool> nulls = new List<bool>();
			List<int> size = new List<int>();
			List<Default> defaultVals = new List<Default>();
			List<bool> unique = new List<bool>();
			List<Check> checks = new List<Check>();
			List<ForeignKey> foreigns = new List<ForeignKey>();
			List<AutoIncreament> autos = new List<AutoIncreament>();
			List<PrimaryKey> primaryKeys = new List<PrimaryKey>();
			List<Column> columns1 = new List<Column>();
			List<Cell> columnDEF = new List<Cell>();

			foreach (string column in columns)
			{
				string[] coldet = Parser.ColumnDetector(column);

				if (coldet[0].Trim() == "`" && this.Alias.Trim() == "`")
				{

					if (coldet[1] == "*")
					{
						col_names.AddRange(this.ColumnNames());
						//datas.AddRange(this[rowIndex].GetCurrentValues());
						columns1.AddRange(this.Head.GetColumns());
						foreach (var d in Head)
						{
							type.Add(d.DataType);
							defaultVals.Add(d.DefaultValue);
							nulls.Add(d.AcceptNull);
							size.Add(d.Size);
							unique.Add(d.IsUnique);
							checks.Add(d.Check);
							foreigns.Add(d.ForeignKey);
							autos.Add(d.Increament);
							primaryKeys.Add(d.PrimaryKey);
							columnDEF.Add(d.ColumnDefault);
						}
					}
					else if (this.Head.HasColumn(coldet[1]))
					{
						col_names.Add(coldet[1]);
						//datas.Add(this.MapCell(coldet[1], rowIndex).Value);
						type.Add(Head.GetColumn(coldet[1]).DataType);
						columns1.Add(Head.GetColumn(coldet[1]));
						defaultVals.Add(Head.GetColumn(coldet[1]).DefaultValue);
						nulls.Add(Head.GetColumn(coldet[1]).AcceptNull);
						size.Add(Head.GetColumn(coldet[1]).Size);
						unique.Add(Head.GetColumn(coldet[1]).IsUnique);
						checks.Add(Head.GetColumn(coldet[1]).Check);
						foreigns.Add(Head.GetColumn(coldet[1]).ForeignKey);
						autos.Add(Head.GetColumn(coldet[1]).Increament);
						primaryKeys.Add(Head.GetColumn(coldet[1]).PrimaryKey);
						columnDEF.Add(Head.GetColumn(coldet[1]).ColumnDefault);
					}

				}
				else if (coldet[0].Trim() != "`" && (this.Alias == "`") ? "@" + this.Name.ToLower() == coldet[0].ToLower() : "@" + this.Alias.ToLower() == coldet[0].ToLower())
				{
					name = this.Name;
					alias = this.Alias;
					if (coldet[1] == "*")
					{
						col_names.AddRange(this.ColumnNames());
						//datas.AddRange(this[rowIndex].GetCurrentValues());
						columns1.AddRange(this.Head.GetColumns());
						foreach (var d in Head)
						{
							type.Add(d.DataType);
							defaultVals.Add(d.DefaultValue);
							nulls.Add(d.AcceptNull);
							size.Add(d.Size);
							unique.Add(d.IsUnique);
							checks.Add(d.Check);
							foreigns.Add(d.ForeignKey);
							autos.Add(d.Increament);
							primaryKeys.Add(d.PrimaryKey);
							columnDEF.Add(d.ColumnDefault);
						}
					}
					else if (this.Head.HasColumn(coldet[1]))
					{
						col_names.Add(coldet[1]);
						//datas.Add(this.MapCell(coldet[1], rowIndex).Value);
						type.Add(Head.GetColumn(coldet[1]).DataType);
						columns1.Add(Head.GetColumn(coldet[1]));
						defaultVals.Add((Head.GetColumn(coldet[1]).DefaultValue));
						nulls.Add(Head.GetColumn(coldet[1]).AcceptNull);
						size.Add(Head.GetColumn(coldet[1]).Size);
						unique.Add(Head.GetColumn(coldet[1]).IsUnique);
						checks.Add(Head.GetColumn(coldet[1]).Check);
						foreigns.Add(Head.GetColumn(coldet[1]).ForeignKey);
						autos.Add(Head.GetColumn(coldet[1]).Increament);
						primaryKeys.Add(Head.GetColumn(coldet[1]).PrimaryKey);
						columnDEF.Add(Head.GetColumn(coldet[1]).ColumnDefault);
					}
				}
				else if (Head.HasColumn(column) && column.Contains("@") && Alias == "`")
				{
					col_names.Add(coldet[1]);
					//datas.Add(this.MapCell(column, rowIndex).Value);
					type.Add(Head.GetColumn(column).DataType);
					columns1.Add(Head.GetColumn(column));
					defaultVals.Add(Head.GetColumn(column).DefaultValue);
					nulls.Add(Head.GetColumn(column).AcceptNull);
					size.Add(Head.GetColumn(column).Size);
					unique.Add(Head.GetColumn(column).IsUnique);
					checks.Add(Head.GetColumn(column).Check);
					foreigns.Add(Head.GetColumn(column).ForeignKey);
					autos.Add(Head.GetColumn(column).Increament);
					primaryKeys.Add(Head.GetColumn(column).PrimaryKey);
					columnDEF.Add(Head.GetColumn(column).ColumnDefault);
					//Console.WriteLine("Yes: "+ MapCell(column, rowIndex).Value);
				}
			}
			return new Detail()
			{
				Name = this.Name,
				Alias = this.Alias,
				ColumnNames = col_names.ToArray<string>(),
				RowDatas = datas.ToArray<object>(),
				DataTypes = type.ToArray<Type>(),
				Columns = columns1.ToArray<Column>(),
				DefaultValues = defaultVals.ToArray<Default>(),
				AcceptNull = nulls.ToArray<bool>(),
				Size = size.ToArray<int>(),
				IsUnique = unique.ToArray<bool>(),
				Checks = checks.ToArray<Check>(),
				ForeignKeys = foreigns.ToArray<ForeignKey>(),
				AutoIncreaments = autos.ToArray<AutoIncreament>(),
				PrimaryKeys = primaryKeys.ToArray<PrimaryKey>(),
				ColumnDefault = columnDEF.ToArray<Cell>()
			};
		}

        public List<Row> GetRowsCells(int row, int? endAt, params int[] columnsIndex)
        {
            Console.WriteLine("GetRowsCells: ---------------------START---------------------");
            Console.WriteLine("--- Row: " + row);
            Console.WriteLine("--- EndAt: " + endAt);
            Console.WriteLine("--- ColumnIndex: " + string.Join(", ", columnsIndex));
            Console.WriteLine("GetRowsCells: ---------------------END-----------------------");

            if (row < 0 || row >= body.Size)
                throw new ISqlException($"Error: row out of bounds");

            List<Row> rows = new List<Row>();
            if(endAt.HasValue)
            {
                while(row < endAt)
                {
                    Console.WriteLine("ROWDEF: " + string.Join(", ", body[row][columnsIndex]));
                    rows.Add(new Row(body[row][columnsIndex].ToCellArray()));
                    row++;
                }
                Console.WriteLine("Returned Length: " + rows.Count);
                return rows;
            }
            
            rows.Add(new Row(body[row][columnsIndex].ToCellArray()));
            Console.WriteLine("Returned Length1: " + rows.Count);
            return rows;
        }


        public object[] GetRowCells(int row, params int[] columnsIndex)
        {
            return body[row][columnsIndex];
        }



        public void SetRowsCell(List<Row> rows, int startPoint, int endPoint, params int[] columnIndexes)
        {
            foreach (var row in rows)
            {
                Row temp_row = new Row(colDef.Count);
                temp_row[columnIndexes] = row.GetCurrentValues();

                if (body.Size <= startPoint)
                {
                    body.AddRow(temp_row);
                }

                else if(body.Size > startPoint)
                {
                    body[startPoint][columnIndexes] = row.GetCurrentValues();

                }
                if (startPoint == endPoint)
                    break;

                startPoint++;
            }
        }
        
        
        /*
        public IEnumerable<Row> GetRowsRange(int fromRow, int toRow, params int[] columnsIndex)
        {

        }*/

		public DataTable Limit(int limit)
		{
			return new DataTable(this.Head, new RowCollection(this.body.Copy().Take(limit).ToArray<Row>()));
		}

		public DataTable Offset(int offset)
		{
			return new DataTable(this.Head, new RowCollection(this.body.Copy().Skip(offset).ToArray<Row>()));
		}

		public DataTable Where(string expression)
		{
			var logic = new Logistics.LogicExpressionEngine(expression);
			string[] variables = logic.GetVariables();

			string missed = "";
			int[] indexs = new int[variables.Length];
			int incount = 0;
			foreach (var ind in Head)
			{
				if (this.Head.HasColumn(variables[incount]) && incount < variables.Length)
				{
					indexs[incount] = this.Head.GetColumn(variables[incount]).ColumnIndex;
					incount++; goto GO_ON;
				}

				var coldet = Parser.ColumnDetector(variables[incount]);

				if (this.Name.ToLower() == coldet[0].ToLower().Replace("@", "") && incount < variables.Length)
				{
					indexs[incount] = this.Head.GetColumn(coldet[1]).ColumnIndex;
					incount++; goto GO_ON;
				}

			GO_ON:
				if (incount >= variables.Length)
					break;
				else continue;

				throw new ISqlColumnNotFoundException($"Error: column '{variables[incount]}' could not be found in this table");
			}

			var releaseDataTable = new DataTable();
			releaseDataTable.Defined(this.Head);

			int row = 0;
			foreach (var d in body)
			{
				object[] var_data = body[row].GetValues(indexs).ToArray<object>();
				logic.SetVariables(variables, var_data);
				logic.Solve();
				if (logic.Result)
				{
					releaseDataTable.AddRow(this[row]);
				}
				row++;
			}
			return releaseDataTable;
		}

		public void RemoveWhere(string expression)
		{
			var logic = new Logistics.LogicExpressionEngine(expression);
			string[] variables = logic.GetVariables();

			string missed = "";
			int[] indexs = new int[variables.Length];
			int incount = 0;
			foreach (var ind in Head)
			{
				if (this.Head.HasColumn(variables[incount]) && incount < variables.Length)
				{
					indexs[incount] = this.Head.GetColumn(variables[incount]).ColumnIndex;
					incount++; goto GO_ON;
				}

				var coldet = Parser.ColumnDetector(variables[incount]);

				if (this.Name.ToLower() == coldet[0].ToLower().Replace("@", "") && incount < variables.Length)
				{
					indexs[incount] = this.Head.GetColumn(coldet[1]).ColumnIndex;
					incount++; goto GO_ON;
				}

			GO_ON:
				if (incount >= variables.Length)
					break;

				else continue;

				throw new ISqlColumnNotFoundException($"Error: column '{variables[incount]}' could not be found in this table");
			}

			int row = 0;
			for (; row < body.Size; row++)
			{
				object[] var_data = body[row].GetValues(indexs).ToArray<object>();
				logic.SetVariables(variables, var_data);
				logic.Solve();
				if (logic.Result)
				{
					if (body.Size == 0)
						break;
					//Console.WriteLine(string.Join("_", this[0].GetCurrentValues()));
					body.RemoveRow(row);
					if (row < body.Size)
						body[row].Index = row;

					row--;
					if (row < 0)
						row = 0;
				}

			}
		}

		public void Distinct()
		{
			List<string> distinct_str = new List<string>();

			for (int i = 0; i < body.Size; i++)
			{
				if (distinct_str.Contains(string.Join("\t", body[i].GetCurrentValues()).ToLower()))
				{
					body.RemoveRow(i); i--;
					continue;
				}
				distinct_str.Add(string.Join("\t", body[i].GetCurrentValues()).ToLower());
			}
			distinct_str.Clear();
		}

		public void DistinctBy(string column)
		{
			if (Head.HasColumn(column))
			{
				List<string> distinct_str = new List<string>();
				for (int i = 0; i < body.Size; i++)
				{
					if (distinct_str.Contains(MapCell(column, i).Value.ToString().ToLower()))
					{
						body.RemoveRow(i); i--;
						continue;
					}
					distinct_str.Add(MapCell(column, i).Value.ToString().ToLower());
				}
				distinct_str.Clear();
			}
		}

		public int CompareTo(DataTable other)
		{
			int h1 = this.Head.CompareTo(other.Head);
			int b1 = this.Body.CompareTo(other.Body);

			int h2 = other.Head.CompareTo(this.Head);
			int b2 = other.Body.CompareTo(this.Body);

			return (h1 + b1).CompareTo((h2 + b2));
		}
		
        public bool Equals(DataTable other)
        {
        	return (this.colDef.Equals(other.colDef) && this.body.Equals(other.body));
        }
        
        public override bool Equals(object obj)
        {
        	if(obj.GetType() == typeof(DataTable))
        	{
        		return this.Equals((DataTable)obj);
        	}
        	
        	return false;
        }

		public IEnumerator<Row> GetEnumerator()
		{
			return ((IEnumerable<Row>)body).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Row>)body).GetEnumerator();
		}

	}
}






