using System;
using System.Data;
using HtmlAgilityPack;
using MySql.Data.MySqlClient;
using ParserWebCore.BuilderApp;
using ParserWebCore.Connections;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.TenderType;

namespace ParserWebCore.Tender
{
    public class TenderB2BWeb : TenderAbstract, ITender
    {
        private readonly TypeB2B _tn;

        public TenderB2BWeb(string etpName, string etpUrl, int typeFz, TypeB2B tn) : base(etpName, etpUrl,
            typeFz)
        {
            _tn = tn;
            PlacingWay = tn.PwName;
        }

        public void ParsingTender()
        {
            using (var connect = ConnectToDb.GetDbConnection())
            {
                var dateUpd = DateTime.Now;
                connect.Open();
                if (TenderExist(connect)) return;
                var s = DownloadString.DownLUserAgent(_tn.Href);
                if (string.IsNullOrEmpty(s))
                {
                    Log.Logger("Empty string in ParsingTender()", _tn.Href);
                    return;
                }

                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(s);
                var navigator = (HtmlNodeNavigator) htmlDoc.CreateNavigator();
                UpdateCancelStatus(connect, dateUpd, out var updated, out var cancelStatus);
                var printForm = _tn.Href;
                AddOrganizer(connect, navigator, out var organiserId);
                AddCustomer(connect, out var customerId);
                GetEtp(connect, out var idEtp);
                GetPlacingWay(connect, out var idPlacingWay);
            }
        }

        private void AddCustomer(MySqlConnection connect, out int customerId)
        {
            customerId = 0;
            if (!string.IsNullOrEmpty(_tn.OrgName))
            {
                var selectCustomer =
                    $"SELECT id_customer FROM {Builder.Prefix}customer WHERE full_name = @full_name";
                var cmd13 = new MySqlCommand(selectCustomer, connect);
                cmd13.Prepare();
                cmd13.Parameters.AddWithValue("@full_name", _tn.OrgName);
                var reader7 = cmd13.ExecuteReader();
                if (reader7.HasRows)
                {
                    reader7.Read();
                    customerId = (int) reader7["id_customer"];
                    reader7.Close();
                }
                else
                {
                    reader7.Close();
                    var insertCustomer =
                        $"INSERT INTO {Builder.Prefix}customer SET reg_num = @reg_num, full_name = @full_name, is223=1";
                    var cmd14 = new MySqlCommand(insertCustomer, connect);
                    cmd14.Prepare();
                    var customerRegNumber = Guid.NewGuid().ToString();
                    cmd14.Parameters.AddWithValue("@reg_num", customerRegNumber);
                    cmd14.Parameters.AddWithValue("@full_name", _tn.OrgName);
                    cmd14.ExecuteNonQuery();
                    customerId = (int) cmd14.LastInsertedId;
                }
            }
        }

        private void AddOrganizer(MySqlConnection connect, HtmlNodeNavigator navigator, out int organiserId)
        {
            organiserId = 0;
            if (!string.IsNullOrEmpty(_tn.OrgName))
            {
                var selectOrg =
                    $"SELECT id_organizer FROM {Builder.Prefix}organizer WHERE full_name = @full_name";
                var cmd3 = new MySqlCommand(selectOrg, connect);
                cmd3.Prepare();
                cmd3.Parameters.AddWithValue("@full_name", _tn.OrgName);
                var dt3 = new DataTable();
                var adapter3 = new MySqlDataAdapter {SelectCommand = cmd3};
                adapter3.Fill(dt3);
                if (dt3.Rows.Count > 0)
                {
                    organiserId = (int) dt3.Rows[0].ItemArray[0];
                }
                else
                {
                    var phone = navigator.SelectSingleNode(
                                        "//td[contains(., 'Номер контактного телефона заказчика')]/following-sibling::td")
                                    ?.Value?.Trim() ??
                                "";
                    var email = navigator.SelectSingleNode(
                                        "//td[contains(., 'Контактный адрес e-mail:')]/following-sibling::td")
                                    ?.Value
                                    ?.Trim() ??
                                "";
                    var contactPerson = navigator.SelectSingleNode(
                                                "//td[contains(., 'Контактное лицо:') or contains(., 'Ответственное лицо:')]/following-sibling::td")
                                            ?.Value?.Trim() ??
                                        "";
                    var postAddr = navigator.SelectSingleNode(
                                           "//td[contains(., 'Почтовый адрес заказчика:')]/following-sibling::td")
                                       ?.Value?.Trim() ??
                                   "";
                    var address = navigator.SelectSingleNode(
                            "//td[contains(., 'Местонахождение заказчика:')]/following-sibling::td")
                        ?.Value?.Trim() ?? "";
                    var addOrganizer =
                        $"INSERT INTO {Builder.Prefix}organizer SET full_name = @full_name, contact_phone = @contact_phone, contact_person = @contact_person, contact_email = @contact_email, post_address = @post_addres, fact_address = @fact_address";
                    var cmd4 = new MySqlCommand(addOrganizer, connect);
                    cmd4.Prepare();
                    cmd4.Parameters.AddWithValue("@full_name", _tn.OrgName);
                    cmd4.Parameters.AddWithValue("@contact_phone", phone);
                    cmd4.Parameters.AddWithValue("@contact_person", contactPerson);
                    cmd4.Parameters.AddWithValue("@contact_email", email);
                    cmd4.Parameters.AddWithValue("@post_address", postAddr);
                    cmd4.Parameters.AddWithValue("@fact_address", address);
                    cmd4.ExecuteNonQuery();
                    organiserId = (int) cmd4.LastInsertedId;
                }
            }
        }

        private void UpdateCancelStatus(MySqlConnection connect, DateTime dateUpd, out bool updated,
            out int cancelStatus)
        {
            updated = false;
            cancelStatus = 0;
            var selectDateT =
                $"SELECT id_tender, date_version, cancel FROM {Builder.Prefix}tender WHERE purchase_number = @purchase_number AND type_fz = @type_fz";
            var cmd2 = new MySqlCommand(selectDateT, connect);
            cmd2.Prepare();
            cmd2.Parameters.AddWithValue("@purchase_number", _tn.PurNum);
            cmd2.Parameters.AddWithValue("@type_fz", TypeFz);
            var adapter2 = new MySqlDataAdapter {SelectCommand = cmd2};
            var dt2 = new DataTable();
            adapter2.Fill(dt2);
            foreach (DataRow row in dt2.Rows)
            {
                updated = true;
                if (dateUpd >= (DateTime) row["date_version"])
                {
                    row["cancel"] = 1;
                }
                else
                {
                    cancelStatus = 1;
                }
            }

            var commandBuilder =
                new MySqlCommandBuilder(adapter2) {ConflictOption = ConflictOption.OverwriteChanges};
            adapter2.Update(dt2);
        }

        private bool TenderExist(MySqlConnection connect)
        {
            var selectTend =
                $"SELECT id_tender FROM {Builder.Prefix}tender WHERE purchase_number = @purchase_number AND end_date = @end_date AND type_fz = @type_fz AND doc_publish_date = @doc_publish_date";
            var cmd = new MySqlCommand(selectTend, connect);
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@purchase_number", _tn.PurNum);
            cmd.Parameters.AddWithValue("@end_date", _tn.DateEnd);
            cmd.Parameters.AddWithValue("@type_fz", TypeFz);
            cmd.Parameters.AddWithValue("@doc_publish_date", _tn.DatePub);
            var dt = new DataTable();
            var adapter = new MySqlDataAdapter {SelectCommand = cmd};
            adapter.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                return true;
            }

            return false;
        }
    }
}