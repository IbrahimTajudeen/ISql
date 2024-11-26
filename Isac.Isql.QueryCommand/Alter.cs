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
	public static class Alter
	{
		private static Parser p = new Parser();
		private static Fundamentals funds = new Fundamentals();
		private static Encryption encrypt = new Encryption();
		private static Collections.DataTable dt = new Collections.DataTable();
		private static string tableName = "";
		private static Connection con;
		
		private static AlterStartEventArgs alterEvent = new AlterStartEventArgs();
		internal static AlterEndEventArgs alterEndEvent = new AlterEndEventArgs();
		
		public static event EventHandler<AlterStartEventArgs> AlterStart;
		public static event EventHandler<AlterEndEventArgs> AlterEnd;
		
		internal static void OnAlterStart(AlterStartEventArgs e)
		{
			AlterStart?.Invoke(typeof(Alter), e);
		}
		
		internal static void OnAlterEnd(AlterEndEventArgs e)
		{
			AlterEnd?.Invoke(typeof(Alter), e);
		}
		
		public static Table Table(string table)
		{
			con = ISqlConnection.CurrentConnection;

			if (con.ConnectionState != 1)
				throw new ISqlConnectionNotFoundException($"Error: no connection found");

			if (!File.Exists(con.Database))
				throw new ISqlDatabaseNotFoundException($"Error: the database '{new FileInfo(con.Database).Name}' could not be found");
				
			table = table.ToLower().Trim();
			if (!table.EndsWith(".idb"))
				table += ".idb";

			List<string[]> head_lines = new List<string[]>();
			
			using (FileStream fs = new FileStream(con.Database, FileMode.Open))
			using (ZipArchive archive = new ZipArchive(fs, ZipArchiveMode.Update))
			{
				dt = new Collections.DataTable();
				dt.Name = table.Substring(0, table.LastIndexOf("."));
				int tbRow = 0;
				
				alterEvent.user = con.UserID;
				alterEvent.charSet = con.CharSet;
				alterEvent.table = table;
				alterEvent.time = DateTime.Now;
				
				OnAlterStart(alterEvent);
				
				var headfile = archive.GetEntry(table + ".head");
				if (headfile == null)
					throw new ISqlTableNotFoundException($"Error: '{new FileInfo(table).Name}' could not be found");
				
				var coldef = new Collections.ColumnDefination();
				dt = funds.HeadReader(out tbRow, headfile, con, encrypt);

				var file = archive.GetEntry(table);

				if (file == null)
					throw new ISqlTableNotFoundException($"Error: '{new FileInfo(table).Name}' could not be found");
					
				dt = funds.BodyReader(dt, file, tbRow, con, encrypt);
				
			}
			var abstractTb = new Isac.Isql.QueryCommand.Table();
			abstractTb.con = con;
			abstractTb.dt = dt;
			abstractTb.tableName = table;
			
			return abstractTb;
		}
		
		internal static Database Database()
		{
			return new Database();//Isac.Isql.QueryCommand.Database;
		}
        
        
	}
}
