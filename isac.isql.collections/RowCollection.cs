using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Isac.Isql.Collections
{
	public class RowCollection : IRowCollection, IComparable<RowCollection>, ICloneable, IEquatable<RowCollection>
	{
		internal Row[] rowDef = new Row[0];
		private readonly int capacity;
		private readonly bool capacity_defined;
		
		internal void RepeatSelf(int times)
		{
			if(Size == 0 || times < 1)
				return;
			
			List<Row> rs = rowDef.ToList<Row>();
			
			for(int i = 0; i < times; i++)
			{
				rs.AddRange(rowDef);
			}
			
			rowDef = rs.ToArray<Row>();
		}
		
		
		public object Clone()
        {
        	return this;
        }
        
        public RowCollection Copy()
        {
        	Row[] old_row = this.rowDef;
        	Row[] new_row = new Row[old_row.Length];
        	Array.Copy(old_row, new_row, old_row.Length);
        	return new RowCollection(new_row);
        }
		
		public event EventHandler<RowAddEventArgs> RowAdd;
		protected virtual void OnRowAdd(RowAddEventArgs e)
		{
			RowAdd?.Invoke(this, e);
		}

		public event EventHandler<RowRemoveEventArgs> RowRemove;
		protected virtual void OnRowRemove(RowRemoveEventArgs e)
		{
			RowRemove?.Invoke(this, e);
		}

		public event EventHandler<RowGetEventArgs> RowGet;
		protected virtual void OnRowGet(RowGetEventArgs e)
		{
			RowGet?.Invoke(this, e);
		}

		public Row this[int index]
		{
			get
			{
				if (index < 0 || index >= rowDef.Length)
					throw new ISqlException($"Error: index is out of bounds of the collection.\nIndex:'{index}'\nSize: '{this.Size}'");

				var rowGt = new RowGetEventArgs();
				rowGt.RowGet = rowDef[index];
				rowGt.IndexGet = index;
				OnRowGet(rowGt);

				return rowDef[index];
			}
			set
			{
				if (index < 0 || index >= rowDef.Length)
					throw new ISqlException($"Error: index is out of bounds of the collection.\nIndex:'{index}'\nSize: '{this.Size}'");

				rowDef[index] = value;
			}
		}
		
        public bool Equals(RowCollection other)
        {
        	if(this.Size != other.Size)
        		return false;
        	
        	int id = 0;	
        	foreach(var col in other)
        	{
        		if(!col.Equals(this[id]))
        			return false; 
        			
        		id++;
        	}
        	
        	return true;
        }
        
        public override bool Equals(object obj)
        {
        	if(obj.GetType() == typeof(RowCollection))
        	{
        		return this.Equals((RowCollection)obj);
        	}
        	
        	return false;
        }
		
		public int Capacity
		{
			get { return (capacity >= Size) ? capacity : Size; }
		}

		public bool IsEmpty
		{
			get { return rowDef.Length <= 0; }
		}

		public int Size
		{
			get { return rowDef.Length; }
		}

		public bool HasRow(Row row)
		{
			object[] nrow = row.GetCurrentValues();
			foreach (var r in rowDef)
			{
				if (r.GetCurrentValues().SequenceEqual(nrow))
					return true;
			}

			return false;
		}

		public Row FindRow(Func<Row, bool> predict)
		{
			return rowDef.Where(predict).ToArray<Row>()[0];
		}

		public IEnumerable<Row> FindRows(Func<Row, bool> predict)
		{
			return rowDef.Where(predict);
		}

		public IEnumerable<Row> GetRange(int index, int count)
		{
			int icount = 0;
			for (int r = index; r < Size; r++)
			{
				yield return rowDef[r];

				if (icount == count)
					yield break;

				icount++;

			}
		}

		public void OrderBy(params int[] indices)
		{
			int currIndex = 0, maxIndex = indices.Length - 1;
			for (int i = 0; i < Size; i++)
			{
				for (int j = 0; j < Size; j++)
				{
				Retry:
					if (!(j + 1 >= Size))
					{
						Row temprow = this[j];
						Cell cell1 = temprow.GetCell(indices[currIndex]);
						Cell cell2 = this[j + 1].GetCell(indices[currIndex]);
						int val = cell1.CompareTo(cell2);
						if (val >= 1)
						{
							this[j] = this[j + 1];
							this[j + 1] = temprow;
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

		public void OrderByDes(params int[] indices)
		{
			int currIndex = 0, maxIndex = indices.Length - 1;// bool retry = false;
			for (int i = Size - 1; i >= 0; i--)
			{
				for (int j = Size - 1; j >= 0; j--)
				{
				Retry:
					if (!(j - 1 <= -1))
					{

						Row temprow = this[j];

						Cell cell1 = temprow.GetCell(indices[currIndex]);

						Cell cell2 = this[j - 1].GetCell(indices[currIndex]);

						int val = cell1.CompareTo(cell2);
						if (val >= 1)
						{
							this[j] = this[j - 1];
							this[j - 1] = temprow;
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

		internal bool can_modify = true;

		public RowCollection() { }

		public RowCollection(int capacity)
		{
			if (capacity < 0)
				throw new ISqlArguementException($"Error: capacity can not be less than zero");

			this.capacity = capacity;
			this.capacity_defined = true;
		}

		public RowCollection(params Row[] rows)
		{
			foreach (Row row in rows)
				AddRow(row);

		}

		public void AddRow(Row row)
		{
			if (!can_modify)
				throw new ISqlModifierException($"Error: this column defination is under a DataTable, it can not be modified externally");

			if (capacity_defined && Size >= capacity)
				throw new ISqlException($"Error: collection is full\nCapacity: '{capacity}'");


			List<Row> newRows = new List<Row>();
			if (rowDef.Length > 0)
				newRows.AddRange(rowDef);

			row.Index = rowDef.Length;

			var rowAdd = new RowAddEventArgs();
			rowAdd.InputRow = row;
			OnRowAdd(rowAdd);

			newRows.Add(row);

			rowDef = new Row[newRows.Count];
			Array.Copy(newRows.ToArray<Row>(), rowDef, newRows.Count);


		}

		public void AddRows(params Row[] rows)
		{
			foreach (Row row in rows)
				AddRow(row);
		}

		public void AddRow(params object[] cellValues)
		{
			Cell[] cells = new Cell[rowDef.Length]; int count = 0;
			foreach (var val in cellValues)
			{
				cells[count] = new Cell(val);
				count++;
			}

			AddRow(new Row(cells));
		}

		public void RemoveRow(int index)
		{
			//if (rowDef.Length == 0)
			//	throw new ISqlException($"Error: collection is empty");

			if (index < 0 || index >= rowDef.Length)
				throw new ISqlArguementException($"Error: index out of bounds");

			List<Row> lrows = new List<Row>();
			int rowIndex = 0;
			for (int i = 0; i < rowDef.Length; i++)
			{
				if (i == index)
				{
					var remove = new RowRemoveEventArgs();
					remove.RemovedRow = rowDef[i];
					remove.RemovedIndex = i;
					OnRowRemove(remove);

					continue;
				}

				rowDef[i].Index = rowIndex;
				lrows.Add(rowDef[i]);
				rowIndex++;
			}

			rowDef = new Row[lrows.Count];
			Array.Copy(lrows.ToArray<Row>(), rowDef, lrows.Count);
		}

		public void RemoveIndex(params int[] index)
		{
			//if (rowDef.Length == 0)
			//	throw new ISqlException($"Error: collection is empty");

			//if(index < 0 || index >= rowDef.Length)
			//	throw new ISqlArguementException($"Error: index out of bounds");

			List<Row> lrows = new List<Row>();
			int rowIndex = 0;
			for (int i = 0; i < rowDef.Length; i++)
			{
				
				if (index.Contains(i))
					continue;

				rowDef[i].Index = rowIndex;
				lrows.Add(rowDef[i]);
				rowIndex++;
			}

			rowDef = new Row[lrows.Count];
			Array.Copy(lrows.ToArray<Row>(), rowDef, lrows.Count);

		}

		public void RemoveRange(int index, int count)
		{
			//if (rowDef.Length == 0)
			//	throw new ISqlException($"Error: collection is empty");

			if (index < 0 || index >= rowDef.Length)
				throw new ISqlArguementException($"Error: index out of bounds");

			//count--;
			int total = index + count;
			if (total > rowDef.Length)
				throw new ISqlException($"Error: index or count is out of bounds of the collection");

			List<Row> newRows = new List<Row>();
			int rowIndex = index;

			int icount = 0;
			while (icount < index)
			{
				newRows.Add(rowDef[icount]);
				icount++;
			}

			//Console.WriteLine($"ICount: '{icount}'\nTotal: '{total}'\nIndex: '{index}'\nCount: '{count}'");
			icount = total;
			while (icount < Size)
			{
				rowDef[icount].Index = index;
				newRows.Add(rowDef[icount]);
				icount++; index++;
			}

			rowDef = new Row[newRows.Count];
			Array.Copy(newRows.ToArray<Row>(), rowDef, newRows.Count);

		}

		public void Clear()
		{
			rowDef = new Row[0];
		}

		public int CompareTo(RowCollection other)
		{
			if (other.GetType() == typeof(RowCollection))
			{
				RowCollection rcollection = other as RowCollection;
				if (Size == rcollection.Size)
				{
					var li = new List<object>();
					var li2 = new List<object>();
					int value = 0;

					foreach (var cl in rcollection)
					{
						li.Add(this.rowDef[value].CompareTo(cl));
						li2.Add(cl.CompareTo(this.rowDef[value]));
						value++;
					}

					int a = int.Parse(string.Join("", li));
					int b = int.Parse(string.Join("", li2));
					return a.CompareTo(b);
				}

				if (Size > rcollection.Size)
					return 1;
				if (Size < rcollection.Size)
					return -1;

			}

			return -1;
		}
		
		public int Compare(object obj)
		{
			if(obj.GetType() ==this.GetType())
			{
				RowCollection rowcoll = obj as RowCollection;
				return this.CompareTo(rowcoll);
			}
			return -1;
		}
		
		public int Compare(object x, object y)
		{
			if(x.GetType() == y.GetType() && x.GetType() == this.GetType())
			{
				RowCollection a = x as RowCollection;
				RowCollection b = y as RowCollection;
				a.CompareTo(b);
			}
			
			return -1;
		}
		
		public Row[] ToRowArray()
		{
			return rowDef;
		}

        public string ToHTML()
        {
            string html = "";
            foreach (var cell in rowDef)
                html += cell.ToHTML();

            return $"<tbody>{html}</tbody>";
        }

        public IEnumerator<Row> GetEnumerator()
		{
			return ((IEnumerable<Row>)rowDef).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Row>)rowDef).GetEnumerator();
		}
	}
}



