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
    /// 
    /// </summary>
    public static class Show
    {
        private static int connection_index = -1;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static List<string> Tables()
        {
            if (ISqlConnection.CurrentConnection.ConnectionState == 1)
            {
                List<string> tableNames = new List<string>();

                using (FileStream fs = new FileStream(ISqlConnection.CurrentConnection.Database, FileMode.Open))
                {
                    using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Update))
                    {
                        tableNames.Clear();

                        foreach (var entry in zip.Entries)
                        {
                            if (entry.FullName.ToString().EndsWith(".idb"))
                            {

                                tableNames.Add(entry.FullName.ToString().Replace(".idb", ""));
                            }
                        }
                        if (tableNames.Count == 0)
                        {
                            tableNames.Add("database is Empty");
                            return tableNames;
                        }

                        return tableNames;

                    }
                }
            }
            else
            {
                throw new ISqlConnectionNotFoundException("no connection found");
            }

        }
    }
}
