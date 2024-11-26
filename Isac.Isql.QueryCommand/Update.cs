using System;
using System.Linq;
using System.Collections.Generic;

using System.IO;
using System.IO.Compression;
using System.Text;

namespace Isac.Isql.QueryCommand
{
	public class Update
	{
		private static Collections.DataTableSet dtSet = null;
		private static Collections.DataTable dt = null;
		private static string tb = "";
		private static List<string[]> head_lines = new List<string[]>();
		
		public static event EventHandler<UpdateStartEventArgs> UpdateStart;
		public static event EventHandler<UpdateEndEventArgs> UpdateEnd;
		
		private static void OnUpdateStart(UpdateStartEventArgs e)
		{
			UpdateStart?.Invoke(typeof(Update), e);
		}
		
		private static void OnUpdateEnd(UpdateEndEventArgs e)
		{
			UpdateEnd?.Invoke(typeof(Update), e);
		}
		
		public static Update Table(string tablename)
		{
			Connection conn = ISqlConnection.CurrentConnection;
			Fundamentals funds = new Fundamentals();
			if (conn.ConnectionState != 1)
				throw new ISqlConnectionNotFoundException($"Error: no connection found");

			if (!File.Exists(conn.Database))
				throw new ISqlDatabaseNotFoundException($"Error: the database '{new FileInfo(conn.Database).Name}' could not be found");

			tablename = tablename.ToLower().Trim();
			if (!tablename.EndsWith(".idb"))
				tablename += ".idb";

			head_lines = new List<string[]>();
			Encryption encrypt = new Encryption();
			
			var startEve = new UpdateStartEventArgs();
			
			//For Delete Start
			startEve.user = conn.UserID;
			startEve.charSet = conn.CharSet;
			startEve.table = tablename;
			startEve.time = DateTime.Now;
			OnUpdateStart(startEve);
			
			using (FileStream fs = new FileStream(conn.Database, FileMode.Open))
			using (ZipArchive archive = new ZipArchive(fs, ZipArchiveMode.Update))
			{
				dt = new Collections.DataTable();
				dt.Name = tablename.Substring(0, tablename.LastIndexOf("."));
				int tbRow = 0;

				var headfile = archive.GetEntry(tablename + ".head");
				if (headfile == null)
					throw new ISqlTableNotFoundException($"Error: '{new FileInfo(tablename).Name}' could not be found");

				dt = funds.HeadReader(out tbRow, headfile, conn, encrypt);
				//Console.WriteLine("Rows: " + tbRow);
				var file = archive.GetEntry(tablename);

				if (file == null)
					throw new ISqlTableNotFoundException($"Error: '{new FileInfo(tablename).Name}' could not be found");
				
				dt = funds.BodyReader(dt, file, tbRow, conn, encrypt);
				
			}

			if (dt.Size() == 0)
				return new Update();
			
			/*foreach(var row in dt)
        		Console.WriteLine(string.Join(" | ", row.GetCurrentValues()));
        	Console.WriteLine();*/
        	
			dtSet = new Collections.DataTableSet();
			dtSet.Add(dt);
			tb = tablename;
			return new Update();
		}

		public void Set(Dictionary<string, object> columnvalues, string where = "`true`")
		{
			Connection conn = ISqlConnection.CurrentConnection;
			Encryption encrypt = new Encryption();
			Fundamentals funds = new Fundamentals();
			
			if (dt != null && dtSet != null)
			{
				if (string.IsNullOrEmpty(where.Trim()))
					where = "`true`";

				bool update = false;
				var logic = new Logistics.LogicExpressionEngine(where);
				int indrow = 0;

				string[] variables = logic.GetVariables();

				foreach (Collections.Row row in dt)
				{
					object[] data_val = dtSet.GetRowValues(variables, indrow);

					logic.SetVariables(variables, data_val);
					logic.Solve();
					
					if (logic.Result)
					{
						foreach (var key in columnvalues.Keys)
						{
							dt.SetCellValue(key, indrow, columnvalues[key]);
						}
						update = true;
					}
					indrow++;
				}
				
				if (update)
				{
					using (FileStream fs = new FileStream(conn.Database, FileMode.Open))
					using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Update))
					{	
						funds.HeadWriter(dt, zip.GetEntry(tb +".head"), conn, encrypt);
					}

					using (FileStream fs = new FileStream(conn.Database, FileMode.Open))
					using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Update))
					{
						var tbfile = zip.GetEntry(tb);
						
						if (dt.Size() == 0)
						{
							zip.GetEntry(tb).Delete();
							zip.CreateEntry(tb);
						}
						else
							funds.BodyWriter(dt, tbfile, conn, encrypt);
					}
				}
			}
			
			var endEve = new UpdateEndEventArgs();
			
			//For Update Start
			endEve.user = conn.UserID;
			endEve.charSet = conn.CharSet;
			endEve.table = tb;
			endEve.time = DateTime.Now;
			OnUpdateEnd(endEve);
		}
	}
}






