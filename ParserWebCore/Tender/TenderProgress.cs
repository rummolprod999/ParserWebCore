using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using ParserWebCore.BuilderApp;
using ParserWebCore.Connections;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.TenderType;

namespace ParserWebCore.Tender
{
    public class TenderProgress : TenderAbstract, ITender
    {
        private readonly TypeProgress _tn;

        public TenderProgress(string etpName, string etpUrl, int typeFz, TypeProgress tn) : base(etpName,
            etpUrl,
            typeFz)
        {
            _tn = tn;
            PlacingWay = tn.PwName;
        }

        public void ParsingTender()
        {
            using (var connect = ConnectToDb.GetDbConnection())
            {
                connect.Open();
                var selectTend =
                    $"SELECT id_tender FROM {AppBuilder.Prefix}tender WHERE purchase_number = @purchase_number AND doc_publish_date = @doc_publish_date AND type_fz = @type_fz AND notice_version = @notice_version AND end_date = @end_date";
                var cmd = new MySqlCommand(selectTend, connect);
                cmd.Prepare();
                cmd.Parameters.AddWithValue("@purchase_number", _tn.PurNum);
                cmd.Parameters.AddWithValue("@doc_publish_date", _tn.DatePub);
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
                var url =
                    $"https://tender.progressagro.com/api/Lots/anonymous/Detail/{_tn.PurNum}/{Guid.NewGuid()}";
                var s = DownloadString.DownL(url);
                if (string.IsNullOrEmpty(s))
                {
                    Log.Logger(
                        $"Empty string in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                        url);
                    return;
                }

                var jObj = JObject.Parse(s);
                var t = jObj.Root ?? throw new ApplicationException($"data was not found  {_tn.Href}");
                var (updated, cancelStatus) = UpdateTenderVersion(connect, _tn.PurNum, dateUpd);
                var printForm = _tn.Href;
                var customerId = 0;
                var organiserId = 0;
                organiserId = GetOrganizer(t, connect);
                GetPlacingWay(connect, out var idPlacingWay);
                GetEtp(connect, out var idEtp);
                var insertTender =
                    $"INSERT INTO {AppBuilder.Prefix}tender SET id_region = @id_region, id_xml = @id_xml, purchase_number = @purchase_number, doc_publish_date = @doc_publish_date, href = @href, purchase_object_info = @purchase_object_info, type_fz = @type_fz, id_organizer = @id_organizer, id_placing_way = @id_placing_way, id_etp = @id_etp, end_date = @end_date, scoring_date = @scoring_date, bidding_date = @bidding_date, cancel = @cancel, date_version = @date_version, num_version = @num_version, notice_version = @notice_version, xml = @xml, print_form = @print_form";
                var cmd9 = new MySqlCommand(insertTender, connect);
                cmd9.Prepare();
                cmd9.Parameters.AddWithValue("@id_region", 0);
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
                cmd9.Parameters.AddWithValue("@print_form", printForm);
                var resInsertTender = cmd9.ExecuteNonQuery();
                var idTender = (int)cmd9.LastInsertedId;
                Counter(resInsertTender, updated);
                var attacments = GetElements(t, "documents");
                WriteAttachments(connect, attacments, idTender);
                GetLots(connect, idTender, t);
                TenderKwords(connect, idTender);
                AddVerNumber(connect, _tn.PurNum, TypeFz);
            }
        }

        private void GetLots(MySqlConnection connect, int idTender, JToken t)
        {
            var cusName = ((string)(t.SelectToken(
                               "companies")) ??
                           EtpName).Trim();
            var customerId = 0;
            if (!string.IsNullOrEmpty(cusName))
            {
                var selectCustomer =
                    $"SELECT id_customer FROM {AppBuilder.Prefix}customer WHERE full_name = @full_name";
                var cmd13 = new MySqlCommand(selectCustomer, connect);
                cmd13.Prepare();
                cmd13.Parameters.AddWithValue("@full_name", cusName);
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
                    cmd14.Parameters.AddWithValue("@full_name", cusName);
                    cmd14.Parameters.AddWithValue("@inn", "");
                    cmd14.ExecuteNonQuery();
                    customerId = (int)cmd14.LastInsertedId;
                }
            }

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
            var delivT1 = ((string)(t.SelectToken(
                               "$..deliveryTime")) ??
                           "").Trim();
            var delivT2 = ((string)(t.SelectToken(
                               "$..deliveryConditionDescription")) ??
                           "").Trim();
            var delivT3 = ((string)(t.SelectToken(
                               "$..paymentConditionDescription")) ??
                           "").Trim();
            var delivTerm =
                $"Срок поставки: {delivT1}\nУсловия поставки: {delivT2}\nУсловия оплаты: {delivT3}";
            var insertCustomerRequirement =
                $"INSERT INTO {AppBuilder.Prefix}customer_requirement SET id_lot = @id_lot, id_customer = @id_customer, delivery_place = @delivery_place, max_price = @max_price, delivery_term = @delivery_term";
            var cmd16 = new MySqlCommand(insertCustomerRequirement, connect);
            cmd16.Prepare();
            cmd16.Parameters.AddWithValue("@id_lot", idLot);
            cmd16.Parameters.AddWithValue("@id_customer", customerId);
            cmd16.Parameters.AddWithValue("@delivery_place", "");
            cmd16.Parameters.AddWithValue("@max_price", "");
            cmd16.Parameters.AddWithValue("@delivery_term", delivTerm);
            cmd16.ExecuteNonQuery();
            var purObjects = GetElements(t, "order.rows");
            purObjects.ForEach(po =>
            {
                var okpdName = ((string)po.SelectToken(
                    "okpd.name") ?? "").Trim();
                var okpdCode = ((string)po.SelectToken(
                    "okpd.code") ?? "").Trim();
                var okei = ((string)po.SelectToken(
                    "unitName") ?? "").Trim();
                var quantity = (string)po.SelectToken(
                    "quantity") ?? "";
                var price = (string)po.SelectToken(
                    "startingPrice") ?? "";
                var sum = (string)po.SelectToken(
                    "max_cost") ?? "";
                var poName = ((string)po.SelectToken(
                    "productName") ?? "").Trim();
                var descr = ((string)po.SelectToken(
                    "otherCharacteristic") ?? "").Trim();
                if (descr != "")
                {
                    poName = $"{poName}\n{descr}";
                }

                var insertLotitem =
                    $"INSERT INTO {AppBuilder.Prefix}purchase_object SET id_lot = @id_lot, id_customer = @id_customer, name = @name, sum = @sum, okpd2_code = @okpd2_code, okpd2_group_code = @okpd2_group_code, okpd2_group_level1_code = @okpd2_group_level1_code, okpd_name = @okpd_name, quantity_value = @quantity_value, customer_quantity_value = @customer_quantity_value, okei = @okei, price = @price";
                var cmd19 = new MySqlCommand(insertLotitem, connect);
                cmd19.Prepare();
                cmd19.Parameters.AddWithValue("@id_lot", idLot);
                cmd19.Parameters.AddWithValue("@id_customer", customerId);
                cmd19.Parameters.AddWithValue("@name", poName);
                cmd19.Parameters.AddWithValue("@sum", sum);
                cmd19.Parameters.AddWithValue("@okpd2_code", okpdCode);
                cmd19.Parameters.AddWithValue("@okpd2_group_code", "");
                cmd19.Parameters.AddWithValue("@okpd2_group_level1_code", "");
                cmd19.Parameters.AddWithValue("@okpd_name", okpdName);
                cmd19.Parameters.AddWithValue("@quantity_value", quantity);
                cmd19.Parameters.AddWithValue("@customer_quantity_value", quantity);
                cmd19.Parameters.AddWithValue("@okei", okei);
                cmd19.Parameters.AddWithValue("@price", price);
                cmd19.ExecuteNonQuery();
            });
        }

