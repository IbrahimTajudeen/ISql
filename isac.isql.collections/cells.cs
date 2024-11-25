using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Isac.Isql.Collections
{
    public class Cell : IComparable, IComparable<Cell>, ICloneable, IEquatable<Cell>
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
        
        private string name = "no name";
        private string cellAddress;
        private int selfIndex;
        private dynamic value = "null";
        private Guid selfId;
        
        public event EventHandler<CellPropertySetEventArgs> CellPropertySet;
        protected virtual void OnCellPropertySet(CellPropertySetEventArgs e)
        {
        	CellPropertySet?.Invoke(this, e);
        }
        
        public event EventHandler<CellPropertyGetEventArgs> CellPropertyGet;
        protected virtual void OnCellPropertyRetrive(CellPropertyGetEventArgs e)
        {
        	CellPropertyGet?.Invoke(this, e);
        }
        
        public Cell(string name, object value)
        {
            Name = name;
            this.Value = value;
            selfId = Guid.NewGuid();
        }
        
        public Cell() { selfId = Guid.NewGuid(); }

        public Cell(object value)
        {
            this.Value = value;
            selfId = Guid.NewGuid();
        }
        
        internal Cell(string name, string address, int index, object value, Guid id)
        {
        	this.name = name; this.cellAddress = address; this.selfIndex = index;
        	this.value = value; this.selfId = id;
        }
        
        public string Name
        {
            get 
            {
            	var propInfo = new CellPropertyGetEventArgs();
            	propInfo.PropertyValue = name;
            	propInfo.PropertyName = "Name";
            	OnCellPropertyRetrive(propInfo);
            	return name; 
            }
            set 
            { 
            	var e = new CellPropertySetEventArgs();
            	e.OldValue = name;
            	e.NewValue = value;
            	e.PropertyName = "Name";
            	OnCellPropertySet(e);
            	name = value; 
            }
        }
        
        public Guid SelfId
        {
            get 
            { 
            	var propInfo = new CellPropertyGetEventArgs();
            	propInfo.PropertyValue = selfId;
            	propInfo.PropertyName = "SelfId";
            	OnCellPropertyRetrive(propInfo);
            	return selfId;
            }
            set 
            {
            	var e = new CellPropertySetEventArgs();
            	e.OldValue = selfId;
            	e.NewValue = value;
            	e.PropertyName = "SelfId";
            	OnCellPropertySet(e);
            	
            	selfId = value;
            }
        }
        
        public int SelfIndex
        {
            get
            {
            	var propInfo = new CellPropertyGetEventArgs();
            	propInfo.PropertyValue = selfIndex;
            	propInfo.PropertyName = "SelfIndex";
            	OnCellPropertyRetrive(propInfo);
                return selfIndex;
            }

            internal set
            {
                selfIndex = value;
            }
        }
        
        public dynamic Value
        {
            get 
            { 
            	var propInfo = new CellPropertyGetEventArgs();
            	propInfo.PropertyValue = value;
            	propInfo.PropertyName = "Value";
            	OnCellPropertyRetrive(propInfo);
            	
            	return value;
            	
            }
            
            set 
            { 
            	var e = new CellPropertySetEventArgs();
            	e.OldValue = this.value;
            	e.NewValue = value;
            	e.PropertyName = "Value";
            	OnCellPropertySet(e);
            	
            	Isac.Value val = new Value(value);
            	
            	this.value = val.Data;
            }
        }
        
        public string CellAddress
        {
            get 
            {
            	var propInfo = new CellPropertyGetEventArgs();
            	propInfo.PropertyValue = cellAddress;
            	propInfo.PropertyName = "CellAddress";
            	OnCellPropertyRetrive(propInfo);
            	
            	return cellAddress; 
            }
            internal set { cellAddress = value; }
        }
        
        public int CompareTo(Cell other)
        {
            	Cell cell = other as Cell;
                return this.Value.CompareTo(cell.Value);
            
            return -1;
        }
        
        public int CompareTo(object obj)
        {
        	if(obj.GetType() == typeof(Cell))
        	{
        		Cell c = obj as Cell;
        		
        		return this.CompareTo(c);
        	}
        	
        	return -1;
        }
        
        public int Compare(object x, object y)
        {
        	if(x.GetType() == y.GetType() && x.GetType() == typeof(Cell))
        	{
        		Cell c1 = x as Cell;
        		Cell c2 = y as Cell;
        		
        		return c1.CompareTo(c2);
        	}
        	return -1;
        }
        
        public object Clone()
        {
        	return this;
        }
        
        public Cell Copy()
        {
        	return new Cell(this.name, this.cellAddress, this.selfIndex, this.value, this.selfId);
        }
        
        public bool Equals(Cell other)
        {
        	if(this.Value.GetType() == other.Value.GetType())
        		return this.Value == other.Value;
        	
        	return false;
        }
        
        public override bool Equals(object obj)
        {
        	if(obj.GetType() == typeof(Cell))
        	{
        		return this.Equals((Cell)obj);
        	}
        	
        	else if(this.Value.GetType() == obj.GetType())
        		return this.Value == obj;
        		
        	return false;

        }
        
        public override string ToString()
        {
            return value.ToString(); 
        }
        
        public string ToHTML()
        {
        	return $"<td>{Value.ToString()}</td>";
        }
        
    }
}
