using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

using Isac.Isql.Collections;

namespace Isac.Isql.QueryCommand
{
	public partial class Query : Process
	{
		public event EventHandler<InsertEventArgs> InsertStart;
		public event EventHandler<InsertingEventArgs> Inserting;
		public event EventHandler<InsertedEventArgs> InsertEnd;
		#region //insert into propreties
		private object[] insertValues;
		private string[] insertDataTypes;
		private string[] columnNames;

		private bool methods = false;
		
		protected virtual void OnInsert(InsertEventArgs e)
		{
			InsertStart?.Invoke(this, e);
		}
		
		protected virtual void OnInserting(InsertingEventArgs e)
		{
			Inserting?.Invoke(this, e);
		}
		
		protected virtual void OnInsertEnd(InsertedEventArgs e)
		{
			InsertEnd?.Invoke(this, e);
		}
		private InsertEventArgs insertEvent = new InsertEventArgs();
		private InsertingEventArgs insertingEvent = new InsertingEventArgs();
		private InsertedEventArgs insertedEvent = new InsertedEventArgs();
		
		//this insert to all columns in the table accordingly
		/// <summary>
		/// input series of data into a table
		/// </summary>
		/// <param name="values">values for each column in the table
		/// <b>Note:</b> the values must be arrange according to how the columns are arrange in the table</param>
		/// <returns></returns>
		public Query Insert(object[] values)
		{
			
			if (ISqlConnection.CurrentConnection.ConnectionState != 1)
				throw new ISqlConnectionNotFoundException($"Error: no connection found");
			
			ProcessStartInvoker(ISqlConnection.CurrentConnection.UserID,
								this.GetType().Name, System.Threading.Thread.CurrentThread,
								this, "Insert");
				
			if (values.Length == 0)
				throw new ISqlArguementException("no values inserted");
			
			insertEvent.User = ISqlConnection.CurrentConnection.UserID;
			insertEvent.Database = ISqlConnection.CurrentConnection.Database;
			insertEvent.CharSet = ISqlConnection.CurrentConnection.CharSet;
			
			Cell[] cell = new Cell[values.Length]; int cm = 0;
			foreach(var c in values)
			{
				cell[cm] = new Cell(c);
				cm++;
			}
			
			insertEvent.Row = new Row(cell); insertEvent.Time = DateTime.Now;
			insertEvent.ListedColumns = Enumerable.Repeat<string>("*", values.Length).ToArray<string>();
			insertValues = values;
			OnInsert(insertEvent);
			
			string type = "";
			for (int i = 0; i < insertValues.Length; i++)
				type += values[i].GetType().ToString() + '\t';

			string[] gtype = type.Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
			insertDataTypes = new string[gtype.Length];
			insertDataTypes = gtype;

			methods = false;
			usedCon = ISqlConnection.CurrentConnection;
			
			ProcessEndInvoker(usedCon.UserID,
						this.GetType().Name, System.Threading.Thread.CurrentThread,
						this, "Insert");

			return this; // new Query(fromWhichMethod, query_connection_index, Grouped_Elements, method, select_element_arr, false,insertValues,insertDataTypes,columnNames);
		}

