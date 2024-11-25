using Isac.Isql;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Isac
{
    public static class ISql
    {
        public static bool TableExists(string table)
        {

            var con = ISqlConnection.CurrentConnection;
            if (con.ConnectionState != 1)
                return false;
            

            using (FileStream fs = new FileStream(con.Database, FileMode.Open))
            using (ZipArchive zipArchive = new ZipArchive(fs, ZipArchiveMode.Update))
            {
                var userData = zipArchive.GetEntry("isql_user_data.crypto");
                var userDataHead = zipArchive.GetEntry("isql_user_data.crypto.head");

                var config = zipArchive.GetEntry("isql_config.crypto");
                var configHead = zipArchive.GetEntry("isql_config.crypto.head");

                int _tbrows = 0;
                if (userData == null || userDataHead == null || config == null || configHead == null)
                    return false;

                if (zipArchive.GetEntry(table.ToLower() + ".idb") != null)
                    return true;
            }

            return false;
        }
    }
}