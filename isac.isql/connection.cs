using System;
using System.Linq;
using System.Collections.Generic;

using System.IO;
using System.IO.Compression;
using System.Timers;
using Isac.Isql.Collections;
using System.Text;

namespace Isac.Isql
{
	public sealed class Connection
	{
		private string database;
		private string userID = "main";
		private string pwd = "null";
		private string userType;
		private bool canRead;
		private bool canWrite;
		private bool canUpdate;
		private bool canDelete;
		private bool canModifyUsers;
		internal Encoding CharSet = System.Text.Encoding.Default;
		private byte[] cryptoKey;
		private byte[] cryptoIV;
		
		private Fundamentals funds = new Fundamentals();
		private Encryption encryption = new Encryption();
		
		public void Msg(string name)
		{
			
		}

		
		
		private int connectionState = -2;
		public int ConnectionState
		{
			get { return connectionState; }
			private set { connectionState = value; }
		}

		public bool IsConnected
		{
			get { return ConnectionState == 1; }
		}

		public string Database
		{
			get { return this.database; }
			set
			{
                //FileInfo fi = new FileInfo(value);
                //if(value.Contains(@"/") || value.Contains(@"\"))

                value = value.Trim().ToLower();
                string a = value.Substring(0, value.LastIndexOf(Path.GetFileNameWithoutExtension(value)));
                a += Path.GetFileNameWithoutExtension(value).ToUpper() + Path.GetExtension(value);
                database = a;
                    
			}
		}

		public string UserID
		{
			get { return this.userID; }
			set { this.userID = value; }
		}

		public string Pwd
		{
			internal get { return this.pwd; }
			set { this.pwd = value; }
		}

		#region Readonly Encapsulated fields
		public string UserType
		{
			get { return this.userType; }
		}

		public bool CanRead
		{
			get { return this.canRead; }
		}

		public bool CanWrite
		{
			get { return this.canWrite; }
		}

		public bool CanUpdate
		{
			get { return this.canUpdate; }
		}

		public bool CanDelete
		{
			get { return this.canDelete; }
		}

		public bool CanModifyUsers
		{
			get { return this.canModifyUsers; }
		}

		internal byte[] Key
		{
			get { return this.cryptoKey; }
			set { this.cryptoKey = value; }
		}

		internal byte[] IV
		{
			get { return this.cryptoIV; }
			set { this.cryptoIV = value; }
		}
		#endregion
		
		public Connection()
		{
			
		}
		
		public Connection(string database, string userID, string pwd)
		{
			Database = database;
			UserID = userID;
			Pwd = pwd;
			
			_Connect(this, EventArgs.Empty);
		}
		
		private Timer time = new Timer();
		public void Connect()
		{
			time.Interval = 1000;
			time.Elapsed += _Connect;
			time.Start();
		}
		
		public void Disconnect()
		{
			ConnectionState = 0;
			time.Elapsed -= _Connect;
			time.Stop();
		}

		private void _Connect(object sender, EventArgs args)
		{
			if (!Database.EndsWith(".isql"))
			{
				Database += ".isql";
			}

			if (!File.Exists(Database))
			{
				ConnectionState = -2;
				return;
			}
			
			#region data fetching
			var userTable = new DataTable();
			var configTable = new DataTable();

			if (File.Exists(Database))
			{
				using(FileStream fs = new FileStream(Database, FileMode.Open))
				using (ZipArchive zipArchive = new ZipArchive(fs, ZipArchiveMode.Update))
				{
					var userData = zipArchive.GetEntry("isql_user_data.crypto");
					var userDataHead = zipArchive.GetEntry("isql_user_data.crypto.head");

					var config = zipArchive.GetEntry("isql_config.crypto");
					var configHead = zipArchive.GetEntry("isql_config.crypto.head");
					
					int _tbrows = 0;
					if(userData == null || userDataHead == null || config == null || configHead == null)
						throw new ISqlException($"Error: database is corrupted or is not a valid type");
					
					//
					//isql_config
					this.Key = encryption.KEY; this.IV = encryption.IV; this.CharSet = Encoding.UTF8;
					
					configTable = funds.HeadReader(out _tbrows, configHead, this, encryption);
					
					configTable = funds.BodyReader(configTable, config, _tbrows, this, encryption);

					//"CharSet\tIV\tKey"
					string[] l = configTable[0][1].Value.ToString().Split(new string[] { @" "},StringSplitOptions.RemoveEmptyEntries);
					
					List<byte> li = new List<byte>();
					foreach (var x in l)
					li.Add(byte.Parse(x));
					
					cryptoIV = li.ToArray<byte>();

					string[] l2 = configTable[0][2].Value.ToString().Split(new string[] { @" "},StringSplitOptions.RemoveEmptyEntries);

					List<byte> li2 = new List<byte>();
					foreach (var x in l2)
						li2.Add(byte.Parse(x));

					cryptoKey = li2.ToArray<byte>();
					CharSet = Parser.GetCharEncoding((CharEncoding)Enum.Parse(typeof(CharEncoding), 
					configTable.MapCell("charset", 0).Value.ToString()));
					li.Clear(); li2.Clear();

					//ISQL_USER_DATA
					userTable = funds.HeadReader(out _tbrows, userDataHead, this, encryption);
					userTable = funds.BodyReader(userTable, userData, _tbrows, this, encryption);
				}
			}
			else
				throw new ISqlDatabaseNotFoundException($"Error: database do not exists in the current path '{new FileInfo(Database).Name}'");
			
			//"ID\tUserID\tUserType\tPwd\tAccess_To_Write\tAcces_To_Read\tAccess_To_Update\tAccess_To_Delete\tAccess_To_Modify_Users"
			var udt = userTable.Where($"UserID == `{UserID}` && Pwd == `{Pwd}`");

			if (udt.Size() != 1)
			{
				ConnectionState = -1;
				return;
			}
			
			userType = udt[0][2].Value.ToString();
			canWrite = Convert.ToBoolean(udt[0][4].Value);
			canRead = Convert.ToBoolean(udt[0][5].Value);
			canUpdate = Convert.ToBoolean(udt[0][6].Value);
			canDelete = Convert.ToBoolean(udt[0][7].Value);
			canModifyUsers = Convert.ToBoolean(udt[0][8].Value);

			ConnectionState = 1;
			#endregion
		}
	}
}





