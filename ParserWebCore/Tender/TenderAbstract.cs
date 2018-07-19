using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
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

        private string _placingWay;
        private string EtpName { get; set; }
        private string EtpUrl { get; set; }
        protected int TypeFz { get; set; }
        public static int Count { get; set; }
        public event Action<int> CountTender;

        protected void Counter(int res)
        {
            CountTender?.Invoke(res);
        }

        protected void AddVerNumber(MySqlConnection connect, string purchaseNumber, int typeFz)
        {
            var verNum = 1;
            var selectTenders =
                $"SELECT id_tender FROM {Builder.Prefix}tender WHERE purchase_number = @purchaseNumber AND type_fz = @typeFz ORDER BY id_tender ASC";
            var cmd1 = new MySqlCommand(selectTenders, connect);
            cmd1.Prepare();
            cmd1.Parameters.AddWithValue("@purchaseNumber", purchaseNumber);
            cmd1.Parameters.AddWithValue("@typeFz", typeFz);
            var dt1 = new DataTable();
            var adapter1 = new MySqlDataAdapter {SelectCommand = cmd1};
            adapter1.Fill(dt1);
            if (dt1.Rows.Count > 0)
            {
                var updateTender =
                    $"UPDATE {Builder.Prefix}tender SET num_version = @num_version WHERE id_tender = @id_tender";
                foreach (DataRow ten in dt1.Rows)
                {
                    var idTender = (int) ten["id_tender"];
                    var cmd2 = new MySqlCommand(updateTender, connect);
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
            var selectEtp = $"SELECT id_etp FROM {Builder.Prefix}etp WHERE name = @name AND url = @url";
            var cmd7 = new MySqlCommand(selectEtp, connect);
            cmd7.Prepare();
            cmd7.Parameters.AddWithValue("@name", EtpName);
            cmd7.Parameters.AddWithValue("@url", EtpUrl);
            var dt5 = new DataTable();
            var adapter5 = new MySqlDataAdapter {SelectCommand = cmd7};
            adapter5.Fill(dt5);
            if (dt5.Rows.Count > 0)
            {
                idEtp = (int) dt5.Rows[0].ItemArray[0];
            }
            else
            {
                var insertEtp =
                    $"INSERT INTO {Builder.Prefix}etp SET name = @name, url = @url, conf=0";
                var cmd8 = new MySqlCommand(insertEtp, connect);
                cmd8.Prepare();
                cmd8.Parameters.AddWithValue("@name", EtpName);
                cmd8.Parameters.AddWithValue("@url", EtpUrl);
                cmd8.ExecuteNonQuery();
                idEtp = (int) cmd8.LastInsertedId;
            }
        }

        protected void GetPlacingWay(MySqlConnection connect, out int idPlacingWay)
        {
            if (!string.IsNullOrEmpty(_placingWay))
            {
                var selectPlacingWay =
                    $"SELECT id_placing_way FROM {Builder.Prefix}placing_way WHERE name = @name";
                var cmd5 = new MySqlCommand(selectPlacingWay, connect);
                cmd5.Prepare();
                cmd5.Parameters.AddWithValue("@name", _placingWay);
                var dt4 = new DataTable();
                var adapter4 = new MySqlDataAdapter {SelectCommand = cmd5};
                adapter4.Fill(dt4);
                if (dt4.Rows.Count > 0)
                {
                    idPlacingWay = (int) dt4.Rows[0].ItemArray[0];
                }
                else
                {
                    var insertPlacingWay =
                        $"INSERT INTO {Builder.Prefix}placing_way SET name= @name, conformity = @conformity";
                    var cmd6 = new MySqlCommand(insertPlacingWay, connect);
                    cmd6.Prepare();
                    var conformity = GetConformity(_placingWay);
                    cmd6.Parameters.AddWithValue("@name", _placingWay);
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
            var sLower = conf.ToLower();
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
        
        protected void TenderKwords(MySqlConnection connect, int idTender, bool pils = false)
        {
            var resString = "";
            if (pils)
            {
                resString = "|лекарственные средства| ";
            }
            var selectPurObj =
                $"SELECT DISTINCT po.name, po.okpd_name FROM {Builder.Prefix}purchase_object AS po LEFT JOIN {Builder.Prefix}lot AS l ON l.id_lot = po.id_lot WHERE l.id_tender = @id_tender";
            var cmd1 = new MySqlCommand(selectPurObj, connect);
            cmd1.Prepare();
            cmd1.Parameters.AddWithValue("@id_tender", idTender);
            var dt = new DataTable();
            var adapter = new MySqlDataAdapter {SelectCommand = cmd1};
            adapter.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                var distrDt = dt.AsEnumerable().Distinct(DataRowComparer.Default);
                foreach (var row in distrDt)
                {
                    var name = !row.IsNull("name") ? ((string) row["name"]) : "";
                    var okpdName = (!row.IsNull("okpd_name")) ? ((string) row["okpd_name"]) : "";
                    resString += $"{name} {okpdName} ";
                }
            }

            var selectCustReq =
                $"SELECT DISTINCT cur.delivery_term FROM {Builder.Prefix}customer_requirement AS cur JOIN {Builder.Prefix}lot AS l ON l.id_lot = cur.id_lot WHERE l.id_tender = @id_tender";
            var cmd7 = new MySqlCommand(selectCustReq, connect);
            cmd7.Prepare();
            cmd7.Parameters.AddWithValue("@id_tender", idTender);
            var dt7 = new DataTable();
            var adapter7 = new MySqlDataAdapter {SelectCommand = cmd7};
            adapter7.Fill(dt7);
            if (dt7.Rows.Count > 0)
            {
                var distrDeliv = dt7.AsEnumerable().Distinct(DataRowComparer.Default);
                foreach (var row in distrDeliv)
                {
                    var delivTerm = !row.IsNull("delivery_term") ? ((string) row["delivery_term"]) : "";
                    resString += $"{delivTerm} ";
                }
            }

            var selectAttach = $"SELECT file_name FROM {Builder.Prefix}attachment WHERE id_tender = @id_tender";
            var cmd2 = new MySqlCommand(selectAttach, connect);
            cmd2.Prepare();
            cmd2.Parameters.AddWithValue("@id_tender", idTender);
            var dt2 = new DataTable();
            var adapter2 = new MySqlDataAdapter {SelectCommand = cmd2};
            adapter2.Fill(dt2);
            if (dt2.Rows.Count > 0)
            {
                var distrDt = dt2.AsEnumerable().Distinct(DataRowComparer.Default);
                foreach (var row in distrDt)
                {
                    var attName = (!row.IsNull("file_name")) ? ((string) row["file_name"]) : "";
                    resString += $" {attName}";
                }
            }

            var idOrg = 0;
            var selectPurInf =
                $"SELECT purchase_object_info, id_organizer FROM {Builder.Prefix}tender WHERE id_tender = @id_tender";
            var cmd3 = new MySqlCommand(selectPurInf, connect);
            cmd3.Prepare();
            cmd3.Parameters.AddWithValue("@id_tender", idTender);
            var dt3 = new DataTable();
            var adapter3 = new MySqlDataAdapter {SelectCommand = cmd3};
            adapter3.Fill(dt3);
            if (dt3.Rows.Count > 0)
            {
                foreach (DataRow row in dt3.Rows)
                {
                    var purOb = (!row.IsNull("purchase_object_info"))
                        ? ((string) row["purchase_object_info"])
                        : "";
                    idOrg = (!row.IsNull("id_organizer")) ? (int) row["id_organizer"] : 0;
                    resString = $"{purOb} {resString}";
                }
            }

            if (idOrg != 0)
            {
                var selectOrg =
                    $"SELECT full_name, inn FROM {Builder.Prefix}organizer WHERE id_organizer = @id_organizer";
                var cmd4 = new MySqlCommand(selectOrg, connect);
                cmd4.Prepare();
                cmd4.Parameters.AddWithValue("@id_organizer", idOrg);
                var dt4 = new DataTable();
                var adapter4 = new MySqlDataAdapter {SelectCommand = cmd4};
                adapter4.Fill(dt4);
                if (dt4.Rows.Count > 0)
                {
                    foreach (DataRow row in dt4.Rows)
                    {
                        var innOrg = (!row.IsNull("inn")) ? ((string) row["inn"]) : "";
                        var nameOrg = (!row.IsNull("full_name")) ? ((string) row["full_name"]) : "";
                        resString += $" {innOrg} {nameOrg}";
                    }
                }
            }

            var selectCustomer =
                $"SELECT DISTINCT cus.inn, cus.full_name FROM {Builder.Prefix}customer AS cus LEFT JOIN {Builder.Prefix}purchase_object AS po ON cus.id_customer = po.id_customer LEFT JOIN {Builder.Prefix}lot AS l ON l.id_lot = po.id_lot WHERE l.id_tender = @id_tender";
            var cmd6 = new MySqlCommand(selectCustomer, connect);
            cmd6.Prepare();
            cmd6.Parameters.AddWithValue("@id_tender", idTender);
            var dt5 = new DataTable();
            var adapter5 = new MySqlDataAdapter {SelectCommand = cmd6};
            adapter5.Fill(dt5);
            if (dt5.Rows.Count > 0)
            {
                var distrDt = dt5.AsEnumerable().Distinct(DataRowComparer.Default);
                foreach (var row in distrDt)
                {
                    var innC = (!row.IsNull("inn")) ? ((string) row["inn"]) : "";
                    var fullNameC = (!row.IsNull("full_name")) ? ((string) row["full_name"]) : "";
                    resString += $" {innC} {fullNameC}";
                }
            }

            resString = Regex.Replace(resString, @"\s+", " ");
            resString = resString.Trim();
            var updateTender =
                $"UPDATE {Builder.Prefix}tender SET tender_kwords = @tender_kwords WHERE id_tender = @id_tender";
            var cmd5 = new MySqlCommand(updateTender, connect);
            cmd5.Prepare();
            cmd5.Parameters.AddWithValue("@id_tender", idTender);
            cmd5.Parameters.AddWithValue("@tender_kwords", resString);
            var resT = cmd5.ExecuteNonQuery();
            if (resT != 1)
            {
                Log.Logger("Не удалось обновить tender_kwords", idTender);
            }
        }
        
        public static void GetOkpd(string okpd2Code, out int okpd2GroupCode, out string okpd2GroupLevel1Code)
        {
            if (okpd2Code.Length > 1)
            {
                var dot = okpd2Code.IndexOf(".");
                if (dot != -1)
                {
                    var okpd2GroupCodeTemp = okpd2Code.Substring(0, dot);
                    okpd2GroupCodeTemp = okpd2GroupCodeTemp.Substring(0, 2);
                    int tempOkpd2GroupCode;
                    if (!int.TryParse(okpd2GroupCodeTemp, out tempOkpd2GroupCode))
                    {
                        tempOkpd2GroupCode = 0;
                    }

                    okpd2GroupCode = tempOkpd2GroupCode;
                }
                else
                {
                    okpd2GroupCode = 0;
                }
            }
            else
            {
                okpd2GroupCode = 0;
            }

            if (okpd2Code.Length > 3)
            {
                var dot = okpd2Code.IndexOf(".");
                if (dot != -1)
                {
                    okpd2GroupLevel1Code = okpd2Code.Substring(dot + 1, 1);
                }
                else
                {
                    okpd2GroupLevel1Code = "";
                }
            }
            else
            {
                okpd2GroupLevel1Code = "";
            }
        }
    }
}