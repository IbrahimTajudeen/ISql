using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Isac.Isql.Collections
{
	public class Detail
	{
		/*
            this.Name = name;
            this.DataType = dataType;
            this.AcceptNull = acceptNull;
            this.Size = size;
            this.DefaultValue = defaultValue;
            this.IsUnique = isUnique;
            this.Check = check;
            this.ForeignKey = foreignKey;
            this.Increament = autoIncreament;
            this.IsprimaryKey = isPrimaryKey;
		*/
		
		private string name = "";
		private string alias ="";
		private string[] column_name = null;
		private Type[] data_types = null;
		private bool[] acceptNull = null;
		private int[] size = null;
		private Default[] defaultValues = new Default[1];
		private bool[] isUnique = null;
		private Check[] checks = null;
		private ForeignKey[] foreignKeys = null;
		private AutoIncreament[] autoIncreaments = null;
		private PrimaryKey[] isprimaryKey = null;
		private object[] row_datas = null;
		private Column[] columns = new Column[1];
		private Cell[] columnDefault = null;
	
		public Detail() { }

		public string Name
		{
			get { return name; }
			internal set { name = value; }
		}
		public string Alias
		{
			get {return alias;}
			internal set {alias = value;}
		}
		
		public Cell[] ColumnDefault
		{
			get { return this. columnDefault; }
			internal set { columnDefault = value; }
		}
		
		public string[] ColumnNames
		{
			get { return column_name; }
			internal set { column_name = value; }
		}
		public Type[] DataTypes
		{
			get { return data_types; }
			internal set { data_types = value; }
		}
		public bool[] AcceptNull
		{
			get { return this.acceptNull; }
			internal set { this.acceptNull = value; }
		}
		public int[] Size
		{
			get { return this.size; }
			internal set { this.size = value; }
		}
		public Default[] DefaultValues
		{
			get { return defaultValues; }
			internal set { defaultValues = value; }
		}
		public bool[] IsUnique
		{
			get { return this.isUnique; }
			internal set { this.isUnique = value; }
		}
		public Check[] Checks
		{
			get { return this.checks; }
			internal set { this.checks = value; }
		}
		public ForeignKey[] ForeignKeys
		{
			get { return foreignKeys; }
			internal set { foreignKeys = value; }
		}
		public AutoIncreament[] AutoIncreaments
		{
			get { return autoIncreaments; }
			internal set { autoIncreaments = value; }
		}
		public PrimaryKey[] PrimaryKeys
		{
			get { return isprimaryKey; }
			internal set { isprimaryKey = value; }
		}
		
		public object[] RowDatas
		{
			get { return row_datas; }
			internal set { row_datas = value; }
		}
		public Column[] Columns
		{
			get { return columns; }
			internal set { columns = value; }
		}
	}

	public class Details : IEnumerable<Detail>
	{
		private Detail[] _details = null;
		
		public Details() { _details = new Detail[0];}
		public Details(params Detail[] details){ Add(details); }
		
		public void Add(params Detail[] details)
		{
			if (_details != null)
			{
				List<Detail> newDT = new List<Detail>();
				newDT.AddRange(_details);
				int startPoint = _details.Length;

				foreach (Detail data in details)
				{
					if (data.Name != "")
					{
						if (_details != null && _details.Length > 0)
						{
							bool TB_Exsits = _details.Any(x => (x.Name.ToLower() + ":" + x.Alias.ToLower() == data.Name.ToLower() + ":" + data.Alias.ToLower()));
							//if (TB_Exsits) 
							//	throw new ISqlException($"Error: the DataTable details '{data.Name + ":" + data.Alias}' Exists in the Set");
						}
						newDT.Add(data); startPoint++;
					}
					else throw new ISqlException($"Error: this DataTable does not have name\n'{data.Name}'");
				}

				_details = new Detail[newDT.Count];
				Array.Copy(newDT.ToArray(), _details, newDT.Count);
			}
			else
			{
				this._details = Enumerable.Repeat<Detail>(new Detail(), details.Length).ToArray<Detail>();

				for (int i = 0; i < details.Length; i++)
				{
					if (details[i].Name != "")
					{
						if (_details != null && _details.Length > 0)
						{
							bool colExsits = _details.Any(x => (x.Name.ToLower() + ":" + x.Alias.ToLower() == details[i].Name.ToLower() + ":" + details[i].Alias.ToLower()));

							//if (colExsits)
							//	throw new ISqlException($"Error: the DataTable '{_details[i].Alias}' Exists in the defination");
						}

						_details[i] = details[i];
					}
					else throw new ISqlException($"Error: this DataTable does not have name\n'{details[i].Name}'");
				}
			}
		}
		
		public void Remove(string fullname)
		{
			List<Detail> nDet = new List<Detail>();
			if (_details.Any(x => x.Name.ToLower() == fullname.ToLower()))
			{
				foreach (Detail dt in _details)
				{
					if (dt.Name.ToLower() == fullname.ToLower())
						continue;
					nDet.Add(dt);
				}
				_details = new Detail[nDet.Count];
				Array.Copy(nDet.ToArray<Detail>(), _details, nDet.Count);
			}
		}

		public void RemoveAt(int index)
		{
			List<Detail> nDet = new List<Detail>();
			
				int count = 0;
				foreach (Detail dt in _details)
				{
					if (count == index)
						continue;
					nDet.Add(dt);
					count++;
				}
				_details = new Detail[nDet.Count];
				Array.Copy(nDet.ToArray<Detail>(), _details, nDet.Count);
			
		}
		
		public Cell[] GetAllData()
		{
			List<Cell> obj = new List<Cell>();
			if(_details != null || _details.Length > 0)
			{
				foreach(Detail d in _details)
				{
					foreach(var m in d.RowDatas)
						obj.Add(new Cell(m));
				}
			}
			return obj.ToArray<Cell>();
		}
		
		public Column[] GetAllColumn()
		{
			List<Column> obj = new List<Column>();
			if(_details != null || _details.Length > 0)
			{
				foreach(Detail d in _details)
					obj.AddRange(d.Columns);
			}
			return obj.ToArray<Column>();
		}
		public string[] GetAllColumnName()
		{
			List<string> obj = new List<string>();
			if(_details != null || _details.Length > 0)
			{
				foreach(Detail d in _details)
					obj.AddRange(d.ColumnNames);
			}
			return obj.ToArray<string>();
		}
		
		public void Clear()
		{
			_details = new Detail[4];
			_details = null;
		}

		public IEnumerator<Detail> GetEnumerator()
		{
			return ((IEnumerable<Detail>)_details).GetEnumerator();
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Detail>)_details).GetEnumerator();
		}
	}
}





