using System.Data;
using System.Threading;
using MySql.Data.MySqlClient;
using ParserWebCore.Connections;
using ParserWebCore.TenderType;

namespace ParserWebCore.Tender
{
    public class TenderJ : TenderAbstract, ITender
    {
        private readonly TypeJ _tn;
        private readonly string _typeFz;

        public TenderJ(TypeJ tn) : base("", "", 0)
        {
            _tn = tn;
        }

        public void ParsingTender()
        {
            using (var connect = ConnectToDb.GetDbConnection())
            {
                connect.Open();
                var selectTend =
                    $"SELECT id FROM event_log WHERE event = @event AND date_time = @date_time AND notification_number = @notification_number";
                var cmd = new MySqlCommand(selectTend, connect);
                cmd.Prepare();
                cmd.Parameters.AddWithValue("@notification_number", _tn.NotificationNumber);
                cmd.Parameters.AddWithValue("@date_time", _tn.DateTime);
                cmd.Parameters.AddWithValue("@event", _tn.Event);
                var dt = new DataTable();
                var adapter = new MySqlDataAdapter { SelectCommand = cmd };
                adapter.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    //Log.Logger("This tender is exist in base", PurNum);
                    return;
                }

                var ev =
                    $"INSERT INTO event_log SET event = @event, date_time = @date_time, notification_number = @notification_number, type_fz = @type_fz, time_zone = @time_zone";
                var cmd4 = new MySqlCommand(ev, connect);
                cmd4.Prepare();
                cmd4.Parameters.AddWithValue("@notification_number", _tn.NotificationNumber);
                cmd4.Parameters.AddWithValue("@event", _tn.Event);
                cmd4.Parameters.AddWithValue("@date_time", _tn.DateTime);
                cmd4.Parameters.AddWithValue("@type_fz", _tn.TypeFz);
                cmd4.Parameters.AddWithValue("@time_zone", _tn.TimeZone);
                cmd4.ExecuteNonQuery();
                Counter(1, false);
            }

            Thread.Sleep(2000);
        }
    }
}