using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Isac.Isql;

namespace Isac.Isql.Collections
{
	public class DataTableSet : IEnumerable<DataTable>
	{
		private DataTable[] dataTableDef = null;


		public DataTableSet()
		{
			dataTableDef = new DataTable[0];
		}

		public int MaxSize()
		{
			int size = 0;
			if (dataTableDef != null || dataTableDef.Length > 0)
			{
				foreach (DataTable dt in dataTableDef)
				{
					if (dt.Size() > size)
						size = dt.Size();
				}
			}
			return size;
		}

		public void Add(params DataTable[] dt)
		{
			//	Console.WriteLine("Name is: "+dt[0].Name);
			if (dataTableDef != null && dataTableDef.Length > 0)
			{
				List<DataTable> newDT = new List<DataTable>();
				newDT.AddRange(dataTableDef);
				int startPoint = dataTableDef.Length;

				foreach (DataTable data in dt)
				{
					if (data.Name.Trim() != string.Empty)
					{
						if (dataTableDef != null && dataTableDef.Length > 0)
						{
							bool TB_Exsits = dataTableDef.Any(x => (x.Name.ToLower() + ":" + x.Alias.ToLower() == data.Name.ToLower() + ":" + data.Alias.ToLower()));

							//if (TB_Exsits)
							//	throw new ISqlException($"Error: the DataTable '{data.Name}' Exists in the Set");
						}
						newDT.Add(data); startPoint++;
					}
					else throw new ISqlException($"Error: '{data}'");
				}

				dataTableDef = new DataTable[newDT.Count];
				Array.Copy(newDT.ToArray(), dataTableDef, newDT.Count);
			}
			else
			{
				dataTableDef = new DataTable[0];
				for (int i = 0; i < dt.Length; i++)
				{
					if (dt[i].Head.Count > 0)
					{

						if (dataTableDef != null && dataTableDef.Length > 0)
						{
							bool colExsits = dataTableDef.Any(x => (x.Name.ToLower() + ":" + x.Alias.ToLower() == dt[i].Name.ToLower() + ":" + dt[i].Alias.ToLower() && dataTableDef.Length > 0));

							//if (colExsits)
							//	throw new ISqlException($"Error: the DataTable '{dt[i].Name}' Exists in the Set");
						}
						if (dataTableDef.Length == 0)
							this.dataTableDef = Enumerable.Repeat<DataTable>(new DataTable(), dt.Length).ToArray<DataTable>();
						dataTableDef[i] = dt[i];
					}
					else throw new ISqlException($"Error: this DataTable does not have columns defined\n'{dt[i]}'");
				}
			}
		}

		public void DeleteByName(string name)
		{
			if (dataTableDef.Any(x => x.Name.ToLower() == name.ToLower()))
			{
				List<DataTable> ndt = new List<DataTable>(dataTableDef.Length - 1);
				foreach (DataTable dt in dataTableDef)
				{
					if (dt.Name.ToLower() == name.ToLower())
						continue;
					ndt.Add(dt);
				}

				dataTableDef = new DataTable[ndt.Count];
				Array.Copy(ndt.ToArray(), dataTableDef, ndt.Count);
			}
		}

		public void DeleteByAlias(string alias)
		{
			if (dataTableDef.Any(x => x.Alias.ToLower() == alias.ToLower()))
			{
				List<DataTable> ndt = new List<DataTable>(dataTableDef.Length - 1);
				foreach (DataTable dt in dataTableDef)
				{
					if (dt.Alias.ToLower() == alias.ToLower())
						continue;

					ndt.Add(dt);
				}

				dataTableDef = new DataTable[ndt.Count];
				Array.Copy(ndt.ToArray(), dataTableDef, ndt.Count);
			}
		}

		public void DeleteByFullName(string fullname)
		{
			fullname = fullname.Replace(": ", ":").Replace(" :", ":");
			if (fullname.Contains(":") && fullname.Count(x => x == ':') == 1)
			{
				if (dataTableDef.Any(x => x.Name.ToLower() + ":" + x.Alias.ToLower() == fullname.ToLower()))
				{
					List<DataTable> ndt = new List<DataTable>(dataTableDef.Length - 1);
					foreach (DataTable dt in dataTableDef)
					{
						if (dt.Name.ToLower() + ":" + dt.Alias.ToLower() == fullname.ToLower())
							continue;
						ndt.Add(dt);
					}

					dataTableDef = new DataTable[ndt.Count];
					Array.Copy(ndt.ToArray(), dataTableDef, ndt.Count);
				}
			}
		}

		public DataTable GetByName(string name)
		{
			DataTable dt = new DataTable();
			if (dataTableDef.Any(x => x.Name.ToLower() == name.ToLower()))
			{
				dt = dataTableDef.First(x => x.Name.ToLower() == name.ToLower());
			}
			else throw new ISqlTableNotFoundException($"Error: DataTable not found in the set");

			return dt;
		}

