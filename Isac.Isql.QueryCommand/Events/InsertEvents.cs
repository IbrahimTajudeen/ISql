using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using Isac.Isql.Collections;

namespace Isac.Isql.QueryCommand
{
	//Done
    public class InsertEventArgs : EventArgs 
    {         
        private string user;
        private string database;
        private Encoding charset;
        private string[] affected_columns;
        private Row row;
        private DateTime row_insert_time;
        
        public DateTime Time
        {
        	get { return row_insert_time; }
        	set { row_insert_time = value; }
        }
        
        public Row Row
        {
        	get { return row; }
        	set { row = value; }
        }
        
        public string[] ListedColumns
        {
        	get { return affected_columns; }
        	set { affected_columns = value; }
        }
        
        public Encoding CharSet
        {
        	get { return charset; }
        	set { charset = value; }
        }
        
        public string Database
        {
        	get { return database; }
        	set { database = value; }
        }
        
        public string User
        {
        	get { return user; }
        	set { user = value; }
        }
    }
    
    public class InsertingEventArgs : EventArgs
    {         
        private string user;
        private string table;
        private string database;
        private Encoding charset;
        private Column current_column;
        private Cell cell;
        private Row row;
        private DateTime row_insert_time;
        private DateTime cell_insert_time;
        
        public DateTime CellInsertTime
        {
        	get { return cell_insert_time; }
        	set { cell_insert_time = value; }
        }
        
        public DateTime RowInsertTime
        {
        	get { return row_insert_time; }
        	set { row_insert_time = value; }
        }
        
        public Row Row
        {
        	get { return row; }
        	set { row = value; }
        }
        
        public Cell CurrentCell
        {
        	get { return cell; }
        	set { cell = value; }
        }
        
        public Column CurrentColumn
        {
        	get { return current_column; }
        	set { current_column = value; }
        }
        
        public Encoding CharSet
        {
        	get { return charset; }
        	set { charset = value; }
        }
        
        public string Database
        {
        	get { return database; }
        	set { database = value; }
        }
        
        public string Table
        {
        	get { return table; }
        	set { table = value; }
        }
        
        public string User
        {
        	get { return user; }
        	set { user = value; }
        }
    }
    
    public class InsertedEventArgs : EventArgs
    {         
        private string user;
        private string table;
        private string database;
        private Encoding charset;
        private Column[] affected_columns;
        private Row row;
        
        private DateTime row_insert_time;
        
        public DateTime RowInsertTime
        {
        	get { return row_insert_time; }
        	set { row_insert_time = value; }
        }
        
        public Row Row
        {
        	get { return row; }
        	set { row = value; }
        }
        
        public Column[] InsertedColumns
        {
        	get { return affected_columns; }
        	set { affected_columns = value; }
        }
        
        public Encoding CharSet
        {
        	get { return charset; }
        	set { charset = value; }
        }
        
        public string Database
        {
        	get { return database; }
        	set { database = value; }
        }
        
        public string Table
        {
        	get { return table; }
        	set { table = value; }
        }
        
        public string User
        {
        	get { return user; }
        	set { user = value; }
        }
    
    	
    }
}




