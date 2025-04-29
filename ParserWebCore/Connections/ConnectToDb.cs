#region

using MySql.Data.MySqlClient;
using ParserWebCore.BuilderApp;

#endregion

namespace ParserWebCore.Connections
{
    public class ConnectToDb
    {
        static ConnectToDb()
        {
            ConnectString = AppBuilder.ConnectString;
        }

        private static string ConnectString { get; }

        public static MySqlConnection GetDbConnection()
        {
            var conn = new MySqlConnection(ConnectString);

            return conn;
        }
    }
}