		public DataTable GetByAlias(string alias)
		{
			DataTable dt = new DataTable();
			if (dataTableDef.Any(x => x.Alias.ToLower() == alias.ToLower()))
			{
				dt = dataTableDef.First(x => x.Alias.ToLower() == alias.ToLower());
			}
			else throw new ISqlTableNotFoundException($"Error: DataTable not found in the set");

			return dt;
		}

		public DataTable GetByFullName(string fullname)
		{
			DataTable dt = new DataTable();

			fullname = fullname.Replace(": ", ":").Replace(" :", ":");
			if (fullname.Contains(":") && fullname.Count(x => x == ':') == 1)
			{

				if (dataTableDef.Any(x => x.Name.ToLower() + ":" + x.Alias.ToLower() == fullname.ToLower()))
				{
					dt = dataTableDef.First(x => x.Name.ToLower() + ":" + x.Alias.ToLower() == fullname.ToLower());
				}
				else throw new ISqlTableNotFoundException($"Error: DataTable not found in the set");
			}
			else throw new ISqlFormatException($"Error: fullname is not in correct format\nFormat Syntax: <table_name>:<alias_name>");
			return dt;
		}

		public int Size()
		{
			return dataTableDef.Length;
		}

		public IEnumerable<string> TableNames()
		{
			foreach (DataTable dt in dataTableDef)
			{
				yield return dt.Name + ":" + dt.Alias;
			}
		}

