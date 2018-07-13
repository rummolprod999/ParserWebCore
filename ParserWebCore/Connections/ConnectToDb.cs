using MySql.Data.MySqlClient;

namespace ParserWebCore.Connections
{
    public class ConnectToDb
    {
        private static string ConnectString { get;}

        static ConnectToDb()
        {
            ConnectString = BuilderApp.Builder.ConnectString;
        }
        
        public static MySqlConnection GetDbConnection()
        {
            var conn = new MySqlConnection(ConnectString);

            return conn;
        }
    }
}