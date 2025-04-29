#region

using System.Data;
using MySql.Data.MySqlClient;
using ParserWebCore.BuilderApp;
using ParserWebCore.Connections;
using ParserWebCore.Logger;
using ParserWebCore.Parser;

#endregion

namespace ParserWebCore.MlConformity
{
    public class ParserConformity : IParser
    {
        private readonly DataTable _dt = new DataTable();

        public void Parsing()
        {
            Log.Logger("Время начала работы");
            Parser();
            Log.Logger("Время окончания работы");
        }

        private void Parser()
        {
            GetListConformity();
            if (_dt.Rows.Count == 0)
            {
                Log.Logger("cannot empty conformity in table");
                return;
            }

            var ml = new ConformityLearner();
            using (var connect = ConnectToDb.GetDbConnection())
            {
                connect.Open();
                ml.PredictConformity(_dt.Rows, connect);
            }
        }

        private void GetListConformity()
        {
            using (var connect = ConnectToDb.GetDbConnection())
            {
                connect.Open();
                var selectPw =
                    $"SELECT id_placing_way, name FROM {AppBuilder.Prefix}placing_way WHERE conformity = 0";
                var cmd1 = new MySqlCommand(selectPw, connect);
                cmd1.Prepare();
                var adapter1 = new MySqlDataAdapter { SelectCommand = cmd1 };
                adapter1.Fill(_dt);
            }
        }
    }
}