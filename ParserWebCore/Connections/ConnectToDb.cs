using MySql.Data.MySqlClient;

namespace ParserWebCore.Connections
{
    public class ConnectToDb
    {
        static ConnectToDb()
        {
            ConnectString = BuilderApp.AppBuilder.ConnectString;
        }

        private static string ConnectString { get; }

        public static MySqlConnection GetDbConnection()
        {
            var conn = new MySqlConnection(ConnectString);

            return conn;
        }
    }
}