        private void WriteAttachments(MySqlConnection connect, List<JToken> attachments, int idTender)
        {
            foreach (var att in attachments)
            {
                var name = ((string)att.SelectToken("name") ?? "").Trim();

                var url = ((string)att.SelectToken("id") ?? "").Trim();
                url = $"https://tender.progressagro.com/api/Files/app/{url}?access_token=null";
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
        }

        private int GetOrganizer(JToken t, MySqlConnection connect)
        {
            var organiserId = 0;
            var orgFullName = ((string)(t.SelectToken(
                                   "companies")) ??
                               "").Trim();
            if (!string.IsNullOrEmpty(orgFullName))
            {
                var selectOrg =
                    $"SELECT id_organizer FROM {AppBuilder.Prefix}organizer WHERE full_name = @full_name";
                var cmd3 = new MySqlCommand(selectOrg, connect);
                cmd3.Prepare();
                cmd3.Parameters.AddWithValue("@full_name", orgFullName);
                var dt3 = new DataTable();
                var adapter3 = new MySqlDataAdapter { SelectCommand = cmd3 };
                adapter3.Fill(dt3);
                if (dt3.Rows.Count > 0)
                {
                    organiserId = (int)dt3.Rows[0].ItemArray[0];
                }
                else
                {
                    var phone = ((string)(t.SelectToken(
                                     "contactPhone")) ??
                                 "").Trim();
                    var email = ((string)(t.SelectToken(
                                     "createdUser.email")) ??
                                 "").Trim();
                    var contactPerson =
                        ((string)(t.SelectToken(
                             "createdUser.name")) ??
                         "").Trim();
                    var addOrganizer =
                        $"INSERT INTO {AppBuilder.Prefix}organizer SET full_name = @full_name, contact_phone = @contact_phone, contact_person = @contact_person, contact_email = @contact_email";
                    var cmd4 = new MySqlCommand(addOrganizer, connect);
                    cmd4.Prepare();
                    cmd4.Parameters.AddWithValue("@full_name", orgFullName);
                    cmd4.Parameters.AddWithValue("@contact_phone", phone);
                    cmd4.Parameters.AddWithValue("@contact_person", contactPerson);
                    cmd4.Parameters.AddWithValue("@contact_email", email);
                    cmd4.ExecuteNonQuery();
                    organiserId = (int)cmd4.LastInsertedId;
                }
            }

            return organiserId;
        }
    }
}