		public IEnumerator<DataTable> GetEnumerator()
		{
			return ((IEnumerable<DataTable>)dataTableDef).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<DataTable>)dataTableDef).GetEnumerator();
		}

		internal object[] GetEachRows(int index)
		{
			//string data = "";
			List<object> data = new List<object>();
			foreach (DataTable dt in dataTableDef)
			{
				if (index >= dt.Size())
					continue;
					
				data.AddRange(dt[index].GetCurrentValues());
				//+= string.Join("\t", dt[index].GetCurrentValues()) + "\t";
			}
			return data.ToArray<object>();
		}

		public object[] GetRowValues(string[] columns, int rowIndex)
		{
			List<object> row = new List<object>();

			string[] tables_own_rel = Enumerable.Repeat("`", columns.Length).ToArray();
			string[] tables_own_col = Enumerable.Repeat("", columns.Length).ToArray();

			bool[] all_col_exists = Enumerable.Repeat(false, columns.Length).ToArray();
			if (dataTableDef != null && dataTableDef.Length > 0)
			{
				int col_exits_index = 0, ind_col_count = 0;
				foreach (string column in columns)
				{
					foreach (DataTable dt in dataTableDef)
					{
						if (dt.Count() <= rowIndex)
							continue;

						string[] data = Parser.ColumnDetector(column);

						if (data[0].ToLower() == "`" && dt.Alias == "`")
						{
							if (data[1] == "*")
							{
								row.AddRange(dt[rowIndex].GetCurrentValues());
								all_col_exists[col_exits_index] = true;
							}
							else if (dt.Head.HasColumn(data[1]))
							{
								if (tables_own_col[col_exits_index] != "")// && tables_own_rel[col_exits_index] != "`")
									throw new ISqlArguementException($"Error: ambiguity on call of column '{data[1]}' on tables '{tables_own_col[col_exits_index]}' and '{dt.RealName}'");

								if (tables_own_col[col_exits_index] == dt.RealName && tables_own_rel[col_exits_index] == "`")
								{
									row[col_exits_index] = dt.MapCell(data[1], rowIndex).Value;
									continue;
								}

								tables_own_rel[col_exits_index] = dt.Alias;
								tables_own_col[col_exits_index] = dt.RealName;
								row.Add(dt.MapCell(data[1], rowIndex).Value);
								all_col_exists[col_exits_index] = true;
							}
						}
						else if (data[0] != "`" && ((dt.Alias == "`") ? "@" + dt.Name.ToLower() == data[0].ToLower() : "@" + dt.Alias.ToLower() == data[0].ToLower()))
						{
							if (data[1] == "*")
							{
								row.AddRange(dt[rowIndex].GetCurrentValues());
								all_col_exists[col_exits_index] = true;
							}

							else if (dt.Head.HasColumn(data[1]))
							{
								if (tables_own_col[col_exits_index] != "")// && tables_own_rel[col_exits_index] != "`")
									throw new ISqlArguementException($"Error: ambiguity on call of column '{data[1]}' on tables '{tables_own_col[col_exits_index]}' and '{dt.RealName}'");

								tables_own_rel[col_exits_index] = dt.RealName;
								tables_own_col[col_exits_index] = dt.RealName;

								row.Add(dt.MapCell(data[1], rowIndex).Value);
								all_col_exists[col_exits_index] = true;
							}
						}
					}
					col_exits_index++;
				}

				string missing_columns = ""; int mis_index = 0;
				foreach (bool vals in all_col_exists)
				{
					if (vals == false)
						missing_columns += columns[mis_index] + ",";

					mis_index++;
				}

				if (missing_columns != "")
					throw new ISqlColumnNotFoundException($"Error: column(s) '{missing_columns.Remove(missing_columns.Length - 1).Replace(",", ", ")}' could not be found. Are you missing a table reference?");

			}
			else throw new ISqlException($"Error: this dataset is empty");

			//Console.WriteLine(string.Join(" ",row));
			return row.ToArray<object>();
		}

		//NOTE!: Ambiguity may occur on columns in this block
		public Details GetRowDetails(int rowIndex, string[] columns)
		{
			//	Console.WriteLine(string.Join(" ",TableNames()));
			Details det = new Details();
			if (dataTableDef != null && dataTableDef.Length > 0)
			{
				bool[] all_col_exists = Enumerable.Repeat(false, columns.Length).ToArray();
				bool stop_check = false;
				foreach (DataTable dt in dataTableDef)
				{
					det.Add(dt.GetDetail(rowIndex, columns));

					if (!stop_check)
					{
						foreach (var name in dt.Head.NameList())
						{
							int miss_col_i = 0;
							foreach (var co in columns)
							{
								string[] coldet = Parser.ColumnDetector(co);

								if (coldet[1] == "*" && (coldet[0] == "`") ? coldet[0].ToLower() == dt.Alias.ToLower() : coldet[0].ToLower() == "@" + dt.Alias.ToLower())
									all_col_exists[miss_col_i] = true;

								miss_col_i++;
							}
						}
						if (all_col_exists.All(x => x == true))
							stop_check = true;
					}
				}

				string missing_columns = ""; int mis_index = 0;
				foreach (bool vals in all_col_exists)
				{
					if (vals == false)
						missing_columns += columns[mis_index] + ",";

					mis_index++;
				}

				//	if(missing_columns != "")
				//	throw new ISqlColumnNotFoundException($"Error: '{missing_columns.Remove(missing_columns.Length - 1).Replace(",",", ")}' column(s) could not be found. are you missing a teble reference?");

			}
			return det;
		}

		//NOTE!: Ambiguity may occur on columns in this block
		public Details GetDetails(string[] columns)
		{
			//	Console.WriteLine(string.Join(" ",TableNames()));
			Details det = new Details();
			if (dataTableDef != null && dataTableDef.Length > 0)
			{
				bool[] all_col_exists = Enumerable.Repeat(false, columns.Length).ToArray();
				bool stop_check = false;
				foreach (DataTable dt in dataTableDef)
				{
					det.Add(dt.GetDetail(columns));

					if (!stop_check)
					{
						foreach (var name in dt.Head.NameList())
						{
							int miss_col_i = 0;
							foreach (var co in columns)
							{
								string[] coldet = Parser.ColumnDetector(co);

								if (coldet[1] == "*" && (coldet[0] == "`") ? coldet[0].ToLower() == dt.Alias.ToLower() : coldet[0].ToLower() == "@" + dt.Alias.ToLower())
									all_col_exists[miss_col_i] = true;

								miss_col_i++;
							}
						}
						if (all_col_exists.All(x => x == true))
							stop_check = true;
					}
				}

				string missing_columns = ""; int mis_index = 0;
				foreach (bool vals in all_col_exists)
				{
					if (vals == false)
						missing_columns += columns[mis_index] + ",";

					mis_index++;
				}

					if(missing_columns != "")
						throw new ISqlColumnNotFoundException($"Error: '{missing_columns.Remove(missing_columns.Length - 1).Replace(",",", ")}' column(s) could not be found. are you missing a teble reference?");
			}
			return det;
		}

		//NOTE!: Still working on it
		internal DataTable GetColumnValues(params string[] columns)
		{
			DataTable Ndt = new DataTable();
			
			bool[] all_col_exists = Enumerable.Repeat(false, columns.Length).ToArray();
			if (dataTableDef != null && dataTableDef.Length > 0)
			{
				int count = 0;
				foreach(var dt in dataTableDef)
				{
					foreach(string in_col in columns)
					{
						var coldet = Parser.ColumnDetector(in_col);
						
						if(dt.Head.HasColumn(coldet[1]) && (dt.Alias.ToLower() == coldet[0].ToLower() || "@" + dt.Alias.ToLower() == coldet[0].ToLower()))
						{
							Ndt.SetColumn(dt.Head.GetColumn(coldet[1]), dt[coldet[1]].ToCellArray());
							all_col_exists[count] = true;
						}
						count++;
					}
					count = 0;
				}
				
				string missing_columns = ""; int mis_index = 0;
				foreach (bool vals in all_col_exists)
				{
					if (vals == false)
						missing_columns += columns[mis_index] + ",";

					mis_index++;
				}

					if(missing_columns != "")
						throw new ISqlColumnNotFoundException($"Error: '{missing_columns.Remove(missing_columns.Length - 1).Replace(",",", ")}' column(s) could not be found. are you missing a table reference?");
			}
			else throw new ISqlException($"Error: this dataset is empty");

			return Ndt;
		}

		public void Clear()
		{
			dataTableDef = new DataTable[0];
		}

	}
}




