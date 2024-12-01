using System;
using System.Linq;
using System.Collections.Generic;
using Isac.Isql;

namespace Isac.Isql
{
    public static class ISqlConnection
    {
        private static Dictionary<string, Connection> connection_preferences = new Dictionary<string, Connection>();
        internal static Connection CurrentConnection = new Connection("", "", "");
        private static bool hasconnect = false;
        public static void Add(string connectionName, Connection connection)
        {
            if (connection_preferences.ContainsKey(connectionName))
                throw new ISqlArguementException($"Error: there is a connection with the name: '{connectionName}' already");

            connection_preferences.Add(connectionName, connection);

            if (hasconnect == false)
                CurrentConnection = connection_preferences[connectionName];

        }

        public static void AddIfNotExist(string connectionName, Connection connection)
        {
            if (connection_preferences.ContainsKey(connectionName))
                return;

            connection_preferences.Add(connectionName, connection);

            if (hasconnect == false)
                CurrentConnection = connection_preferences[connectionName];

        }

        public static void RemoveIfExist(string connectionName)
        {
            if (connection_preferences.ContainsKey(connectionName))
                connection_preferences.Remove(connectionName);
        }

        public static void Remove(string connectionName)
        {
            connection_preferences.Remove(connectionName);
        }

        public static void Clear()
        {
            connection_preferences.Clear();
            hasconnect = false;
        }

        public static void Use(string connectionName)
        {
            if (!connection_preferences.ContainsKey(connectionName))
                throw new ISqlArguementException($"Error: there is no connection with the name: '{connectionName}'");

            CurrentConnection = connection_preferences[connectionName];
            hasconnect = true;
        }

        public static void Use(int index)
        {
            if (index < 0 || index >= connection_preferences.Count)
                throw new ISqlArguementException($"Error: index is out of boundary");

            int count = 0;
            foreach (var k in connection_preferences.Keys)
            {
                if (count == index)
                {
                    CurrentConnection = connection_preferences[k];
                    hasconnect = true;
                    break;
                }
                count++;
            }

        }

        /*public static void Use(Connection connection)
        {
        	CurrentConnection = connection;
        }*/

        internal static Connection GetConnection(string connectionName)
        {
            if (!connection_preferences.ContainsKey(connectionName))
                throw new ISqlArguementException($"Error: there is no connection with the name: '{connectionName}'");

            return connection_preferences[connectionName];
        }


    }
}
