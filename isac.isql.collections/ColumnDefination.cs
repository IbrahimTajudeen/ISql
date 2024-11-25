using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Isac.Isql;
using Isac;
namespace Isac.Isql.Collections
{
    public class ColumnDefination : IEnumerable<Column>, IComparable, IComparable<ColumnDefination>, ICloneable, IEquatable<ColumnDefination>
    {
        public event EventHandler<ColumnRemoveEventArgs> ColumnRemove;
        protected virtual void OnColumnRemove(ColumnRemoveEventArgs e)
        {
        	ColumnRemove?.Invoke(this, e);
        }
        
        public event EventHandler<ColumnAddEventArgs> ColumnAdd;
        protected virtual void OnColumnAdd(ColumnAddEventArgs e)
        {
        	ColumnAdd?.Invoke(this, e);
        }
        
        private Column[] colDefinations = new Column[0];
        internal bool can_modify = true;
        
        public string ToHTML()
        {
            string html = "";
            foreach (var cell in colDefinations)
                html += cell.ToHTML();

            return $"<tr>{html}</tr>";
        }

        public int Count
        {
            get
            {
				//Console.WriteLine(">>> " + colDefinations.Length);
                return (colDefinations == null) ? 0 : colDefinations.Length;
            }
        }
        
        public bool IsEmpty
        {
            get
            {
                return (colDefinations.Length > 0) ? false : true;
            }
        }
        
        internal bool IsDefined;
        public Column this[int index]
        {
            get
            {
            	return this.colDefinations[index]; 
            }   
        }
        
        public Column this[string name]
        {
            get
            {
                if ((colDefinations.Length == 0 || colDefinations != null) && HasColumn(name))
                {
                    return colDefinations.First(x => x.Name.ToLower() == name.ToLower());
                }
                return  null;
            }
            
            set 
            {
            	if((colDefinations.Length == 0 || colDefinations != null) && HasColumn(name))
            	{
	            	int i = colDefinations.First(x => x.Name.ToLower() == name.ToLower()).ColumnIndex;
	            	colDefinations[i] = value;
            	}
            	else throw (HasColumn(name)) ? new ISqlException($"Error: collection is empty") : new ISqlColumnNotFoundException($"Error: such column '{name}' do not exists");
            }
        }
        
        public bool Equals(ColumnDefination other)
        {
        	if(this.Count != other.Count)
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
        	if(obj.GetType() == typeof(ColumnDefination))
        	{
        		return this.Equals((ColumnDefination)obj);
        	}
        	
        	return false;
        }

        public ColumnDefination() 
        {
        	//can_modify = true;
        	colDefinations = new Column[0];
        }
        
        public ColumnDefination(params Column[] column)
        {
            this.colDefinations = Enumerable.Repeat<Column>(new Column("_"), column.Length).ToArray<Column>();
            int count = 0;
            foreach (Column col in column)
            {
                if (col.IsDefined == true)
                {
                    if (colDefinations != null && colDefinations.Length > 0)
                    {
                        bool colExsits = colDefinations.Any(x => x.Name.ToLower() == col.Name.ToLower());
                        if (colExsits)
                            throw new ISqlException($"Error: the column '{col.Name}' Exists in the defination");
                    }
                    
                    if(colDefinations.Where(x => x.IsPrimaryKey == true).Count() == 1 && col.IsPrimaryKey == true)
                    	throw new ISqlException($"Error: a column defination can only have one PrimaryKey");
                   
                    col.ColumnIndex = count;
                    
                    var colAdd = new ColumnAddEventArgs();
                    colAdd.Column = col;
                    OnColumnAdd(colAdd);
                    
                    colDefinations[count] = col;
                    count++;
                }
                else throw new ISqlException($"Error: this column is not well define\n'{col}'");
            }
            IsDefined = true;
        }
        
		public void RemoveColumn(string columnName)
		{
			//if(!can_modify)
                //throw new ISqlModifierException($"Error: this column defination is under a DataTable, it can not be modified externally");
			
				if(!this.HasColumn(columnName))
					throw new ISqlColumnNotFoundException($"Error: column '{columnName}' could not be found");

					int indexc = GetColumn(columnName).ColumnIndex;
					List<Column> lcol = colDefinations.ToList<Column>();
					
					lcol.RemoveAt(indexc);

					int i = 0;
					foreach(Column column in lcol)
					{
						column.ColumnIndex = i;
						i++;
					}
					colDefinations = new Column[lcol.Count]; 
					Array.Copy(lcol.ToArray<Column>(), colDefinations, lcol.Count);
					
					
		} 
		
