using System;
using System.Linq;
using System.Collections.Generic;

using System.IO;
using System.IO.Compression;
using System.Text;


namespace Isac.Isql.QueryCommand
{
	public class Delete
	{
		private static Collections.DataTableSet dtSet = null;
		private static Collections.DataTable dt = null;
		private static string tb = "";
		
		public static event EventHandler<DeleteStartEventArgs> DeleteStart;
		public static event EventHandler<DeleteEndEventArgs> DeleteEnd;
		
		private static void OnDeleteStart(DeleteStartEventArgs e)
		{
			DeleteStart?.Invoke(typeof(Delete), e);
		}
		
		private static void OnDeleteEnd(DeleteEndEventArgs e)
		{
			DeleteEnd?.Invoke(typeof(Delete), e);
		}

		public static Delete From(string tablename, string where = "`true`")
		{
			Connection conn = ISqlConnection.CurrentConnection;
			Fundamentals funds = new Fundamentals();
			
			var startEve = new DeleteStartEventArgs();
			var endEve = new DeleteEndEventArgs();
			
			//For Delete Start
			startEve.user = conn.UserID;
			startEve.charSet = conn.CharSet;
			startEve.table = tablename;
			startEve.time = DateTime.Now;
			
			if (conn.ConnectionState != 1)
				throw new ISqlConnectionNotFoundException($"Error: no connection found");

			if (!File.Exists(conn.Database))
				throw new ISqlDatabaseNotFoundException($"Error: the database '{new FileInfo(conn.Database).Name}' could not be found");

			tablename = tablename.ToLower().Trim();
			if (!tablename.EndsWith(".idb"))
				tablename += ".idb";
			
			OnDeleteStart(startEve);
			
			List<string[]> head_lines = new List<string[]>();
			Encryption encrypt = new Encryption();
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

				var file = archive.GetEntry(tablename);

				if (file == null)
					throw new ISqlTableNotFoundException($"Error: '{new FileInfo(tablename).Name}' could not be found");
				
				dt = funds.BodyReader(dt, file, tbRow, conn, encrypt);

			}
			
			if (dt.Size() == 0)
				return new Delete();

			dtSet = new Collections.DataTableSet();
			dtSet.Add(dt);
			tb = tablename;

			if (Where(where))
			{
				using (FileStream fs = new FileStream(conn.Database, FileMode.Open))
				using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Update))
				{	
					funds.HeadWriter(dt, zip.GetEntry(tablename + ".head"), conn, encrypt);
				}
				
				using (FileStream fs = new FileStream(conn.Database, FileMode.Open))
				using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Update))
				{
					var tbfile = zip.GetEntry(tb);
					
					if (dt.Size() == 0)
					{
						zip.GetEntry(tablename).Delete();
						zip.CreateEntry(tablename);
					}
					else
						funds.BodyWriter(dt, tbfile, conn, encrypt);
						
				//foreach(var r in dt)
					//Console.WriteLine("Row: " + string.Join("\t\t", r.GetCurrentValues()));
				}
			}
			
			//For Delete End
			endEve.user = conn.UserID;
			endEve.charSet = conn.CharSet;
			endEve.table = tablename;
			endEve.time = DateTime.Now;
			
			OnDeleteEnd(endEve);
			return new Delete();
		}

		private static bool Where(string where = "`true`")
		{
			bool update = false;
			if (dt != null && dtSet != null)
			{
				if (string.IsNullOrEmpty(where.Trim()))
					where = "`true`";
					
				var logic = new Logistics.LogicExpressionEngine(where);
				string[] variables = logic.GetVariables();
				
				int rco = 0;

				for(; rco < dt.Size(); rco++)
				{
					object[] data_val = dt.GetDetail(rco, variables).RowDatas;
					logic.SetVariables(variables, data_val);
					logic.Solve();
					if(logic.Result)
					{
						dt.RemoveRow(rco);
						update = true;
						rco--;
					}
				}
			}
			return update;
		}
	}
}






