#region

using System;
using System.Data;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using ParserWebCore.BuilderApp;
using ParserWebCore.Connections;
using ParserWebCore.Extensions;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.TenderType;

#endregion

namespace ParserWebCore.Tender
{
    public class TenderMts : TenderAbstract, ITender
    {
        private readonly TypeMts _tn;

        public TenderMts(string etpName, string etpUrl, int typeFz, TypeMts tn) : base(etpName,
            etpUrl,
            typeFz)
        {
            _tn = tn;
            PlacingWay = tn.PlacingWay;
        }

        public void ParsingTender()
        {
            using (var connect = ConnectToDb.GetDbConnection())
            {
                connect.Open();
                var selectTend =
                    $"SELECT id_tender FROM {AppBuilder.Prefix}tender WHERE purchase_number = @purchase_number AND type_fz = @type_fz AND notice_version = @notice_version AND end_date = @end_date";
                var cmd = new MySqlCommand(selectTend, connect);
                cmd.Prepare();
                cmd.Parameters.AddWithValue("@purchase_number", _tn.PurNum);
                cmd.Parameters.AddWithValue("@type_fz", TypeFz);
                cmd.Parameters.AddWithValue("@notice_version", _tn.Status);
                cmd.Parameters.AddWithValue("@end_date", _tn.DateEnd);
                var dt = new DataTable();
                var adapter = new MySqlDataAdapter { SelectCommand = cmd };
                adapter.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    return;
                }

                var dateUpd = DateTime.Now;
                var (updated, cancelStatus) = UpdateTenderVersion(connect, _tn.PurNum, dateUpd);
                var result = DownloadString.DownLUserAgent("https://tenders.mts.ru/api/v2/tender/" + _tn.Id);
                var t = JObject.Parse(result);
                var datePubS =
                    ((string)t.SelectToken(
                         "publicationDate") ??
                     throw new ApplicationException($"datePub not found {_tn.Href}")).Trim();
                _tn.DatePub = datePubS.ParseDateUn("yyyy-MM-dd");
                CreaateOrganizer(connect, out var organiserId);
                GetPlacingWay(connect, out var idPlacingWay);
                GetEtp(connect, out var idEtp);
                var idRegion = GetRegionFromString(_tn.Region, connect);
                var idTender = CreateTender(connect, idRegion, organiserId, idPlacingWay, idEtp, cancelStatus, dateUpd,
                    updated);
                var attacments = GetElements(t, "attachments");
                foreach (var att in attacments)
                {
                    var name = ((string)att.SelectToken("name") ?? "").Trim();
                    var url = ((string)att.SelectToken("url") ?? "").Trim();
                    if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(url))
                    {
                        var insertAttach =
                            $"INSERT INTO {AppBuilder.Prefix}attachment SET id_tender = @id_tender, file_name = @file_name, url = @url, description = @description";
                        var cmd10 = new MySqlCommand(insertAttach, connect);
                        cmd10.Prepare();
                        cmd10.Parameters.AddWithValue("@id_tender", idTender);
                        cmd10.Parameters.AddWithValue("@file_name", name);
                        cmd10.Parameters.AddWithValue("@url", url);
                        cmd10.Parameters.AddWithValue("@description", "");
                        cmd10.ExecuteNonQuery();
                    }
                }