        public void AddColumn(params Column[] column)
        {
            //if (!can_modify)
                //throw new ISqlModifierException($"Error: this column defination is under a DataTable, it can not be modified externally");
            
                if (colDefinations != null || colDefinations.Length > 0)
                {
                    List<Column> newCols = new List<Column>();
                    newCols.AddRange(colDefinations);
                    int startPoint = colDefinations.Length;
                    foreach (Column data in column)
                    {
                        
                        if (!data.IsDefined == true)
                            throw new ISqlException($"Error: '{data}' is not well defined");

                            if (colDefinations != null && colDefinations.Length > 0)
                            {
                                    bool colExsits = colDefinations.Any(x => x.Name.ToLower() == data.Name.ToLower());
                                    if (colExsits)
                                        throw new ISqlException($"Error: the column '{data.Name}' Exists in the defination");
                            }
                            
                    		if(colDefinations.Where(x => x.IsPrimaryKey == true).Count() == 1 && data.IsPrimaryKey == true)
                    			throw new ISqlException($"Error: a column defination can only have one PrimaryKey");

                            data.ColumnIndex = startPoint;

                            
                            var colAdd = new ColumnAddEventArgs();
                    		colAdd.Column = data;
                    		OnColumnAdd(colAdd);
                    		
                            newCols.Add(data);
                            startPoint++;

                    }

                    colDefinations = new Column[newCols.Count];
                    Array.Copy(newCols.ToArray(), colDefinations, newCols.Count);
                }
                
                else
                {
                    this.colDefinations = Enumerable.Repeat<Column>(new Column("_"), column.Length).ToArray<Column>();
                    for (int i = 0; i < column.Length; i++)
                    {
                        if (column[i].IsDefined == true)
                            throw new ISqlException($"Error: this column is not well define\n'{column[i]}'");
                            
                            if (colDefinations != null && colDefinations.Length > 0)
                            {
                                    bool colExsits = colDefinations.Any(x => x.Name.ToLower() == column[i].Name.ToLower());
                                    if (colExsits)
                                        throw new ISqlException($"Error: the column '{column[i].Name}' Exists in the defination");
                            }
                            
                    		if(colDefinations.Where(x => x.IsPrimaryKey == true).Count() == 1 && column[i].IsPrimaryKey == true)
                    			throw new ISqlException($"Error: a column defination can only have one PrimaryKey");

                            column[i].ColumnIndex = i;
                            
                            var colAdd = new ColumnAddEventArgs();
                    		colAdd.Column = column[i];
                    		OnColumnAdd(colAdd);
                            
                            colDefinations[i] = column[i];
                    }
                }
                
                IsDefined = true;
        }
        
        public string[] NameList()
        {
            string val = "";
            if (colDefinations != null)
            {
                foreach (var item in colDefinations)
                {
                    val += item.Name + "\t";
                }
            }
            return val.Split(new string[]{"\t"}, StringSplitOptions.RemoveEmptyEntries);
        }
        
        public bool HasColumn(string name)
        {
            return (Count > 0) ? colDefinations.Any<Column>(x => x.Name.ToLower() == name.ToLower()) : false;
        }
        
        public Column GetColumn(string name)
        { 
        	if(colDefinations.Any(x => x.Name.ToLower() == name.ToLower()))
            	return colDefinations.First(x => x.Name.ToLower() == name.ToLower());
        	else throw new ISqlColumnNotFoundException($"Error: '{name}' could not be found");
        }
        
        public Column GetColumn(Predicate<Column> filter)
        {
        	return colDefinations.ToList<Column>().Find(filter);
        }
        
        public int CompareTo(ColumnDefination other)
        {
			if (other.GetType() == typeof(ColumnDefination))
			{
				ColumnDefination rcollection = other as ColumnDefination;
				if (Count == rcollection.Count)
				{
					var li = new List<object>();
					var li2 = new List<object>();
					int value = 0;

					foreach (var cl in rcollection)
					{
						li.Add(this.colDefinations[value].CompareTo(cl));
						li2.Add(cl.CompareTo(this.colDefinations[value]));
						value++;
					}

					int a = int.Parse(string.Join("", li));
					int b = int.Parse(string.Join("", li2));
					return a.CompareTo(b);
				}

				if (Count > rcollection.Count)
					return 1;
				if (Count < rcollection.Count)
					return -1;

			}

			return -1;
		}
		
		public int CompareTo(object obj)
		{
			if(obj.GetType() == this.GetType())
			{
				ColumnDefination columnsDefs = obj as ColumnDefination;
				return this.CompareTo(columnsDefs);
			}
			
			return -1;
		}
		
		public int Compare(object x, object y)
		{
			if(x.GetType() == y.GetType() && x.GetType() == this.GetType())
			{
				ColumnDefination columns1 = x as ColumnDefination;
				ColumnDefination columns2 = y as ColumnDefination;
				
				return columns1.CompareTo(columns2);
			}
			
			return -1;
		}
        
        public object Clone()
        {
        	return this;
        }
        
        public ColumnDefination Copy()
        {
        	Column[] columns = this.GetColumns().ToArray<Column>();
        	Column[] columns1 = new Column[columns.Length];
        	Array.Copy(columns, columns1, columns.Length);
        	return new ColumnDefination(columns1);
        }
        
        public IEnumerable<Column> GetColumns()
        {
            foreach (Column column in colDefinations)
            {
                yield return column;
            }
        }
        
        public IEnumerator<Column> GetEnumerator()
        {
            return ((IEnumerable<Column>)colDefinations).GetEnumerator();
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {  	
            return ((IEnumerable<Column>)colDefinations).GetEnumerator();
        }
    }
}

