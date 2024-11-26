using System;
using System.Linq;
using System.Collections.Generic;

using System.IO;
using System.IO.Compression;
using Isac.Isql.Collections;

namespace Isac.Isql.QueryCommand
{
    public sealed class Table 
    {   
    	internal DataTable dt;
    	internal string tableName ="";
    	internal Connection con;
    	private Fundamentals funds = new Fundamentals();
    	private Encryption encrypt = new Encryption();
    	
    	
    	
    	private void Write()
    	{
    		using (FileStream fs = new FileStream(con.Database, FileMode.Open))
            {
                    using (ZipArchive zipArchive = new ZipArchive(fs, ZipArchiveMode.Update))
                    {
                    	var headf = zipArchive.GetEntry(tableName + ".head");
                    	if(headf == null)
                    		throw new ISqlTableNotFoundException($"Error: table could not be found '{new FileInfo(tableName).Name}'");
                    	
                    	var file = zipArchive.GetEntry(tableName);
                    	if(file == null)
                    		throw new ISqlTableNotFoundException($"Error: table could not be found '{new FileInfo(tableName).Name}'");
                    	
                    	funds.HeadWriter(dt, headf, con, encrypt);
                    	funds.BodyWriter(dt, file, con, encrypt);
                    }
                }
            
            Alter.alterEndEvent.user = con.UserID;
            Alter.alterEndEvent.charSet = con.CharSet;
            Alter.alterEndEvent.table = tableName;
            Alter.alterEndEvent.time = DateTime.Now;
            
            Alter.OnAlterEnd(Alter.alterEndEvent);
    	}
    	
    	internal Table() { }
    	
        public Table SetAutoIncreament(string columnName, int startPoint = 1, int increament = 1)
        {
            if (string.IsNullOrEmpty(columnName) || string.IsNullOrWhiteSpace(columnName))
                throw new ISqlException($"Error: columnName cannot be empty");
            
            startPoint--;
            //if (startPoint < 0)
            //  throw new ISqlArguementException($"Error: start_point cannot be less than ");
            
            if (increament < 1)
                throw new ISqlArguementException($"Error: increament cannot be less than 1");
            
            if (con.ConnectionState != 1)
            	throw new ISqlConnectionNotFoundException($"Error: no connection found");
            	
            	if(!dt.Head.HasColumn(columnName))
            		throw new ISqlColumnNotFoundException($"Error: the column '{columnName}' could not be found");
            		
                	dt.Head.can_modify = true;
                	Column column = dt.Head.GetColumn(columnName);
                	
                	//if(column.IsAutoIncreament == true)
                		//throw new ISqlException($"Error: the column '{column.Name}' already have it autoincreament set");
                	
                	column.Increament = new AutoIncreament(column.Name, startPoint, increament);
                	dt.Head[column.Name] = column;

            Write();
            return this;
        }
        
        public Table RenameColumn(string columnName, string newName)
        {
        	if (string.IsNullOrEmpty(columnName) || string.IsNullOrWhiteSpace(columnName))
                throw new ISqlException($"Error: column_name cannot be empty");
              
            if (string.IsNullOrEmpty(newName) || string.IsNullOrWhiteSpace(newName))
                throw new ISqlException($"Error: newName cannot be empty");
            
            if (con.ConnectionState != 1)
            	throw new ISqlConnectionNotFoundException($"Error: no connection found");
            
            if(!dt.Head.HasColumn(columnName))
            	throw new ISqlColumnNotFoundException($"Error: the column '{columnName}' could not be found");
            
            dt.Head.GetColumn(columnName).Name = newName; //Column column = 
            /*Console.WriteLine("************ALTER**************");
            Console.WriteLine(dt.Head.GetColumn(newName).Name);
            Console.WriteLine(dt.Head.GetColumn(newName).DataType);
            Console.WriteLine(dt.Head.GetColumn(newName).IsAutoIncreament);
            Console.WriteLine(dt.Head.GetColumn(newName).Increament);
            Console.WriteLine(dt.Head.GetColumn(newName).Size);
            Console.WriteLine("************ALTER**************");*/
            
            Write();
        	return this;
        }
        
        public Table AddColumn(Column column)
        {
        	if (con.ConnectionState != 1)
            	throw new ISqlConnectionNotFoundException($"Error: no connection found");
        	
        	if(!column.IsDefined)
        		throw new ISqlException($"Error: column is not well defined");
        	
        	dt.DefinedColumn(column);
        	
        	Write();
        	
        	return this;
        }
        
        public Table DropColumn(string columnName)
        {
        	if (string.IsNullOrEmpty(columnName) || string.IsNullOrWhiteSpace(columnName))
                throw new ISqlException($"Error: column_name cannot be empty");
                
            if (con.ConnectionState != 1)
            	throw new ISqlConnectionNotFoundException($"Error: no connection found");
            
            if(!dt.Head.HasColumn(columnName))
            	throw new ISqlColumnNotFoundException($"Error: column '{columnName}' could not be found");
            
            dt.RemoveColumn(columnName);
            
            Write();
                
        	return this;
        }
        
        public Table ModifyColumn(string columnName, Column modify)
        {
        	
        	if (string.IsNullOrEmpty(columnName) || string.IsNullOrWhiteSpace(columnName))
                throw new ISqlException($"Error: columnName cannot be empty");
                
            if (con.ConnectionState != 1)
            	throw new ISqlConnectionNotFoundException($"Error: no connection found");
            
            dt.Head[dt.Head.GetColumn(columnName).Name] = modify;
            
            Write();
            
        	return this;
        }
        
        public Table SetConstraint(string columnName, params Constraint[] constraints)
        {
        	if (con.ConnectionState != 1)
            	throw new ISqlConnectionNotFoundException($"Error: no connection found");
            	
        	columnName = columnName.Trim();
        	
        	if(constraints.Length == 0)
        		throw new ISqlArguementException($"Error: constraints cannot be empty");
        		
        	if (string.IsNullOrEmpty(columnName) || string.IsNullOrWhiteSpace(columnName))
                throw new ISqlException($"Error: columnName cannot be empty");
        	
        	foreach(Constraint constraint in constraints)
        	{
        		if(constraint.GetType() == typeof(AutoIncreament))
        		{
        			dt.Head[dt.Head.GetColumn(columnName).Name].Increament = (AutoIncreament)constraint;
        		}
        		else if(constraint.GetType() == typeof(Default))
        		{
        			dt.Head[dt.Head.GetColumn(columnName).Name].DefaultValue = (Default)constraint;
        		}
        		else if(constraint.GetType() == typeof(ForeignKey))
        		{
        			dt.Head[dt.Head.GetColumn(columnName).Name].ForeignKey = (ForeignKey)constraint;
        		}
        		else if(constraint.GetType() == typeof(Check))
        		{
        			dt.Head[dt.Head.GetColumn(columnName).Name].Check =  (Check)constraint;
        		}
        		else if(constraint.GetType() == typeof(PrimaryKey))
        		{
        			dt.Head[dt.Head.GetColumn(columnName).Name].PrimaryKey = (PrimaryKey)constraint;
        		}
        		else throw new ISqlArguementException($"Error: unknown constraint arguement passed");
        		
        	}
        	
        	Write();
        	
        	return this;
        }
        
    }
}