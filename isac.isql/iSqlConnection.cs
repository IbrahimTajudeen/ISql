using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

namespace Isa.Isql
{
    /// <summary>
    /// 
    /// </summary>
    public static class ISqlConnection
    {

        private static string connect = "null";
        private static bool isConnected = false;
        private static string recontemp = "null";
        private static int last_index = -1;
        
        internal static List<object[]> Connections = new List<object[]>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ConnectionString"></param>
        public static void AddConnection(string ConnectionString)
        {
            if (ConnectionString.Contains("  "))
            {
                bool isTrue = false;

                string @new = "";

                for (int i = 0; i < ConnectionString.Length; i++)
                {
                    if (ConnectionString[i] == ' ' && i == 0)
                    {
                        isTrue = true;
                        continue;
                    }
                    else if (ConnectionString[i] == ' ' && isTrue == true)
                    {
                        continue;
                    }
                    else if (ConnectionString[i] == ' ' && isTrue == false)
                    {
                        isTrue = true;
                        @new += ConnectionString[i];
                    }
                    else if (ConnectionString[i] != ' ')
                    {
                        isTrue = false;
                        @new += ConnectionString[i];
                    }
                }
                ConnectionString = @new;
            }


            ConnectionString = ConnectionString.Replace(", ", ",").Replace(" ,", ",").Replace(" =","=").Replace("= ","=");
            
            ConnectionString = ConnectionString.Trim();
            string[] connectstr = ConnectionString.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            
            if (connectstr.Length > 3)
                throw new Exception("invalid connection, database not connected");

            string dataPath = "", userName = "null", passWord = "null"; isConnected = false;
            int check = 0;
            for (int i = 0; i < connectstr.Length; i++)
            {
                if (connectstr[i].Trim().ToLower().StartsWith("database="))
                {       
                    dataPath = connectstr[i].Substring(connectstr[i].IndexOf("=") + 1).Trim();
                    check++;
                }

                if (connectstr[i].Trim().ToLower().StartsWith("user="))
                {
                    userName = connectstr[i].Substring(connectstr[i].IndexOf("=") + 1).Trim();
                    check++;
                }

                if (connectstr[i].Trim().ToLower().StartsWith("pwd="))
                {
                    passWord = connectstr[i].Substring(connectstr[i].IndexOf("=") + 1).Trim();
                    check++;
                }
            }
            if (check != 3)
                throw new Exception($"Error: connection details are not complete");

            if (!dataPath.EndsWith(".isql"))
            {
                dataPath += ".isql";
            }
            check = 0;
            
            if (File.Exists(dataPath))
            {
                using (ZipArchive zipArchive = new ZipArchive(new FileStream(dataPath, FileMode.Open), ZipArchiveMode.Read))
                {
                    var mfile = zipArchive.GetEntry("database.crypto.imeta");

                    using (StreamReader read = new StreamReader(mfile.Open()))
                    {
                        while (!read.EndOfStream)
                        {
                            string[] linedb = read.ReadLine().Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);

                            if (linedb[0] == userName && linedb[1] == passWord)
                            {
                                check++;
                                Connections.Add(new object[] { dataPath, userName, passWord, true });
                            }
                        }
                        if (check == 0)
                            throw new Exception("Error: invalid userName or Password");

                    }
                }
            }
            else
                throw new Exception($"Error: database do not exists in the current path '{dataPath}'");

        }

        //this adds connection to the list
        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public static void AddConnection(string database, string username = "null", string password = "null")
        {
            string connect_str = "database="+database + ",user=" + username + ",pwd=" + password;
            AddConnection(connect_str);
        }

        //this remove a connection in a specific index
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        public static void RemoveConnection(int index)
        {
            if (index < 0 || index > Connections.Count)
                throw new Exception($"Error: index out of bounds");

            Connections.RemoveAt(index);
        }

        //this remove the last connection added to the list
        /// <summary>
        /// 
        /// </summary>
        public static void RemoveConnection()
        {
            Connections.RemoveAt(Connections.Count - 1);
        }