                CreateCustomer(connect, out var customerId);
                CreateLot(connect, idTender, customerId);
                TenderKwords(connect, idTender);
                AddVerNumber(connect, _tn.PurNum, TypeFz);
            }
        }

        private void CreateLot(MySqlConnection connect, int idTender, int customerId)
        {
            var lotNum = 1;
            var insertLot =
                $"INSERT INTO {AppBuilder.Prefix}lot SET id_tender = @id_tender, lot_number = @lot_number, max_price = @max_price, currency = @currency, finance_source = @finance_source";
            var cmd18 = new MySqlCommand(insertLot, connect);
            cmd18.Prepare();
            cmd18.Parameters.AddWithValue("@id_tender", idTender);
            cmd18.Parameters.AddWithValue("@lot_number", lotNum);
            cmd18.Parameters.AddWithValue("@max_price", "");
            cmd18.Parameters.AddWithValue("@currency", "");
            cmd18.Parameters.AddWithValue("@finance_source", "");
            cmd18.ExecuteNonQuery();
            var idLot = (int)cmd18.LastInsertedId;
            var insertLotitem =
                $"INSERT INTO {AppBuilder.Prefix}purchase_object SET id_lot = @id_lot, id_customer = @id_customer, name = @name, quantity_value = @quantity_value, okei = @okei, customer_quantity_value = @customer_quantity_value, price = @price, sum = @sum";
            var cmd20 = new MySqlCommand(insertLotitem, connect);
            cmd20.Prepare();
            cmd20.Parameters.AddWithValue("@id_lot", idLot);
            cmd20.Parameters.AddWithValue("@id_customer", customerId);
            cmd20.Parameters.AddWithValue("@name", _tn.PurName);
            cmd20.Parameters.AddWithValue("@quantity_value", "");
            cmd20.Parameters.AddWithValue("@okei", "");
            cmd20.Parameters.AddWithValue("@customer_quantity_value", "");
            cmd20.Parameters.AddWithValue("@price", "");
            cmd20.Parameters.AddWithValue("@sum", "");
            cmd20.ExecuteNonQuery();
        }

        private void CreaateOrganizer(MySqlConnection connect, out int organiserId)
        {
            organiserId = 0;
            if (!string.IsNullOrEmpty(EtpName))
            {
                var selectOrg =
                    $"SELECT id_organizer FROM {AppBuilder.Prefix}organizer WHERE full_name = @full_name";
                var cmd3 = new MySqlCommand(selectOrg, connect);
                cmd3.Prepare();
                cmd3.Parameters.AddWithValue("@full_name", EtpName);
                var dt3 = new DataTable();
                var adapter3 = new MySqlDataAdapter { SelectCommand = cmd3 };
                adapter3.Fill(dt3);
                if (dt3.Rows.Count > 0)
                {
                    organiserId = (int)dt3.Rows[0].ItemArray[0];
                }
                else
                {
                    var phone = "";
                    var email = "";
                    var inn = "";
                    var kpp = "";
                    var contactPerson = "";
                    var addOrganizer =
                        $"INSERT INTO {AppBuilder.Prefix}organizer SET full_name = @full_name, contact_phone = @contact_phone, contact_person = @contact_person, contact_email = @contact_email, inn = @inn, kpp = @kpp";
                    var cmd4 = new MySqlCommand(addOrganizer, connect);
                    cmd4.Prepare();
                    cmd4.Parameters.AddWithValue("@full_name", EtpName);
                    cmd4.Parameters.AddWithValue("@contact_phone", phone);
                    cmd4.Parameters.AddWithValue("@contact_person", contactPerson);
                    cmd4.Parameters.AddWithValue("@contact_email", email);
                    cmd4.Parameters.AddWithValue("@inn", inn);
                    cmd4.Parameters.AddWithValue("@kpp", kpp);
                    cmd4.ExecuteNonQuery();
                    organiserId = (int)cmd4.LastInsertedId;
                }
            }
        }

        private int CreateTender(MySqlConnection connect, int idRegion, int organiserId, int idPlacingWay, int idEtp,
            int cancelStatus, DateTime dateUpd, bool updated)
        {
            var insertTender =
                $"INSERT INTO {AppBuilder.Prefix}tender SET id_region = @id_region, id_xml = @id_xml, purchase_number = @purchase_number, doc_publish_date = @doc_publish_date, href = @href, purchase_object_info = @purchase_object_info, type_fz = @type_fz, id_organizer = @id_organizer, id_placing_way = @id_placing_way, id_etp = @id_etp, end_date = @end_date, scoring_date = @scoring_date, bidding_date = @bidding_date, cancel = @cancel, date_version = @date_version, num_version = @num_version, notice_version = @notice_version, xml = @xml, print_form = @print_form";
            var cmd9 = new MySqlCommand(insertTender, connect);
            cmd9.Prepare();
            cmd9.Parameters.AddWithValue("@id_region", idRegion);
            cmd9.Parameters.AddWithValue("@id_xml", _tn.PurNum);
            cmd9.Parameters.AddWithValue("@purchase_number", _tn.PurNum);
            cmd9.Parameters.AddWithValue("@doc_publish_date", _tn.DatePub);
            cmd9.Parameters.AddWithValue("@href", _tn.Href);
            cmd9.Parameters.AddWithValue("@purchase_object_info", _tn.PurName);
            cmd9.Parameters.AddWithValue("@type_fz", TypeFz);
            cmd9.Parameters.AddWithValue("@id_organizer", organiserId);
            cmd9.Parameters.AddWithValue("@id_placing_way", idPlacingWay);
            cmd9.Parameters.AddWithValue("@id_etp", idEtp);
            cmd9.Parameters.AddWithValue("@end_date", _tn.DateEnd);
            cmd9.Parameters.AddWithValue("@scoring_date", DateTime.MinValue);
            cmd9.Parameters.AddWithValue("@bidding_date", DateTime.MinValue);
            cmd9.Parameters.AddWithValue("@cancel", cancelStatus);
            cmd9.Parameters.AddWithValue("@date_version", dateUpd);
            cmd9.Parameters.AddWithValue("@num_version", 1);
            cmd9.Parameters.AddWithValue("@notice_version", _tn.Status);
            cmd9.Parameters.AddWithValue("@xml", _tn.Href);
            cmd9.Parameters.AddWithValue("@print_form", _tn.Href);
            var resInsertTender = cmd9.ExecuteNonQuery();
            var idTender = (int)cmd9.LastInsertedId;
            Counter(resInsertTender, updated);
            return idTender;
        }

        private void CreateCustomer(MySqlConnection connect, out int customerId)
        {
            customerId = 0;
            if (!string.IsNullOrEmpty(EtpName))
            {
                var selectCustomer =
                    $"SELECT id_customer FROM {AppBuilder.Prefix}customer WHERE full_name = @full_name";
                var cmd13 = new MySqlCommand(selectCustomer, connect);
                cmd13.Prepare();
                cmd13.Parameters.AddWithValue("@full_name", EtpName);
                var reader7 = cmd13.ExecuteReader();
                if (reader7.HasRows)
                {
                    reader7.Read();
                    customerId = (int)reader7["id_customer"];
                    reader7.Close();
                }
                else
                {
                    reader7.Close();
                    var insertCustomer =
                        $"INSERT INTO {AppBuilder.Prefix}customer SET reg_num = @reg_num, full_name = @full_name, is223=1, inn = @inn";
                    var cmd14 = new MySqlCommand(insertCustomer, connect);
                    cmd14.Prepare();
                    var customerRegNumber = Guid.NewGuid().ToString();
                    cmd14.Parameters.AddWithValue("@reg_num", customerRegNumber);
                    cmd14.Parameters.AddWithValue("@full_name", EtpName);
                    cmd14.Parameters.AddWithValue("@inn", "");
                    cmd14.ExecuteNonQuery();
                    customerId = (int)cmd14.LastInsertedId;
                }
            }
        }
    }
}