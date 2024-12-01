using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using Isac.Isql.Collections;
using myCon = Isac.Isql.ISqlConnection;

namespace Isac.Isql
{
	/// <summary>
	/// create new instance of data
	/// </summary>
	public static class Create
	{

		private static Fundamentals funds = new Fundamentals();
		private static Encryption encryption = new Encryption();

		//private Events objects
		private static CreateEventArgs createEvent = new CreateEventArgs();
		private static CreatedEventArgs createdEvent = new CreatedEventArgs();

		//Create Event Handlers
		public static event EventHandler<CreateEventArgs> CreateStart;
		public static event EventHandler<CreatedEventArgs> CreateEnd;
		
		//Create Event Logic
		private static void OnCreate(CreateEventArgs e)
		{
			CreateStart?.Invoke(typeof(Create), e);
		}

		private static void OnCreated(CreatedEventArgs e)
		{
			CreateEnd?.Invoke(typeof(Create), e);
		}

		/// <summary>
		/// to create a table in a database
		/// </summary>
		/// <param name="tableName">the name will be used to identify the table in the database</param>
		/// <param name="columnsDafination">the columns to include in the table</param>
		public static void Table(string tableName, ColumnDefination columnsDafination)
		{
			if (myCon.CurrentConnection.ConnectionState != 1)
				throw new ISqlConnectionNotFoundException($"Error: no connection found.");
				
			Parser p = new Parser();
			tableName = tableName.ToLower();
			p.IsValidate(tableName, "name");

			if (columnsDafination.Count == 0)
				throw new ISqlArguementException($"Error: the table must have at least one column");

			if (!tableName.EndsWith(".idb"))
				tableName += ".idb";

			var dt = new Collections.DataTable();
			dt.Defined(columnsDafination);

			using (FileStream fs = new FileStream(ISqlConnection.CurrentConnection.Database, FileMode.Open))
			{
				using (ZipArchive zipArchive = new ZipArchive(fs, ZipArchiveMode.Update))
				{
					var see = zipArchive.GetEntry(tableName);
					if (see != null)
						throw new ISqlTableExistsException($"Error: the table '{new FileInfo(tableName).Name}' exists in the database\nDATABASE: '{new FileInfo(ISqlConnection.CurrentConnection.Database).Name}'");
					
					createEvent.user = myCon.CurrentConnection.UserID;
					createEvent.name = tableName;
					createEvent.createType = "TABLE";
					createEvent.charSet = myCon.CurrentConnection.CharSet;
					createEvent.clock = DateTime.Now;
					OnCreate(createEvent);
					
					var mfile = zipArchive.CreateEntry(tableName, CompressionLevel.Fastest);
					var headerfile = zipArchive.CreateEntry(tableName + ".head", CompressionLevel.Fastest);
					
					funds.HeadWriter(dt, headerfile, myCon.CurrentConnection, encryption);
				}
			}
			
			createdEvent.user = myCon.CurrentConnection.UserID;
			createdEvent.name = tableName;
			createdEvent.createType = "TABLE";
			createdEvent.charSet = myCon.CurrentConnection.CharSet;
			createdEvent.clock = DateTime.Now;
			OnCreated(createdEvent);
		}

