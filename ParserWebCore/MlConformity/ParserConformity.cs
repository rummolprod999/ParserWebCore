using System.Data;
using MySql.Data.MySqlClient;
using ParserWebCore.BuilderApp;
using ParserWebCore.Connections;
using ParserWebCore.Logger;
using ParserWebCore.Parser;

namespace ParserWebCore.MlConformity
{
    public class ParserConformity : IParser
    {
        private DataTable dt = new DataTable();

        public void Parsing()
        {
            Log.Logger("Время начала работы");
            Parser();
            Log.Logger("Время окончания работы");
        }

        private void Parser()
        {
            GetListConformity();
            if (dt.Rows.Count == 0)
            {
                Log.Logger("Can not empty conformity in table");
                return;
            }

            var ml = new ConformityLearner();
            using (var connect = ConnectToDb.GetDbConnection())
            {
                connect.Open();
                ml.PredictConformity(dt.Rows, connect);
            }
        }

        private void GetListConformity()
        {
            using (var connect = ConnectToDb.GetDbConnection())
            {
                connect.Open();
                var selectPw =
                    $"SELECT id_placing_way, name FROM {Builder.Prefix}placing_way WHERE conformity = 0";
                var cmd1 = new MySqlCommand(selectPw, connect);
                cmd1.Prepare();
                var adapter1 = new MySqlDataAdapter {SelectCommand = cmd1};
                adapter1.Fill(dt);
            }
        }
    }
}