        /// <summary>
        /// 
        /// </summary>
        public static int Count
        {
            get
            {
                return Connections.Count;
            }
            
        }

        /*
        public static object[] Get(int index)
        {
                if (index < 0 || index >= Connections.Count)
                    throw new Exception($"Error: index is out of bound");

               return Connections[index];
        }
        */
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        public static void Reconnect(int index)
        {
            if (index < 0 || index >= Connections.Count)
                throw new Exception("Error: index is out of bounds");

            if (Connections.Count == 0)
                throw new Exception($"Error: no connection found\n connection is empty");

            last_index = index;
            object[] disConnection = Connections[index];
            bool is_Connected = IsConnected(index);
            if (is_Connected)
                Connections[index] = new object[] { disConnection[0], disConnection[1], disConnection[2], is_Connected };
            else
                throw new Exception($"Error: cannot reconnect to the database, error occured");
        }

        /// <summary>
        /// 
        /// </summary>
        public static void Reconnect()
        {
            if (last_index == -1)
                throw new Exception("cannot reconnect to an unknown database, try disconnecting");

            object[] disConnection = Connections[last_index];
            int state = ConnectionState(Connections[last_index]);
            if (state == 0 || state == 1)
                Connections[last_index] = new object[] { disConnection[0], disConnection[1], disConnection[2], true };
            else
                throw new Exception($"Error: cannot reconnect to the database, error occured");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool IsConnected(int index)
        {
            int state = ConnectionState(Connections[index]);
            if (state == 1)
                return true;

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Connection_Data"></param>
        /// <returns></returns>
        public static int ConnectionState(object[] Connection_Data)
        {
            if (!Connections.Contains(Connection_Data))
                throw new Exception($"Error: no such connection exists");

            string db = "database=" + Connection_Data[0].ToString();
            string user = "user=" + Connection_Data[1].ToString();
            string pwd = "pwd=" + Connection_Data[2].ToString();
            bool istrue = bool.Parse(Connection_Data[3].ToString());

            int check = 0;
            
                if (db.ToLower().StartsWith("database="))
                {
                    db = db.Substring(db.IndexOf("=") + 1);
                    check++;
                }

                if (user.ToLower().StartsWith("user="))
                {
                    user = user.Substring(user.IndexOf("=") + 1);
                    check++;
                }

                if (pwd.ToLower().StartsWith("pwd="))
                {
                    pwd = pwd.Substring(pwd.IndexOf("=") + 1);
                    check++;
                }
            
            if (check <= 0)
                return -2;

            if (!db.EndsWith(".isql"))
            {
                db += ".isql";
            }

            check = 0;
            if (File.Exists(db))
            {
                using (ZipArchive zipArchive = new ZipArchive(new FileStream(db, FileMode.Open), ZipArchiveMode.Read))
                {
                    var mfile = zipArchive.GetEntry("database.crypto.imeta");

                    using (StreamReader read = new StreamReader(mfile.Open()))
                    {
                        while (!read.EndOfStream)
                        {
                            string[] linedb = read.ReadLine().Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);

                            if (linedb[0] == user && linedb[1] == pwd)
                            {
                                check++;
                                if(bool.Parse(istrue.ToString()))
                                    return 1;

                                return 0;
                            }
                        }
                        if (check == 0)
                            return -1;

                    }
                }
            }
            else
                return -2;

            return -2;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        public static void Disconnect(int index)
        {
            if (index < 0 || index >= Connections.Count)
                throw new Exception("Error: index is out of bounds");
            
            if(Connections.Count == 0)
                throw new Exception($"Error: no connection found\n connection is empty");

            last_index = index;
            object[] disConnection = Connections[index];
            Connections[index] = new object[] { disConnection[0], disConnection[1], disConnection[2], false};
        }
        /// <summary>
        /// 
        /// </summary>
        public static void Disconnect()
        {
            if(last_index == -1)
                throw new Exception($"Error: no connection found, connection is empty");

            object[] disConnection = Connections[last_index];
            Connections[last_index] = new object[] { disConnection[0], disConnection[1], disConnection[2], false };
        }
    }
}
