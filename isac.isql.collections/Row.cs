using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace Isac.Isql.Collections
{
    public class Row : IEnumerable<Cell>, IComparable, IComparable<Row>, IComparer, ICloneable, IEquatable<Row>
    {
    	
        public event EventHandler<RowModifyEventArgs> RowModify;
        protected virtual void OnRowModify(RowModifyEventArgs e)
        {
        	RowModify?.Invoke(this, e);
        }
        
        public event EventHandler<CellRetriveEventArgs> CellRetrive;
        protected virtual void OnCellRetrive(CellRetriveEventArgs e)
        {
        	CellRetrive?.Invoke(this, e);
        }
        
        public event EventHandler<CellAddEventArgs> CellAdd;
        protected virtual void OnCellAdd(CellAddEventArgs e)
        {
        	CellAdd?.Invoke(this, e);
        }
        
        private Cell[] cellBlock = new Cell[0];

        public Cell this[int index]
        {
            get
            {
                if (index < 0 == false && index >= cellBlock.Length == false)
                {
                	var retrive = new CellRetriveEventArgs();
                	retrive.RetriveCell = cellBlock[index];
                	OnCellRetrive(retrive);
                	
                    return cellBlock[index];
                }
                else throw new ISqlException($"Error: index is out of range");
            }
            
            set 
            {
            	if (index > -1 && index <= cellBlock.Length -1)
                {
                	var rowModify = new RowModifyEventArgs();
                	rowModify.OldData = this;
                	rowModify.IncomingCell = value;
                	rowModify.ModifyPoint = index;
                    
                    cellBlock[index] = value;
                    
                    rowModify.NewData = this;
                    OnRowModify(rowModify);
                }
                else throw new ISqlException($"Error: index is out of range index '{index}' '{Length}'");
            	/* set the specified index to value here */;
            }
        }

        public object[] this[params int[] indexes]
        {
            get
            {
                List<object> values = new List<object>();
                foreach (var index in indexes)
                {
                    values.Add(this[index]);
                }
                return values.ToArray();
            }

            set
            {
                if (indexes.Length != value.Length)
                    throw new ISqlException($"Error: value object length and index do not match");
                int i = 0;
                foreach (int index in indexes)
                {
                    if (index < 0 || index >= Length)
                        throw new ISqlException($"Error: index out of bounds");
                    
                    this[index].Value = value[i];
                }
            }
        }
        
        public bool Equals(Row other)
        {
        	if(this.Length != other.Length)
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
        
        public override int GetHashCode()
        {
        	return cellBlock.GetHashCode();
        }
        
        public override bool Equals(object obj)
        {
        	if(obj.GetType() == typeof(Row))
        	{
        		return this.Equals((Row)obj);
        	}
        	
        	return false;
        }

        public IEnumerable<object> GetValues(params int[] indexs)
        {
        	List<object> objLi = new List<object>();
        	foreach(var i in indexs)
        	{
                if (i < 0 || i >= cellBlock.Length)
                	throw new ISqlArguementException($"Error: index is out of range");
        		
        		yield return cellBlock[i].Value;
        	}
        }

        public int CompareTo(Row other)
        {
            if (this.GetType() == other.GetType())
            {
            	Row row = other as Row;
            	
            	if(this.cellBlock.SequenceEqual(row.cellBlock.ToList<Cell>()))
            		return 0;
            	
            	else
            	{
            		var li = new List<object>();
            		var li2 = new List<object>();
            		int value = 0;
            		
            		foreach(var cl in row)
            		{
            			li.Add(this.cellBlock[value].CompareTo(cl));
            			li2.Add(cl.CompareTo(this.cellBlock[value]));
            			value++;
            		}
            		
            		int a = int.Parse(string.Join("", li));
            		int b = int.Parse(string.Join("", li2));
            		return a.CompareTo(b);
            	}
            	
            	return -1;
            }
            return -1;
        }
        
        public int CompareTo(object obj)
        {
        	Row row = obj as Row;
        	return this.CompareTo(row);
        }
        
        public int Compare(object x, object y)
        {
        	if(x.GetType() == y.GetType() && x.GetType() == typeof(Row))
        	{
        		Row a = x as Row; Row b = y as Row;
        		return a.CompareTo(b);
        	} 
        	
        	return -1;
        }
        
        private int index = -1;

        public int Index
        {
            get
            {
                return index;
            }

            internal set
            {
                index = value;
            }
        }

        internal int Length
        {
            get
            {
                return cellBlock.Length;
            }
        }

        public Row() { cellBlock = new Cell[0]; }
        
        public Row(int numberOfCell)
        {
        	//int count = 0; cellBlock = new Cell[numberOfCell];
        	foreach(var cell in Enumerable.Repeat<Cell>(new Cell(), numberOfCell).ToArray<Cell>())
            {
            	Add(cell);
            }
        }
        
        public Row(params Cell[] cells)
        {
            foreach (Cell cell in cells)
            	Add(cell);

        }
        
        public void Add(Cell cell)
        {
        	List<Cell> liCell = new List<Cell>();
        	
        	if(!(cellBlock == null || cellBlock.Length == 0))
        		liCell.AddRange(cellBlock);
            
            //for cell add event
           	var cellAddEvent = new CellAddEventArgs();
           	cellAddEvent.InputCell = cell;
           	OnCellAdd(cellAddEvent);
           	
        	liCell.Add(cell);
        	cellBlock = new Cell[liCell.Count];
        	Array.Copy(liCell.ToArray<Cell>(), cellBlock, liCell.Count);
        }
        
        public object[] GetCurrentValues()
        {
            List<object> vals = new List<object>();
            foreach (Cell cell in cellBlock)
            {
            	Value v1 = new Value(cell.Value);
            	vals.Add(v1.GetAsValue());
            }

            return vals.ToArray<object>();
        }
        
        public IEnumerable<Cell> GetCells()
        {
        	for(int i = 0; i < cellBlock.Length; i++)
            	yield return this[i];
        }
        
        public IEnumerator<Cell> GetEnumerator()
        {
            return ((IEnumerable<Cell>)cellBlock).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Cell>)cellBlock).GetEnumerator();
        }
        
        internal Cell[] GetCellArray()
        {
        	List<Cell> liCell = new List<Cell>();
        	for(int i = 0; i < cellBlock.Length; i++)
        	{
        		liCell.Add(this[i]);
        	}
        	return liCell.ToArray<Cell>();
        }
        
        public Cell FindCell(Predicate<Cell> filter)
        {
            return this.ToList<Cell>().Find(filter);
        }
        
        public Cell GetCell(int index)
        {
            return this[index];
        }
        
        public object Clone()
        {
        	return this;
        }
        
        public Row Copy()
        {
        	Cell[] old_row = this.GetCellArray();
        	Cell[] new_row = new Cell[old_row.Length];
        	Array.Copy(old_row, new_row, old_row.Length);
        	return new Row(new_row);
        }
        
        public override string ToString()
        {
        	return string.Join<Cell>("\t",cellBlock);
        }

        public string ToHTML()
        {
            string html = "";
            foreach (var cell in cellBlock)
                html += cell.ToHTML();

            return $"<tr>{html}</tr>";
        }

        /*public Cell GetCell(string column)
        {
        	try{
            return cellBlock.First(x => x.CellAddress.ToLower().StartsWith((column+":").ToLower()));
        }

        public int CompareTo(object obj)
        {
            int value = 0;
            Row row = obj as Row;
            if (row != null && row.cellBlock.Length == this.cellBlock.Length)
            {
                value = string.Join("\t",this.GetCurrentValues()).CompareTo(string.Join("\t",row.GetCurrentValues()));
            }
            else throw new ISqlException($"Error: the object '{nameof(obj)}' is not a valid Row type");

            return value;
        }
        
        
        public IEnumerator<Cell> GetEnumerator()
        {
            return ((IEnumerable<Cell>)cellBlock).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Cell>)cellBlock).GetEnumerator();
        }*/
    }
}

