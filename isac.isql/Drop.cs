using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

namespace Isac.Isql
{
    /// <summary>
    /// To drop an instance [table, views, database, index, etc....]
    /// </summary>
    public static class Drop
    {
        private static DropStartEventArgs dropStartEvent = new DropStartEventArgs();
        private static DropFinishEventArgs dropFinishEvent = new DropFinishEventArgs();
        
        public static event EventHandler<DropStartEventArgs> DropStart;
        public static event EventHandler<DropFinishEventArgs> DropEnd;
        
        public static void OnDropStart(DropStartEventArgs e)
        {
        	DropStart?.Invoke(typeof(Drop), e);
        }
        
        public static void OnDropEnd(DropFinishEventArgs e)
        {
        	DropEnd?.Invoke(typeof(Drop), e);
        }
        
        /// <summary>
        /// to drop a table, this drops all other things associated with the table [index]
        /// </summary>
        /// <param name="tableName"></param>
        public static void Table(string tableName)
        {
        	if (ISqlConnection.CurrentConnection.ConnectionState != 1)
                throw new ISqlConnectionNotFoundException("Error: no connection found");
        	
            tableName = tableName.ToLower().Trim();
            if (!tableName.EndsWith(".idb"))
                tableName += ".idb";
                
            dropStartEvent.name = tableName;
            dropStartEvent.user = ISqlConnection.CurrentConnection.UserID;
            dropStartEvent.time = DateTime.Now;
            dropStartEvent.dropType = "TABLE";
            OnDropStart(dropStartEvent);

            using (FileStream fs = new FileStream(ISqlConnection.CurrentConnection.Database, FileMode.Open))
            {
                    using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Update))
                    {
                        int a = 0;

                    NEXT:
                        foreach (var entry in zip.Entries)
                        {

                            if (entry.FullName.ToString() == tableName)
                            {
                                var en = zip.GetEntry(tableName);
                                en.Delete();
                                a++;
                                goto NEXT;
                            }

                            if (entry.FullName.ToString() == tableName + ".head")
                            {
                                var en = zip.GetEntry(tableName + ".head");
                                en.Delete();
                                goto NEXT;
                            }

                            if (entry.FullName.ToString() == tableName + ".view")
                            {
                                var en = zip.GetEntry(tableName + "view");
                                en.Delete();
                                goto NEXT;
                            }

                            if (entry.FullName.ToString() == tableName + ".index")
                            {
                                var en = zip.GetEntry(tableName + "index");
                                en.Delete();
                                goto NEXT;
                            }

                            if (entry.FullName.ToString() == tableName + ".procedure")
                            {
                                var en = zip.GetEntry(tableName + "procedure");
                                en.Delete();
                                goto NEXT;
                            }


                        }

                        if (a == 0 || a > 1)
                            throw new ISqlTableNotFoundException($"Error: '{Path.GetFileNameWithoutExtension(tableName)}' do not exists in the database");

                        a = 0;
                    }
                }
                
            dropFinishEvent.name = tableName;
            dropFinishEvent.user = ISqlConnection.CurrentConnection.UserID;
            dropFinishEvent.dropType = "TABLE";
            dropFinishEvent.time = DateTime.Now;
            OnDropEnd(dropFinishEvent);
        }

        /// <summary>
        /// to drop a database and all datas in it
        /// </summary>
        public static void Database(string database)
        {
        	dropStartEvent.name = database;
            dropStartEvent.user = ISqlConnection.CurrentConnection.UserID;
            dropStartEvent.time = DateTime.Now;
            dropStartEvent.dropType = "DATABASE";
            OnDropStart(dropStartEvent);
            
        	if(database.Contains("/"))
            {
                string DB_Name = database.Substring(database.LastIndexOf("/"));
                DB_Name = DB_Name.ToUpper();
                database = database.Substring(0, database.LastIndexOf("/") + 1) + DB_Name.Trim();
            }
            else
                database = database.ToUpper().Trim(); 
                
            if (!database.EndsWith(".isql"))
                database += ".isql";            
            
            if (!File.Exists(database))
                throw new ISqlDatabaseNotFoundException($"database '{new FileInfo(database).Name}' do not exists");
                
            File.Delete(database);
            
            dropFinishEvent.name = database;
            dropFinishEvent.user = ISqlConnection.CurrentConnection.UserID;
            dropFinishEvent.dropType = "DATABASE";
            dropFinishEvent.time = DateTime.Now;
            OnDropEnd(dropFinishEvent);

        }
    }
}