		/// <summary>
		/// to create a database where table and other data can be stored
		/// </summary>
		/// <param name="name">a name use to identify the database</param>
		/// <param name="credentials">the secured connection details [username, password]</param>
		public static void Database(string name, Credentials credentials)
		{
            name = name.Trim().ToLower();
            string a = name.Substring(0, name.LastIndexOf(Path.GetFileNameWithoutExtension(name)));
            a += Path.GetFileNameWithoutExtension(name).ToUpper() + Path.GetExtension(name);
            name = a;

			if (!name.EndsWith(".isql"))
				name += ".isql";

			if (File.Exists(name))
				throw new ISqlDatabaseExistsException($"Error: the database '{name.Substring(name.LastIndexOf("/") + 1, name.LastIndexOf("."))}' exists");

			using (FileStream fileStream = new FileStream(name, FileMode.Create))
			{
				using (ZipArchive zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Update))
				{
					createEvent.user = myCon.CurrentConnection.UserID;
					createEvent.name = name;
					createEvent.createType = "DATABASE";
					createdEvent.charSet = Parser.GetCharEncoding(credentials.CharSet);
					createEvent.clock = DateTime.Now;
					OnCreate(createEvent);
					
					ZipArchiveEntry userTb = zipArchive.CreateEntry("isql_user_data.crypto", CompressionLevel.Fastest);
					ZipArchiveEntry userTbhead = zipArchive.CreateEntry("isql_user_data.crypto.head", CompressionLevel.Fastest);
					ZipArchiveEntry config = zipArchive.CreateEntry("isql_config.crypto", CompressionLevel.Fastest);
					ZipArchiveEntry confighead = zipArchive.CreateEntry("isql_config.crypto.head", CompressionLevel.Fastest);

					Encoding encode = Parser.GetCharEncoding(credentials.CharSet);
					
					
					var dt = new Collections.DataTable();
					dt.Defined(new Collections.ColumnDefination(
						new Collections.Column("ID", typeof(int), false, -1, "null", false, new Check(), null, true, false),
						new Collections.Column("UserID", typeof(Varchar), false, 100, "main", false, new Check(), null, false, false),
						new Collections.Column("UserType", typeof(Varchar), false, 13, "Standard", false, new Check(), null, false, false),
						new Collections.Column("Pwd", typeof(Varchar), false, 100, null, false, new Check(), null, false, false),
						new Collections.Column("Access_To_Write", typeof(bool), false, 5, true, false, new Check(), null, false, false),
						new Collections.Column("Access_To_Read", typeof(bool), false, 5, true, false, new Check(), null, false, false),
						new Collections.Column("Access_To_Update", typeof(bool), false, 5, true, false, new Check(), null, false, false),
						new Collections.Column("Access_To_Delete", typeof(bool), false, 5, true, false, new Check(), null, false, false),
						new Collections.Column("Access_To_Modify_Users", typeof(bool), false, 5, true, false, new Check(), null, false, false)
						));
						
					dt.AddRow(new object[] { "1", credentials.UserID, "Adminstrator", credentials.Pwd, true, true, true, true, true });

					var con = new Connection("null", "Code", "CODE");
					con.IV = credentials.IV; con.Key = credentials.Key; con.CharSet = Parser.GetCharEncoding(credentials.CharSet);
					
					funds.HeadWriter(dt, userTbhead, con, encryption);
					
					funds.BodyWriter(dt, userTb, con.Key, con.IV, encryption, encode);

					dt.Clear();
					
					dt.Defined(new Collections.ColumnDefination(
						new Collections.Column("CharSet", typeof(Varchar), false, -1, "Default", false, new Check(), null, false, false),
						new Collections.Column("IV", typeof(Varchar), false, -1, null, false, new Check(), null, false, false),
						new Collections.Column("Key", typeof(Varchar), false, -1, null, false, new Check(), null, false, false)
						));
					dt.AddRow(new object[] { credentials.CharSet, string.Join(" ", credentials.IV), string.Join(" ", credentials.Key) });

					con.Key = encryption.KEY; con.IV = encryption.IV; con.CharSet = Encoding.UTF8;
					
					funds.HeadWriter(dt, confighead, con, encryption);
					
					funds.BodyWriter(dt, config, con, encryption);
					
					
				}
			}
			
			createdEvent.user = myCon.CurrentConnection.UserID;
			createdEvent.name = name;
			createdEvent.createType = "DATABASE";
			createdEvent.charSet = Parser.GetCharEncoding(credentials.CharSet);
			createdEvent.clock = DateTime.Now;
			OnCreated(createdEvent);
		}
	}
}




