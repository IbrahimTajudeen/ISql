using System;
using System.IO;
using System.IO.Compression;

using System.Collections.Generic;

namespace Isac.Isql.QueryCommand
{
	public static class Truncate
	{
		public static event EventHandler<TruncateStartEventArgs> TruncateStart;
		public static event EventHandler<TruncateEndEventArgs> TruncateEnd;
		
		private static void OnTruncateStart(TruncateStartEventArgs e)
		{
			TruncateStart?.Invoke(typeof(Truncate), e);
		}
		
		private static void OnTruncateEnd(TruncateEndEventArgs e)
		{
			TruncateEnd?.Invoke(typeof(Truncate), e);
		}
		
		public static void Table(string tableName)
		{
			var dt = new Collections.DataTable();
			var startEve = new TruncateStartEventArgs();
			var endEve = new TruncateEndEventArgs();
			Fundamentals funds = new Fundamentals();
			Connection conn = ISqlConnection.CurrentConnection;

			if (conn.ConnectionState != 1)
				throw new ISqlConnectionNotFoundException($"Error: no connection found");

			if (!File.Exists(conn.Database))
				throw new ISqlDatabaseNotFoundException($"Error: the database '{new FileInfo(conn.Database).Name}' could not be found");

			tableName = tableName.ToLower().Trim();
			if (!tableName.EndsWith(".idb"))
				tableName += ".idb";
				
			//For Truncate Start
			startEve.user = conn.UserID;
			startEve.charSet = conn.CharSet;
			startEve.table = tableName;
			startEve.time = DateTime.Now;
			OnTruncateStart(startEve);
			
			List<string[]> head_lines = new List<string[]>();
			Encryption encrypt = new Encryption();
			using (FileStream fs = new FileStream(conn.Database, FileMode.Open))
			using (ZipArchive archive = new ZipArchive(fs, ZipArchiveMode.Update))
			{
				dt = new Collections.DataTable();
				dt.Name = tableName.Substring(0, tableName.LastIndexOf("."));
				int tbRow = 0;
				
				var headfile = archive.GetEntry(tableName + ".head");
				if (headfile == null)
					throw new ISqlTableNotFoundException($"Error: '{new FileInfo(tableName).Name}' could not be found");
				
				dt = funds.HeadReader(out tbRow, headfile, conn, encrypt);
				

				var file = archive.GetEntry(tableName);

				if (file == null)
					throw new ISqlTableNotFoundException($"Error: '{new FileInfo(tableName).Name}' could not be found");
				
				
			}		
			using (FileStream fs = new FileStream(conn.Database, FileMode.Open))
			using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Update))
			{
				var table_file = zip.GetEntry(tableName);
				
				table_file.Delete();
				zip.CreateEntry(tableName);
			}

			using (FileStream fs = new FileStream(conn.Database, FileMode.Open))
			using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Update))
			{
				var headf = zip.GetEntry(tableName + ".head");
				
				dt.Clear();
				funds.HeadWriter(dt, headf, conn, encrypt);
			}
			
			//For Truncate End
			endEve.user = conn.UserID;
			endEve.charSet = conn.CharSet;
			endEve.table = tableName;
			endEve.time = DateTime.Now;
			OnTruncateEnd(endEve);
		}
	}
}



