using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using AngleSharp.Parser.Html;
using HtmlAgilityPack;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using ParserWebCore.BuilderApp;
using ParserWebCore.Connections;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.TenderType;

namespace ParserWebCore.Tender
{
    public class TenderSberB2B : TenderAbstract, ITender
    {
        private readonly TypeSber _tn;

        public TenderSberB2B(string etpName, string etpUrl, int typeFz, TypeSber tn) : base(etpName, etpUrl,
            typeFz)
        {
            _tn = tn;
        }

        public void ParsingTender()
        {
            using (var connect = ConnectToDb.GetDbConnection())
            {
                connect.Open();
                var selectTend =
                    $"SELECT id_tender FROM {Builder.Prefix}tender WHERE purchase_number = @purchase_number AND doc_publish_date = @doc_publish_date AND type_fz = @type_fz AND notice_version = @notice_version";
                var cmd = new MySqlCommand(selectTend, connect);
                cmd.Prepare();
                cmd.Parameters.AddWithValue("@purchase_number", _tn.PurNum);
                cmd.Parameters.AddWithValue("@doc_publish_date", _tn.DatePub);
                cmd.Parameters.AddWithValue("@type_fz", TypeFz);
                cmd.Parameters.AddWithValue("@notice_version", _tn.Status);
                var dt = new DataTable();
                var adapter = new MySqlDataAdapter {SelectCommand = cmd};
                adapter.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    //Log.Logger("This tender is exist in base", PurNum);
                    return;
                }

                var s = DownloadString.DownL(_tn.Href);
                if (String.IsNullOrEmpty(s))
                {
                    Log.Logger(
                        $"Empty string in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                        _tn.Href);
                }

                var ss = s.GetDataFromRegex(":request=\"(.+)}\"");
                ss += "}";
                ss = HttpUtility.HtmlDecode(ss);
                var tender = JObject.Parse(ss);
                var purNum = _tn.PurNum;
                var noticeVersion = _tn.Status;
                var dateUpd = DateTime.Now;

                var (updated, cancelStatus) = UpdateTenderVersion(connect, purNum, dateUpd);
                var printForm = _tn.Href;
                var customerId = 0;
                var organiserId = 0;
                if (_tn.CusName != "")
                {
                    var selectOrg =
                        $"SELECT id_organizer FROM {Builder.Prefix}organizer WHERE full_name = @full_name";
                    var cmd3 = new MySqlCommand(selectOrg, connect);
                    cmd3.Prepare();
                    cmd3.Parameters.AddWithValue("@full_name", _tn.CusName);
                    var dt3 = new DataTable();
                    var adapter3 = new MySqlDataAdapter {SelectCommand = cmd3};
                    adapter3.Fill(dt3);
                    if (dt3.Rows.Count > 0)
                    {
                        organiserId = (int) dt3.Rows[0].ItemArray[0];
                    }
                    else
                    {
                        var phone = ((string) tender.SelectToken("customer.phone") ?? "").Trim();
                        var email = "";
                        var contactPerson = ((string) tender.SelectToken("customer.director_fullname") ?? "").Trim();
                        var inn = ((string) tender.SelectToken("customer.inn") ?? "").Trim();
                        var kpp = ((string) tender.SelectToken("customer.kpp") ?? "").Trim();
                        var addOrganizer =
                            $"INSERT INTO {Builder.Prefix}organizer SET full_name = @full_name, contact_phone = @contact_phone, contact_person = @contact_person, contact_email = @contact_email, inn = @inn, kpp = @kpp";
                        var cmd4 = new MySqlCommand(addOrganizer, connect);
                        cmd4.Prepare();
                        cmd4.Parameters.AddWithValue("@full_name", _tn.CusName);
                        cmd4.Parameters.AddWithValue("@contact_phone", phone);
                        cmd4.Parameters.AddWithValue("@contact_person", contactPerson);
                        cmd4.Parameters.AddWithValue("@contact_email", email);
                        cmd4.Parameters.AddWithValue("@inn", inn);
                        cmd4.Parameters.AddWithValue("@kpp", kpp);
                        cmd4.ExecuteNonQuery();
                        organiserId = (int) cmd4.LastInsertedId;
                    }
                }

                var idPlacingWay = 0;
                GetEtp(connect, out var idEtp);
                var insertTender =
                    $"INSERT INTO {Builder.Prefix}tender SET id_region = @id_region, id_xml = @id_xml, purchase_number = @purchase_number, doc_publish_date = @doc_publish_date, href = @href, purchase_object_info = @purchase_object_info, type_fz = @type_fz, id_organizer = @id_organizer, id_placing_way = @id_placing_way, id_etp = @id_etp, end_date = @end_date, scoring_date = @scoring_date, bidding_date = @bidding_date, cancel = @cancel, date_version = @date_version, num_version = @num_version, notice_version = @notice_version, xml = @xml, print_form = @print_form";
                var cmd9 = new MySqlCommand(insertTender, connect);
                cmd9.Prepare();
                cmd9.Parameters.AddWithValue("@id_region", 0);
                cmd9.Parameters.AddWithValue("@id_xml", purNum);
                cmd9.Parameters.AddWithValue("@purchase_number", purNum);
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
                cmd9.Parameters.AddWithValue("@notice_version", noticeVersion);
                cmd9.Parameters.AddWithValue("@xml", _tn.Href);
                cmd9.Parameters.AddWithValue("@print_form", printForm);
                var resInsertTender = cmd9.ExecuteNonQuery();
                var idTender = (int) cmd9.LastInsertedId;
                Counter(resInsertTender, updated);
                var attachments = GetElements(tender, "attachments_files");
                foreach (var att in attachments)
                {
                    var name = ((string) att.SelectToken("name") ?? "").Trim();
                    var fName = ((string) att.SelectToken("file.original_name") ?? "").Trim();
                    var webPath = ((string) att.SelectToken("web_path") ?? "").Trim();
                    var urlAtt = $"https://sberb2b.ru{webPath}{name}";
                    if (!string.IsNullOrEmpty(fName))
                    {
                        var insertAttach =
                            $"INSERT INTO {Builder.Prefix}attachment SET id_tender = @id_tender, file_name = @file_name, url = @url";
                        var cmd10 = new MySqlCommand(insertAttach, connect);
                        cmd10.Prepare();
                        cmd10.Parameters.AddWithValue("@id_tender", idTender);
                        cmd10.Parameters.AddWithValue("@file_name", fName);
                        cmd10.Parameters.AddWithValue("@url", urlAtt);
                        cmd10.ExecuteNonQuery();
                    }
                }

                var lotNum = 1;
                var nmck = (decimal?) tender.SelectToken("last_customer_published_condition.total_price") ?? 0.0m;
                var insertLot =
                    $"INSERT INTO {Builder.Prefix}lot SET id_tender = @id_tender, lot_number = @lot_number, max_price = @max_price, currency = @currency";
                var cmd18 = new MySqlCommand(insertLot, connect);
                cmd18.Prepare();
                cmd18.Parameters.AddWithValue("@id_tender", idTender);
                cmd18.Parameters.AddWithValue("@lot_number", lotNum);
                cmd18.Parameters.AddWithValue("@max_price", nmck);
                cmd18.Parameters.AddWithValue("@currency", "");
                cmd18.ExecuteNonQuery();
                var idLot = (int) cmd18.LastInsertedId;
                if (!string.IsNullOrEmpty(_tn.CusName))
                {
                    var selectCustomer =
                        $"SELECT id_customer FROM {Builder.Prefix}customer WHERE full_name = @full_name";
                    var cmd13 = new MySqlCommand(selectCustomer, connect);
                    cmd13.Prepare();
                    cmd13.Parameters.AddWithValue("@full_name", _tn.CusName);
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
                            $"INSERT INTO {Builder.Prefix}customer SET reg_num = @reg_num, full_name = @full_name, inn = @inn, is223=1";
                        var cmd14 = new MySqlCommand(insertCustomer, connect);
                        cmd14.Prepare();
                        var customerRegNumber = Guid.NewGuid().ToString();
                        var inn = ((string) tender.SelectToken("customer.inn") ?? "").Trim();
                        cmd14.Parameters.AddWithValue("@reg_num", customerRegNumber);
                        cmd14.Parameters.AddWithValue("@full_name", _tn.CusName);
                        cmd14.Parameters.AddWithValue("@inn", inn);
                        cmd14.ExecuteNonQuery();
                        customerId = (int) cmd14.LastInsertedId;
                    }
                }

                var insertLotitem =
                    $"INSERT INTO {Builder.Prefix}purchase_object SET id_lot = @id_lot, id_customer = @id_customer, name = @name, sum = @sum, okpd2_code = @okpd2_code, okpd2_group_code = @okpd2_group_code, okpd2_group_level1_code = @okpd2_group_level1_code, okpd_name = @okpd_name";
                var cmd19 = new MySqlCommand(insertLotitem, connect);
                cmd19.Prepare();
                cmd19.Parameters.AddWithValue("@id_lot", idLot);
                cmd19.Parameters.AddWithValue("@id_customer", customerId);
                cmd19.Parameters.AddWithValue("@name", _tn.PurName);
                cmd19.Parameters.AddWithValue("@sum", nmck);
                cmd19.Parameters.AddWithValue("@okpd2_code", "");
                cmd19.Parameters.AddWithValue("@okpd2_group_code", "");
                cmd19.Parameters.AddWithValue("@okpd2_group_level1_code", "");
                cmd19.Parameters.AddWithValue("@okpd_name", "");
                cmd19.ExecuteNonQuery();
                var delivPl =
                    ((string) tender.SelectToken(
                         "last_customer_published_condition.condition_deliveries[0].builded_string") ?? "").Trim();
                var delivTerm =
                    ((string) tender.SelectToken(
                         "last_customer_published_condition.condition_payment.string_representation") ?? "").Trim();
                if (delivTerm != "" || delivPl != "")
                {
                    var insertCustomerRequirement =
                        $"INSERT INTO {Builder.Prefix}customer_requirement SET id_lot = @id_lot, id_customer = @id_customer, max_price = @max_price, delivery_place = @delivery_place, delivery_term = @delivery_term";
                    var cmd16 = new MySqlCommand(insertCustomerRequirement, connect);
                    cmd16.Prepare();
                    cmd16.Parameters.AddWithValue("@id_lot", idLot);
                    cmd16.Parameters.AddWithValue("@id_customer", customerId);
                    cmd16.Parameters.AddWithValue("@delivery_place", delivPl);
                    cmd16.Parameters.AddWithValue("@delivery_term", delivTerm);
                    cmd16.Parameters.AddWithValue("@max_price", nmck);
                    cmd16.ExecuteNonQuery();
                }

                TenderKwords(connect, idTender);
                AddVerNumber(connect, purNum, TypeFz);
            }
        }

        public List<JToken> GetElements(JToken j, string s)
        {
            var els = new List<JToken>();
            var elsObj = j.SelectToken(s);
            if (elsObj == null || elsObj.Type == JTokenType.Null) return els;
            switch (elsObj.Type)
            {
                case JTokenType.Object:
                    els.Add(elsObj);
                    break;
                case JTokenType.Array:
                    els.AddRange(elsObj);
                    break;
            }

            return els;
        }
    }
}