		//this insert to specific columns
		/// <summary>
		/// input series of data into a table
		/// </summary>
		/// <param name="columns">specified name of columns to give values</param>
		/// <param name="values">values for each column listed in the column parameter
		/// Note: the values must be arrange according to how the columns are listed in the columns parameter</param>
		/// <returns>type Query class for chainning</returns>
		public Query Insert(string columns, params object[] values)
		{
			if (ISqlConnection.CurrentConnection.ConnectionState != 1)
				throw new ISqlConnectionNotFoundException($"Error: no connection found");

			columnNames = columns.Split(new string[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries);
			
			if (columnNames.Length != values.Length)
				throw new ISqlArguementException($"Error: not all columns and values have pairs");
			
			insertEvent.User = ISqlConnection.CurrentConnection.UserID;
			insertEvent.Database = ISqlConnection.CurrentConnection.Database;
			insertEvent.CharSet = ISqlConnection.CurrentConnection.CharSet;
			
			Cell[] cell = new Cell[values.Length]; int cm = 0;
			insertEvent.ListedColumns = columnNames;
			foreach(var c in values)
			{
				cell[cm] = new Cell(c);
				cm++;
			}
			
			insertEvent.Row = new Row(cell); insertEvent.Time = DateTime.Now;
			
			insertValues = values;

			string type = "";
			for (int i = 0; i < insertValues.Length; i++)
				type += values[i].GetType().ToString() + '\t';

			string[] gtype = type.Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
			insertDataTypes = gtype;

			methods = true;
			usedCon = ISqlConnection.CurrentConnection;

			return this; // new Query(fromWhichMethod, query_connection_index, Grouped_Elements, method, select_element_arr, true, insertValues, insertDataTypes, columnNames);
		}

		//multi insert
		//public Query Insert()

		//this is the into method which the table name is specified
		/// <summary>
		/// into a specific table
		/// </summary>
		/// <param name="tableName">name of the specific table to insert data(s) to</param>
		public void Into(string tableName)
		{
			if (usedCon.ConnectionState != 1)
				throw new ISqlConnectionNotFoundException($"Error: no connection found");
			ProcessStartInvoker(usedCon.UserID,
								this.GetType().Name, System.Threading.Thread.CurrentThread,
								this, "Into");
			
			insertingEvent.User = insertEvent.User;
			insertingEvent.Database = insertEvent.Database;
			insertingEvent.Table = tableName;
			insertingEvent.CharSet = usedCon.CharSet;

			if (!File.Exists(usedCon.Database))
				throw new ISqlDatabaseNotFoundException($"Error: the database '{new FileInfo(usedCon.Database).Name}' could not be found");

			if(!usedCon.CanWrite)
				throw new ISqlPermissionException($"Error: permission deny, '{usedCon.UserID}' do not have the permssion to write data");
			
			insertValues = insertEvent.Row.GetCurrentValues();
			columnNames = insertEvent.ListedColumns;
			
			Encryption encrypt = new Encryption();
			DataTable insDt = new DataTable();
			tableName = tableName.ToLower().Trim();
			p.IsValidate(tableName, "name");

			if (!tableName.EndsWith(".idb"))
				tableName += ".idb";

			insDt.Name = new FileInfo(tableName).Name;
			int _tbrows_ = 0;
			//this get the eight headings of the table 
			//from columnName - autoincrement
			using (FileStream fs = new FileStream(ISqlConnection.CurrentConnection.Database, FileMode.Open))
			using (ZipArchive zipArchive = new ZipArchive(fs, ZipArchiveMode.Read))
			{

				var entryhead = zipArchive.GetEntry(tableName + ".head");
				if (entryhead == null)
					throw new ISqlTableNotFoundException($"Error: the table '{new FileInfo(tableName).Name}' could not be found in the database '{new FileInfo(usedCon.Database).Name}'");
				
				insDt = funds.HeadReader(out _tbrows_, entryhead, usedCon, encrypt);
			}

			//reading the file for manipulations
				using (FileStream fileStream = new FileStream(ISqlConnection.CurrentConnection.Database, FileMode.Open))
				using (ZipArchive zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Update))
				{
					var thefile = zipArchive.GetEntry(tableName);
					if(thefile == null)
						throw new ISqlTableNotFoundException($"Error: the table '{new FileInfo(tableName).Name}' could not be found in the database '{new FileInfo(usedCon.Database).Name}'");
					
					insDt = funds.BodyReader(insDt, thefile, _tbrows_, usedCon, encrypt);
				}
				
			//foreach(var i in insDt)
			//	Console.WriteLine("InsertData: " + string.Join("_", i.GetCurrentValues()));
			insDt.IgnoreConstraints = false;
			if (!methods)
			{

				//checking for length of insertvalues and tablenotnull
				if (insertValues.Length != insDt.Head.Count)
					throw new ISqlException($"Error: not all columns have values");

				insDt.AddRow(insertValues);
			}
			
			else if (methods)
			{
				//checking duplicates in the selected column you want to insert to
				funds.DublicatesChecker(columnNames);

				if (insertValues.Length > insDt.Head.Count)
					throw new Exception("Error: the values arguements are too long for the table columns");

				string errorCols = "";
				Detail detail = insDt.GetDetail(insDt.Head.NameList());

				Cell[] cells = detail.ColumnDefault;

				int cell_count = 0;
				foreach (var cols in columnNames)
				{
					if (!insDt.Head.HasColumn(cols))
						errorCols += cols + ",";
						
					else if (insDt.Head.HasColumn(cols))
						cells[insDt.Head.GetColumn(cols).ColumnIndex] = new Cell(insertValues[cell_count]);
					
					cell_count++;
				}


				if (errorCols != "")
					throw new ISqlColumnNotFoundException($"Error: column(s) {errorCols.Substring(0, errorCols.Length - 2)} could not be found in the table {tableName}");

				/*if (autoName != "null")
					cells[autoIndex].Value = autoCount.ToString();*/
					
				insDt.AddRow(new Row(cells));
			}

			//Console.WriteLine("Insert Data: " + string.Join(" | ", insDt[insDt.Size() -1].GetCurrentValues()));

			//saving datas to file
			using (FileStream fs = new FileStream(usedCon.Database, FileMode.Open))
			using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Update))
			{
				var table_file = zip.GetEntry(tableName);
				
				funds.BodyWriter(insDt, table_file, usedCon, encrypt);
			}

			//saving header file
			using (FileStream fs = new FileStream(ISqlConnection.CurrentConnection.Database, FileMode.Open))
			using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Update))
			{
				var headf = zip.GetEntry(tableName + ".head");
				funds.HeadWriter(insDt, headf, usedCon, encrypt);
			}

			insDt = new DataTable();
		}
		#endregion
	}
}
