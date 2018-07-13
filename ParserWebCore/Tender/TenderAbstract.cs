﻿using System;
using System.Data;
using MySql.Data.MySqlClient;
using ParserWebCore.BuilderApp;
using ParserWebCore.Logger;

namespace ParserWebCore.Tender
{
    public abstract class TenderAbstract
    {
        protected TenderAbstract(string etpName, string etpUrl, int typeFz)
        {
            EtpName = etpName ?? throw new ArgumentNullException(nameof(etpName));
            EtpUrl = etpUrl ?? throw new ArgumentNullException(nameof(etpUrl));
            TypeFz = typeFz;
            CountTender += delegate(int d)
            {
                if (d > 0)
                    Count++;
                else
                    Log.Logger("Не удалось добавить Tender");
            };
        }

        private string PlacingWay;
        private string EtpName { get; set; }
        private string EtpUrl { get; set; }
        private int TypeFz { get; set; }
        public static int Count { get; set; }
        public event Action<int> CountTender;

        protected void AddVerNumber(MySqlConnection connect, string purchaseNumber, int typeFz)
        {
            int verNum = 1;
            string selectTenders =
                $"SELECT id_tender FROM {Builder.Prefix}tender WHERE purchase_number = @purchaseNumber AND type_fz = @typeFz ORDER BY id_tender ASC";
            MySqlCommand cmd1 = new MySqlCommand(selectTenders, connect);
            cmd1.Prepare();
            cmd1.Parameters.AddWithValue("@purchaseNumber", purchaseNumber);
            cmd1.Parameters.AddWithValue("@typeFz", typeFz);
            DataTable dt1 = new DataTable();
            MySqlDataAdapter adapter1 = new MySqlDataAdapter {SelectCommand = cmd1};
            adapter1.Fill(dt1);
            if (dt1.Rows.Count > 0)
            {
                string updateTender =
                    $"UPDATE {Builder.Prefix}tender SET num_version = @num_version WHERE id_tender = @id_tender";
                foreach (DataRow ten in dt1.Rows)
                {
                    int idTender = (int) ten["id_tender"];
                    MySqlCommand cmd2 = new MySqlCommand(updateTender, connect);
                    cmd2.Prepare();
                    cmd2.Parameters.AddWithValue("@id_tender", idTender);
                    cmd2.Parameters.AddWithValue("@num_version", verNum);
                    cmd2.ExecuteNonQuery();
                    verNum++;
                }
            }
        }

        protected void GetEtp(MySqlConnection connect, out int idEtp)
        {
            string selectEtp = $"SELECT id_etp FROM {Builder.Prefix}etp WHERE name = @name AND url = @url";
            MySqlCommand cmd7 = new MySqlCommand(selectEtp, connect);
            cmd7.Prepare();
            cmd7.Parameters.AddWithValue("@name", EtpName);
            cmd7.Parameters.AddWithValue("@url", EtpUrl);
            DataTable dt5 = new DataTable();
            MySqlDataAdapter adapter5 = new MySqlDataAdapter {SelectCommand = cmd7};
            adapter5.Fill(dt5);
            if (dt5.Rows.Count > 0)
            {
                idEtp = (int) dt5.Rows[0].ItemArray[0];
            }
            else
            {
                string insertEtp =
                    $"INSERT INTO {Builder.Prefix}etp SET name = @name, url = @url, conf=0";
                MySqlCommand cmd8 = new MySqlCommand(insertEtp, connect);
                cmd8.Prepare();
                cmd8.Parameters.AddWithValue("@name", EtpName);
                cmd8.Parameters.AddWithValue("@url", EtpUrl);
                cmd8.ExecuteNonQuery();
                idEtp = (int) cmd8.LastInsertedId;
            }
        }

        protected void GetPlacingWay(MySqlConnection connect, out int idPlacingWay)
        {
            if (!string.IsNullOrEmpty(PlacingWay))
            {
                string selectPlacingWay =
                    $"SELECT id_placing_way FROM {Builder.Prefix}placing_way WHERE name = @name";
                MySqlCommand cmd5 = new MySqlCommand(selectPlacingWay, connect);
                cmd5.Prepare();
                cmd5.Parameters.AddWithValue("@name", PlacingWay);
                DataTable dt4 = new DataTable();
                MySqlDataAdapter adapter4 = new MySqlDataAdapter {SelectCommand = cmd5};
                adapter4.Fill(dt4);
                if (dt4.Rows.Count > 0)
                {
                    idPlacingWay = (int) dt4.Rows[0].ItemArray[0];
                }
                else
                {
                    string insertPlacingWay =
                        $"INSERT INTO {Builder.Prefix}placing_way SET name= @name, conformity = @conformity";
                    MySqlCommand cmd6 = new MySqlCommand(insertPlacingWay, connect);
                    cmd6.Prepare();
                    int conformity = GetConformity(PlacingWay);
                    cmd6.Parameters.AddWithValue("@name", PlacingWay);
                    cmd6.Parameters.AddWithValue("@conformity", conformity);
                    cmd6.ExecuteNonQuery();
                    idPlacingWay = (int) cmd6.LastInsertedId;
                }
            }
            else
            {
                idPlacingWay = 0;
            }
        }

        private static int GetConformity(string conf)
        {
            string sLower = conf.ToLower();
            if (sLower.IndexOf("открыт", StringComparison.Ordinal) != -1)
            {
                return 5;
            }

            if (sLower.IndexOf("аукцион", StringComparison.Ordinal) != -1)
            {
                return 1;
            }

            if (sLower.IndexOf("котиров", StringComparison.Ordinal) != -1)
            {
                return 2;
            }

            if (sLower.IndexOf("предложен", StringComparison.Ordinal) != -1)
            {
                return 3;
            }

            if (sLower.IndexOf("единств", StringComparison.Ordinal) != -1)
            {
                return 4;
            }

            return 6;
        }
    }
}