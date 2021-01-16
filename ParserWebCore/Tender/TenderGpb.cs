using System;
using System.Collections.Generic;
using System.Data;
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
    public class TenderGpb : TenderAbstract, ITender
    {
        private readonly TypeGpb _tn;

        public TenderGpb(string etpName, string etpUrl, int typeFz, TypeGpb tn) : base(etpName, etpUrl,
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
                    $"SELECT id_tender FROM {Builder.Prefix}tender WHERE purchase_number = @purchase_number AND doc_publish_date = @doc_publish_date AND type_fz = @type_fz AND end_date = @end_date";
                var cmd = new MySqlCommand(selectTend, connect);
                cmd.Prepare();
                cmd.Parameters.AddWithValue("@purchase_number", _tn.PurNum);
                cmd.Parameters.AddWithValue("@doc_publish_date", _tn.DatePub);
                cmd.Parameters.AddWithValue("@type_fz", TypeFz);
                cmd.Parameters.AddWithValue("@end_date", _tn.DateEnd);
                var dt = new DataTable();
                var adapter = new MySqlDataAdapter {SelectCommand = cmd};
                adapter.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    return;
                }

                var s = DownloadString.DownLUserAgent(_tn.Href);
                if (string.IsNullOrEmpty(s))
                {
                    Log.Logger(
                        $"Empty string in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                        _tn.Href);
                    return;
                }

                var tender = JObject.Parse(s);
                var dateUpd = DateTime.Now;
                var (updated, cancelStatus) = UpdateTenderVersion(connect, _tn.PurNum, dateUpd);
                var printForm = _tn.Href;
                var organiserId = 0;
                var orgName = EtpName;
                if (orgName != "")
                {
                    var selectOrg =
                        $"SELECT id_organizer FROM {Builder.Prefix}organizer WHERE full_name = @full_name";
                    var cmd3 = new MySqlCommand(selectOrg, connect);
                    cmd3.Prepare();
                    cmd3.Parameters.AddWithValue("@full_name", orgName);
                    var dt3 = new DataTable();
                    var adapter3 = new MySqlDataAdapter {SelectCommand = cmd3};
                    adapter3.Fill(dt3);
                    if (dt3.Rows.Count > 0)
                    {
                        organiserId = (int) dt3.Rows[0].ItemArray[0];
                    }
                    else
                    {
                        var phone = "";
                        var email = "";
                        var contactPerson = "";
                        var inn = "";
                        var kpp = "";
                        var postAddr = "";
                        var addOrganizer =
                            $"INSERT INTO {Builder.Prefix}organizer SET full_name = @full_name, contact_phone = @contact_phone, contact_person = @contact_person, contact_email = @contact_email, inn = @inn, kpp = @kpp, post_address = @post_address";
                        var cmd4 = new MySqlCommand(addOrganizer, connect);
                        cmd4.Prepare();
                        cmd4.Parameters.AddWithValue("@full_name", orgName);
                        cmd4.Parameters.AddWithValue("@contact_phone", phone);
                        cmd4.Parameters.AddWithValue("@contact_person", contactPerson);
                        cmd4.Parameters.AddWithValue("@contact_email", email);
                        cmd4.Parameters.AddWithValue("@inn", inn);
                        cmd4.Parameters.AddWithValue("@kpp", kpp);
                        cmd4.Parameters.AddWithValue("@post_address", postAddr);
                        cmd4.ExecuteNonQuery();
                        organiserId = (int) cmd4.LastInsertedId;
                    }
                }

                var idPlacingWay = 0;
                GetEtp(connect, out var idEtp);
                var idRegion = 0;
                var regions = GetElements(tender, "regions");
                if (regions.Count == 1)
                {
                    var regName = (string) regions[0].SelectToken("name") ?? "";
                    idRegion = GetRegionFromString(regName, connect);
                }

                var noticeVersion = (int?) tender.SelectToken("status") ?? 0;
                var insertTender =
                    $"INSERT INTO {Builder.Prefix}tender SET id_region = @id_region, id_xml = @id_xml, purchase_number = @purchase_number, doc_publish_date = @doc_publish_date, href = @href, purchase_object_info = @purchase_object_info, type_fz = @type_fz, id_organizer = @id_organizer, id_placing_way = @id_placing_way, id_etp = @id_etp, end_date = @end_date, scoring_date = @scoring_date, bidding_date = @bidding_date, cancel = @cancel, date_version = @date_version, num_version = @num_version, notice_version = @notice_version, xml = @xml, print_form = @print_form";
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
                cmd9.Parameters.AddWithValue("@bidding_date", _tn.DateEnd);
                cmd9.Parameters.AddWithValue("@cancel", cancelStatus);
                cmd9.Parameters.AddWithValue("@date_version", dateUpd);
                cmd9.Parameters.AddWithValue("@num_version", 1);
                cmd9.Parameters.AddWithValue("@notice_version", noticeVersion);
                cmd9.Parameters.AddWithValue("@xml", _tn.Href);
                cmd9.Parameters.AddWithValue("@print_form", printForm);
                var resInsertTender = cmd9.ExecuteNonQuery();
                var idTender = (int) cmd9.LastInsertedId;
                Counter(resInsertTender, updated);
                var attacments = GetElements(tender, "customer_documents");
                WriteAttachments(connect, attacments, idTender);
                var lotNum = 1;
                var insertLot =
                    $"INSERT INTO {Builder.Prefix}lot SET id_tender = @id_tender, lot_number = @lot_number, max_price = @max_price, currency = @currency";
                var cmd18 = new MySqlCommand(insertLot, connect);
                cmd18.Prepare();
                cmd18.Parameters.AddWithValue("@id_tender", idTender);
                cmd18.Parameters.AddWithValue("@lot_number", lotNum);
                cmd18.Parameters.AddWithValue("@max_price", "");
                cmd18.Parameters.AddWithValue("@currency", "");
                cmd18.ExecuteNonQuery();
                var idLot = (int) cmd18.LastInsertedId;
                var customerId = 0;
                var cusName = ((string) tender.SelectToken("customer_name") ?? "").Trim();
                var cusInn = ((string) tender.SelectToken("customer_inn") ?? "").Trim();
                if (!string.IsNullOrEmpty(cusName))
                {
                    var selectCustomer =
                        $"SELECT id_customer FROM {Builder.Prefix}customer WHERE full_name = @full_name";
                    var cmd13 = new MySqlCommand(selectCustomer, connect);
                    cmd13.Prepare();
                    cmd13.Parameters.AddWithValue("@full_name", cusName);
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
                        cmd14.Parameters.AddWithValue("@reg_num", customerRegNumber);
                        cmd14.Parameters.AddWithValue("@full_name", cusName);
                        cmd14.Parameters.AddWithValue("@inn", cusInn);
                        cmd14.ExecuteNonQuery();
                        customerId = (int) cmd14.LastInsertedId;
                    }
                }

                var restricts = new List<string>();
                var forSmallBusiness = (bool?) tender.SelectToken("for_small_business") ?? false;
                if (forSmallBusiness)
                {
                    restricts.Add("Субъект малого и среднего предпринимательства");
                }

                var forProducer = (bool?) tender.SelectToken("for_producer") ?? false;
                if (forProducer)
                {
                    restricts.Add("Производитель");
                }

                var forAuthorizedDealer = (bool?) tender.SelectToken("for_authorized_dealer") ?? false;
                if (forAuthorizedDealer)
                {
                    restricts.Add("Официальный дилер");
                }

                var russianProduction = (bool?) tender.SelectToken("russian_production") ?? false;
                if (russianProduction)
                {
                    restricts.Add("Российское производство");
                }

                var nationalProject = (bool?) tender.SelectToken("national_project") ?? false;
                if (nationalProject)
                {
                    restricts.Add("Национальный проект");
                }

                var denyAlternative = (bool?) tender.SelectToken("deny_alternative") ?? false;
                if (denyAlternative)
                {
                    restricts.Add("Запрет альтернатив");
                }

                foreach (var restrict in restricts)
                {
                    var insertRestrict =
                        $"INSERT INTO {Builder.Prefix}restricts SET id_lot = @id_lot, foreign_info = @foreign_info, info = @info";
                    var cmd19 = new MySqlCommand(insertRestrict, connect);
                    cmd19.Prepare();
                    cmd19.Parameters.AddWithValue("@id_lot", idLot);
                    cmd19.Parameters.AddWithValue("@foreign_info", restrict);
                    cmd19.Parameters.AddWithValue("@info", "");
                    cmd19.ExecuteNonQuery();
                }

                var delivDateS = (string) tender.SelectToken("date_delivery") ?? "";
                var dateDeliv = delivDateS.ParseDateUn("yyyy-MM-dd");
                var delivTerm = (string) tender.SelectToken("delivery_conditions") ?? "";
                if (dateDeliv != DateTime.MinValue)
                {
                    delivTerm =
                        $"{delivTerm}\nПредполагаемая дата поставки / выполнения работ (услуг): {dateDeliv:yyyy-MM-dd}";
                }

                foreach (var region in regions)
                {
                    var regName = (string) regions[0].SelectToken("name") ?? "";
                    if (delivTerm != "" || regName != "")
                    {
                        var insertCustomerRequirement =
                            $"INSERT INTO {Builder.Prefix}customer_requirement SET id_lot = @id_lot, id_customer = @id_customer, max_price = @max_price, delivery_place = @delivery_place, delivery_term = @delivery_term";
                        var cmd16 = new MySqlCommand(insertCustomerRequirement, connect);
                        cmd16.Prepare();
                        cmd16.Parameters.AddWithValue("@id_lot", idLot);
                        cmd16.Parameters.AddWithValue("@id_customer", customerId);
                        cmd16.Parameters.AddWithValue("@delivery_place", regName);
                        cmd16.Parameters.AddWithValue("@delivery_term", delivTerm);
                        cmd16.Parameters.AddWithValue("@max_price", "");
                        cmd16.ExecuteNonQuery();
                    }
                }

                if (regions.Count == 0)
                {
                    var insertCustomerRequirement =
                        $"INSERT INTO {Builder.Prefix}customer_requirement SET id_lot = @id_lot, id_customer = @id_customer, max_price = @max_price, delivery_place = @delivery_place, delivery_term = @delivery_term";
                    var cmd16 = new MySqlCommand(insertCustomerRequirement, connect);
                    cmd16.Prepare();
                    cmd16.Parameters.AddWithValue("@id_lot", idLot);
                    cmd16.Parameters.AddWithValue("@id_customer", customerId);
                    cmd16.Parameters.AddWithValue("@delivery_place", "");
                    cmd16.Parameters.AddWithValue("@delivery_term", delivTerm);
                    cmd16.Parameters.AddWithValue("@max_price", "");
                    cmd16.ExecuteNonQuery();
                }

                var purObjects = GetElements(tender, "positions");
                purObjects.ForEach(po =>
                {
                    var okpdName = ((string) po.SelectToken(
                        "okpd2_name") ?? "").Trim();
                    var okpdCode = ((string) po.SelectToken(
                        "okpd2_code") ?? "").Trim();
                    var okei = ((string) po.SelectToken(
                        "okei_name") ?? "").Trim();
                    var quantity = (string) po.SelectToken(
                        "quantity") ?? "";
                    var price = (string) po.SelectToken(
                        "max_price") ?? "";
                    var sum = (string) po.SelectToken(
                        "max_cost") ?? "";
                    var poName = ((string) po.SelectToken(
                        "position_name") ?? "").Trim();
                    var insertLotitem =
                        $"INSERT INTO {Builder.Prefix}purchase_object SET id_lot = @id_lot, id_customer = @id_customer, name = @name, sum = @sum, okpd2_code = @okpd2_code, okpd2_group_code = @okpd2_group_code, okpd2_group_level1_code = @okpd2_group_level1_code, okpd_name = @okpd_name, quantity_value = @quantity_value, customer_quantity_value = @customer_quantity_value, okei = @okei, price = @price";
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
                TenderKwords(connect, idTender);
                AddVerNumber(connect, _tn.PurNum, TypeFz);
            }
        }

        private void WriteAttachments(MySqlConnection connect, List<JToken> attachments, int idTender)
        {
            foreach (var att in attachments)
            {
                var name = ((string) att.SelectToken("name") ?? "").Trim();
                var customName = ((string) att.SelectToken("custom_name") ?? "").Trim();
                if (customName != "")
                {
                    name = customName;
                }

                var webPath = ((string) att.SelectToken("link") ?? "").Trim();
                var description = ((string) att.SelectToken("descr") ?? "").Trim();
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(webPath))
                {
                    var insertAttach =
                        $"INSERT INTO {Builder.Prefix}attachment SET id_tender = @id_tender, file_name = @file_name, url = @url, description = @description";
                    var cmd10 = new MySqlCommand(insertAttach, connect);
                    cmd10.Prepare();
                    cmd10.Parameters.AddWithValue("@id_tender", idTender);
                    cmd10.Parameters.AddWithValue("@file_name", name);
                    cmd10.Parameters.AddWithValue("@url", webPath);
                    cmd10.Parameters.AddWithValue("@description", description);
                    cmd10.ExecuteNonQuery();
                }
            }
        }
    }
}