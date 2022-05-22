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
    public class TenderZakupMos : TenderAbstract, ITender
    {
        private readonly TypeZakupMos _tn;

        public TenderZakupMos(string etpName, string etpUrl, int typeFz, TypeZakupMos tn) : base(etpName, etpUrl,
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

                if (_tn.NeedId != 0)
                {
                    ParserNeed(connect);
                }
                else if (_tn.TenderId != 0)
                {
                    ParserTender(connect);
                }
                else if (_tn.AuctionId != 0)
                {
                    ParserAuction(connect);
                }
            }
        }

        private void ParserNeed(MySqlConnection connect)
        {
            var url = $"https://old.zakupki.mos.ru/api/Cssp/Need/GetEntity?id={_tn.NeedId}";
            var s = DownloadString.DownLHttpPostWithCookiesB2b(url, cookie: null, useProxy: AppBuilder.UserProxy);
            if (s.Contains("\"tag\":\"nopermission\""))
            {
                url = $"https://zakupki.mos.ru/newapi/api/Need/Get?needId={_tn.NeedId}";
                s = DownloadString.DownLHttpPostWithCookiesB2b(url, cookie: null, useProxy: AppBuilder.UserProxy);
            }

            if (string.IsNullOrEmpty(s))
            {
                Log.Logger(
                    $"Empty string in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                    _tn.Href);
                return;
            }

            var tender = JObject.Parse(s);
            var purNum = _tn.PurNum;
            var noticeVersion = _tn.Status;
            var dateUpd = DateTime.Now;

            var (updated, cancelStatus) = UpdateTenderVersion(connect, purNum, dateUpd);
            var printForm = _tn.Href;
            var organiserId = 0;
            if (_tn.OrgName != "")
            {
                var selectOrg =
                    $"SELECT id_organizer FROM {AppBuilder.Prefix}organizer WHERE full_name = @full_name";
                var cmd3 = new MySqlCommand(selectOrg, connect);
                cmd3.Prepare();
                cmd3.Parameters.AddWithValue("@full_name", _tn.OrgName);
                var dt3 = new DataTable();
                var adapter3 = new MySqlDataAdapter { SelectCommand = cmd3 };
                adapter3.Fill(dt3);
                if (dt3.Rows.Count > 0)
                {
                    organiserId = (int)dt3.Rows[0].ItemArray[0];
                }
                else
                {
                    var phone = ((string)tender.SelectToken("contactPhone") ?? "").Trim();
                    var email = "";
                    var contactPerson = ((string)tender.SelectToken("contactPerson") ?? "").Trim();
                    var inn = _tn.OrgInn;
                    var kpp = ((string)tender.SelectToken("createdByCompany.kpp") ?? "").Trim();
                    var postAddr = ((string)tender.SelectToken("createdByCompany.legalAddress") ?? "").Trim();
                    var addOrganizer =
                        $"INSERT INTO {AppBuilder.Prefix}organizer SET full_name = @full_name, contact_phone = @contact_phone, contact_person = @contact_person, contact_email = @contact_email, inn = @inn, kpp = @kpp, post_address = @post_address";
                    var cmd4 = new MySqlCommand(addOrganizer, connect);
                    cmd4.Prepare();
                    cmd4.Parameters.AddWithValue("@full_name", _tn.OrgName);
                    cmd4.Parameters.AddWithValue("@contact_phone", phone);
                    cmd4.Parameters.AddWithValue("@contact_person", contactPerson);
                    cmd4.Parameters.AddWithValue("@contact_email", email);
                    cmd4.Parameters.AddWithValue("@inn", inn);
                    cmd4.Parameters.AddWithValue("@kpp", kpp);
                    cmd4.Parameters.AddWithValue("@post_address", postAddr);
                    cmd4.ExecuteNonQuery();
                    organiserId = (int)cmd4.LastInsertedId;
                }
            }

            var idPlacingWay = 0;
            GetEtp(connect, out var idEtp);
            var idRegion = GetRegionFromString(_tn.RegionName, connect);
            var insertTender =
                $"INSERT INTO {AppBuilder.Prefix}tender SET id_region = @id_region, id_xml = @id_xml, purchase_number = @purchase_number, doc_publish_date = @doc_publish_date, href = @href, purchase_object_info = @purchase_object_info, type_fz = @type_fz, id_organizer = @id_organizer, id_placing_way = @id_placing_way, id_etp = @id_etp, end_date = @end_date, scoring_date = @scoring_date, bidding_date = @bidding_date, cancel = @cancel, date_version = @date_version, num_version = @num_version, notice_version = @notice_version, xml = @xml, print_form = @print_form";
            var cmd9 = new MySqlCommand(insertTender, connect);
            cmd9.Prepare();
            cmd9.Parameters.AddWithValue("@id_region", idRegion);
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
            var idTender = (int)cmd9.LastInsertedId;
            Counter(resInsertTender, updated);
            var attacments = GetElements(tender, "files");
            WriteAttachments(connect, attacments, idTender);
            var lotNum = 1;
            var insertLot =
                $"INSERT INTO {AppBuilder.Prefix}lot SET id_tender = @id_tender, lot_number = @lot_number, max_price = @max_price, currency = @currency";
            var cmd18 = new MySqlCommand(insertLot, connect);
            cmd18.Prepare();
            cmd18.Parameters.AddWithValue("@id_tender", idTender);
            cmd18.Parameters.AddWithValue("@lot_number", lotNum);
            cmd18.Parameters.AddWithValue("@max_price", _tn.Nmck);
            cmd18.Parameters.AddWithValue("@currency", "");
            cmd18.ExecuteNonQuery();
            var idLot = (int)cmd18.LastInsertedId;
            _tn.Customers.ForEach(cus =>
            {
                var customerId = 0;
                if (!string.IsNullOrEmpty(cus.CusName))
                {
                    var selectCustomer =
                        $"SELECT id_customer FROM {AppBuilder.Prefix}customer WHERE full_name = @full_name";
                    var cmd13 = new MySqlCommand(selectCustomer, connect);
                    cmd13.Prepare();
                    cmd13.Parameters.AddWithValue("@full_name", cus.CusName);
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
                            $"INSERT INTO {AppBuilder.Prefix}customer SET reg_num = @reg_num, full_name = @full_name, inn = @inn, is223=1";
                        var cmd14 = new MySqlCommand(insertCustomer, connect);
                        cmd14.Prepare();
                        var customerRegNumber = Guid.NewGuid().ToString();
                        cmd14.Parameters.AddWithValue("@reg_num", customerRegNumber);
                        cmd14.Parameters.AddWithValue("@full_name", cus.CusName);
                        cmd14.Parameters.AddWithValue("@inn", cus.CusInn);
                        cmd14.ExecuteNonQuery();
                        customerId = (int)cmd14.LastInsertedId;
                    }
                }

                var delivPl =
                    ((string)tender.SelectToken(
                        "deliveryPlace") ?? "").Trim();
                var delivTerm =
                    ((string)tender.SelectToken(
                        "paymentTerms") ?? "").Trim();
                if (delivTerm != "" || delivPl != "")
                {
                    var insertCustomerRequirement =
                        $"INSERT INTO {AppBuilder.Prefix}customer_requirement SET id_lot = @id_lot, id_customer = @id_customer, max_price = @max_price, delivery_place = @delivery_place, delivery_term = @delivery_term";
                    var cmd16 = new MySqlCommand(insertCustomerRequirement, connect);
                    cmd16.Prepare();
                    cmd16.Parameters.AddWithValue("@id_lot", idLot);
                    cmd16.Parameters.AddWithValue("@id_customer", customerId);
                    cmd16.Parameters.AddWithValue("@delivery_place", delivPl);
                    cmd16.Parameters.AddWithValue("@delivery_term", delivTerm);
                    cmd16.Parameters.AddWithValue("@max_price", _tn.Nmck);
                    cmd16.ExecuteNonQuery();
                }

                var purObjects = GetElements(tender, "items");
                purObjects.ForEach(po =>
                {
                    var okpdName = ((string)po.SelectToken(
                        "okpd.name") ?? "").Trim();
                    var okpdCode = ((string)po.SelectToken(
                        "okpd.code") ?? "").Trim();
                    var okei = ((string)po.SelectToken(
                        "okei.name") ?? "").Trim();
                    var quantity = (decimal?)po.SelectToken(
                        "amount") ?? 0.0m;
                    var price = (decimal?)po.SelectToken(
                        "cost") ?? 0.0m;
                    var sum = (decimal?)po.SelectToken(
                        "totalCost") ?? 0.0m;
                    var poName = ((string)po.SelectToken(
                        "name") ?? "").Trim();
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
            });
            TenderKwords(connect, idTender);
            AddVerNumber(connect, purNum, TypeFz);
        }

        private void WriteAttachments(MySqlConnection connect, List<JToken> attachments, int idTender)
        {
            foreach (var att in attachments)
            {
                var name = ((string)att.SelectToken("fileStorage.fileName") ?? "").Trim();
                var webPath = ((string)att.SelectToken("fileStorage.fileUrl") ?? "").Trim();
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(webPath))
                {
                    var insertAttach =
                        $"INSERT INTO {AppBuilder.Prefix}attachment SET id_tender = @id_tender, file_name = @file_name, url = @url";
                    var cmd10 = new MySqlCommand(insertAttach, connect);
                    cmd10.Prepare();
                    cmd10.Parameters.AddWithValue("@id_tender", idTender);
                    cmd10.Parameters.AddWithValue("@file_name", name);
                    cmd10.Parameters.AddWithValue("@url", webPath);
                    cmd10.ExecuteNonQuery();
                }
            }
        }

        private void WriteAttachmentsAuction(MySqlConnection connect, List<JToken> attachments, int idTender)
        {
            foreach (var att in attachments)
            {
                var name = ((string)att.SelectToken("name") ?? "").Trim();
                var webPath = ((string)att.SelectToken("id") ?? "").Trim();
                webPath = $"https://old.zakupki.mos.ru/api/Core/FileStorage/GetDownload?id={webPath}";
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(webPath))
                {
                    var insertAttach =
                        $"INSERT INTO {AppBuilder.Prefix}attachment SET id_tender = @id_tender, file_name = @file_name, url = @url";
                    var cmd10 = new MySqlCommand(insertAttach, connect);
                    cmd10.Prepare();
                    cmd10.Parameters.AddWithValue("@id_tender", idTender);
                    cmd10.Parameters.AddWithValue("@file_name", name);
                    cmd10.Parameters.AddWithValue("@url", webPath);
                    cmd10.ExecuteNonQuery();
                }
            }
        }

        private void ParserTender(MySqlConnection connect)
        {
            var url = $"https://old.zakupki.mos.ru/api/Cssp/Tender/GetEntity?id={_tn.TenderId}";
            var s = DownloadString.DownLHttpPostWithCookiesB2b(url, cookie: null, useProxy: AppBuilder.UserProxy);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger(
                    $"Empty string in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                    _tn.Href);
                return;
            }

            var tender = JObject.Parse(s);
            var purNum = _tn.PurNum;
            var noticeVersion = _tn.Status;
            var dateUpd = DateTime.Now;

            var (updated, cancelStatus) = UpdateTenderVersion(connect, purNum, dateUpd);
            var printForm = _tn.Href;
            var organiserId = 0;
            if (_tn.Customers[0].CusName != "")
            {
                var selectOrg =
                    $"SELECT id_organizer FROM {AppBuilder.Prefix}organizer WHERE full_name = @full_name";
                var cmd3 = new MySqlCommand(selectOrg, connect);
                cmd3.Prepare();
                cmd3.Parameters.AddWithValue("@full_name", _tn.Customers[0].CusName);
                var dt3 = new DataTable();
                var adapter3 = new MySqlDataAdapter { SelectCommand = cmd3 };
                adapter3.Fill(dt3);
                if (dt3.Rows.Count > 0)
                {
                    organiserId = (int)dt3.Rows[0].ItemArray[0];
                }
                else
                {
                    var phone = ((string)tender.SelectToken("contactPhone") ?? "").Trim();
                    var email = "";
                    var contactPerson = ((string)tender.SelectToken("contactPerson") ?? "").Trim();
                    var inn = _tn.Customers[0].CusInn;
                    var kpp = ((string)tender.SelectToken("createdByCompany.kpp") ?? "").Trim();
                    var postAddr = ((string)tender.SelectToken("createdByCompany.legalAddress") ?? "").Trim();
                    var addOrganizer =
                        $"INSERT INTO {AppBuilder.Prefix}organizer SET full_name = @full_name, contact_phone = @contact_phone, contact_person = @contact_person, contact_email = @contact_email, inn = @inn, kpp = @kpp, post_address = @post_address";
                    var cmd4 = new MySqlCommand(addOrganizer, connect);
                    cmd4.Prepare();
                    cmd4.Parameters.AddWithValue("@full_name", _tn.Customers[0].CusName);
                    cmd4.Parameters.AddWithValue("@contact_phone", phone);
                    cmd4.Parameters.AddWithValue("@contact_person", contactPerson);
                    cmd4.Parameters.AddWithValue("@contact_email", email);
                    cmd4.Parameters.AddWithValue("@inn", inn);
                    cmd4.Parameters.AddWithValue("@kpp", kpp);
                    cmd4.Parameters.AddWithValue("@post_address", postAddr);
                    cmd4.ExecuteNonQuery();
                    organiserId = (int)cmd4.LastInsertedId;
                }
            }

            PlacingWay = ((string)tender.SelectToken("tenderType.name") ?? "").Trim();
            GetPlacingWay(connect, out var idPlacingWay);
            GetEtp(connect, out var idEtp);
            var idRegion = GetRegionFromString(_tn.RegionName, connect);
            var insertTender =
                $"INSERT INTO {AppBuilder.Prefix}tender SET id_region = @id_region, id_xml = @id_xml, purchase_number = @purchase_number, doc_publish_date = @doc_publish_date, href = @href, purchase_object_info = @purchase_object_info, type_fz = @type_fz, id_organizer = @id_organizer, id_placing_way = @id_placing_way, id_etp = @id_etp, end_date = @end_date, scoring_date = @scoring_date, bidding_date = @bidding_date, cancel = @cancel, date_version = @date_version, num_version = @num_version, notice_version = @notice_version, xml = @xml, print_form = @print_form";
            var cmd9 = new MySqlCommand(insertTender, connect);
            cmd9.Prepare();
            cmd9.Parameters.AddWithValue("@id_region", idRegion);
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
            var idTender = (int)cmd9.LastInsertedId;
            Counter(resInsertTender, updated);
            var attacments = GetElements(tender, "files");
            WriteAttachments(connect, attacments, idTender);
            var lots = GetElements(tender, "lot");
            var lotNum = 1;
            lots.ForEach(lot =>
            {
                var nmck = (decimal?)lot.SelectToken(
                    "initialSum") ?? _tn.Nmck;
                var currency = ((string)lot.SelectToken(
                    "currency.shortName") ?? "").Trim();
                var insertLot =
                    $"INSERT INTO {AppBuilder.Prefix}lot SET id_tender = @id_tender, lot_number = @lot_number, max_price = @max_price, currency = @currency";
                var cmd18 = new MySqlCommand(insertLot, connect);
                cmd18.Prepare();
                cmd18.Parameters.AddWithValue("@id_tender", idTender);
                cmd18.Parameters.AddWithValue("@lot_number", lotNum);
                cmd18.Parameters.AddWithValue("@max_price", nmck);
                cmd18.Parameters.AddWithValue("@currency", currency);
                cmd18.ExecuteNonQuery();
                var idLot = (int)cmd18.LastInsertedId;
                lotNum++;
                _tn.Customers.ForEach(cus =>
                {
                    var customerId = 0;
                    if (!string.IsNullOrEmpty(cus.CusName))
                    {
                        var selectCustomer =
                            $"SELECT id_customer FROM {AppBuilder.Prefix}customer WHERE full_name = @full_name";
                        var cmd13 = new MySqlCommand(selectCustomer, connect);
                        cmd13.Prepare();
                        cmd13.Parameters.AddWithValue("@full_name", cus.CusName);
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
                                $"INSERT INTO {AppBuilder.Prefix}customer SET reg_num = @reg_num, full_name = @full_name, inn = @inn, is223=1";
                            var cmd14 = new MySqlCommand(insertCustomer, connect);
                            cmd14.Prepare();
                            var customerRegNumber = Guid.NewGuid().ToString();
                            cmd14.Parameters.AddWithValue("@reg_num", customerRegNumber);
                            cmd14.Parameters.AddWithValue("@full_name", cus.CusName);
                            cmd14.Parameters.AddWithValue("@inn", cus.CusInn);
                            cmd14.ExecuteNonQuery();
                            customerId = (int)cmd14.LastInsertedId;
                        }
                    }


                    var purObjects = GetElements(lot, "lotSpecification");
                    purObjects.ForEach(po =>
                    {
                        var delivPl =
                            ((string)po.SelectToken(
                                "deliveryPlace") ?? "").Trim();
                        var delivTerm =
                            ((string)po.SelectToken(
                                "paymentTerms") ?? "").Trim();
                        if (delivTerm != "" || delivPl != "")
                        {
                            var insertCustomerRequirement =
                                $"INSERT INTO {AppBuilder.Prefix}customer_requirement SET id_lot = @id_lot, id_customer = @id_customer, max_price = @max_price, delivery_place = @delivery_place, delivery_term = @delivery_term";
                            var cmd16 = new MySqlCommand(insertCustomerRequirement, connect);
                            cmd16.Prepare();
                            cmd16.Parameters.AddWithValue("@id_lot", idLot);
                            cmd16.Parameters.AddWithValue("@id_customer", customerId);
                            cmd16.Parameters.AddWithValue("@delivery_place", delivPl);
                            cmd16.Parameters.AddWithValue("@delivery_term", delivTerm);
                            cmd16.Parameters.AddWithValue("@max_price", nmck);
                            cmd16.ExecuteNonQuery();
                        }

                        var okpdName = ((string)po.SelectToken(
                            "production.okpds[0].okpd.name") ?? "").Trim();
                        var okpdCode = ((string)po.SelectToken(
                            "production.okpds[0].okpd.code") ?? "").Trim();
                        var okei = ((string)po.SelectToken(
                            "okei.name") ?? "").Trim();
                        var quantity = (decimal?)po.SelectToken(
                            "quantity") ?? 0.0m;
                        var price = (decimal?)po.SelectToken(
                            "cost") ?? 0.0m;
                        var sum = (decimal?)po.SelectToken(
                            "amount") ?? 0.0m;
                        var poName = ((string)po.SelectToken(
                            "subject") ?? "").Trim();
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
                });
            });
            TenderKwords(connect, idTender);
            AddVerNumber(connect, purNum, TypeFz);
        }

        private void ParserAuction(MySqlConnection connect)
        {
            var url = $"https://zakupki.mos.ru/newapi/api/Auction/Get?auctionId={_tn.AuctionId}";
            var s = DownloadString.DownLHttpPostWithCookiesB2b(url, cookie: null, useProxy: AppBuilder.UserProxy);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger(
                    $"Empty string in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                    _tn.Href);
                return;
            }

            var tender = JObject.Parse(s);
            var purNum = _tn.PurNum;
            var noticeVersion = _tn.Status;
            var dateUpd = DateTime.Now;
            var (updated, cancelStatus) = UpdateTenderVersion(connect, purNum, dateUpd);
            var printForm = _tn.Href;
            var organiserId = 0;
            if (_tn.OrgName != "")
            {
                var selectOrg =
                    $"SELECT id_organizer FROM {AppBuilder.Prefix}organizer WHERE full_name = @full_name";
                var cmd3 = new MySqlCommand(selectOrg, connect);
                cmd3.Prepare();
                cmd3.Parameters.AddWithValue("@full_name", _tn.OrgName);
                var dt3 = new DataTable();
                var adapter3 = new MySqlDataAdapter { SelectCommand = cmd3 };
                adapter3.Fill(dt3);
                if (dt3.Rows.Count > 0)
                {
                    organiserId = (int)dt3.Rows[0].ItemArray[0];
                }
                else
                {
                    var phone = ((string)tender.SelectToken("contactPhone") ?? "").Trim();
                    var email = "";
                    var contactPerson = ((string)tender.SelectToken("contactPerson") ?? "").Trim();
                    var inn = _tn.OrgInn;
                    var kpp = ((string)tender.SelectToken("createdByCompany.kpp") ?? "").Trim();
                    var postAddr = ((string)tender.SelectToken("createdByCompany.legalAddress") ?? "").Trim();
                    var addOrganizer =
                        $"INSERT INTO {AppBuilder.Prefix}organizer SET full_name = @full_name, contact_phone = @contact_phone, contact_person = @contact_person, contact_email = @contact_email, inn = @inn, kpp = @kpp, post_address = @post_address";
                    var cmd4 = new MySqlCommand(addOrganizer, connect);
                    cmd4.Prepare();
                    cmd4.Parameters.AddWithValue("@full_name", _tn.OrgName);
                    cmd4.Parameters.AddWithValue("@contact_phone", phone);
                    cmd4.Parameters.AddWithValue("@contact_person", contactPerson);
                    cmd4.Parameters.AddWithValue("@contact_email", email);
                    cmd4.Parameters.AddWithValue("@inn", inn);
                    cmd4.Parameters.AddWithValue("@kpp", kpp);
                    cmd4.Parameters.AddWithValue("@post_address", postAddr);
                    cmd4.ExecuteNonQuery();
                    organiserId = (int)cmd4.LastInsertedId;
                }
            }

            var idPlacingWay = 0;
            GetEtp(connect, out var idEtp);
            var idRegion = GetRegionFromString(_tn.RegionName, connect);
            var insertTender =
                $"INSERT INTO {AppBuilder.Prefix}tender SET id_region = @id_region, id_xml = @id_xml, purchase_number = @purchase_number, doc_publish_date = @doc_publish_date, href = @href, purchase_object_info = @purchase_object_info, type_fz = @type_fz, id_organizer = @id_organizer, id_placing_way = @id_placing_way, id_etp = @id_etp, end_date = @end_date, scoring_date = @scoring_date, bidding_date = @bidding_date, cancel = @cancel, date_version = @date_version, num_version = @num_version, notice_version = @notice_version, xml = @xml, print_form = @print_form";
            var cmd9 = new MySqlCommand(insertTender, connect);
            cmd9.Prepare();
            cmd9.Parameters.AddWithValue("@id_region", idRegion);
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
            var idTender = (int)cmd9.LastInsertedId;
            Counter(resInsertTender, updated);
            var attacments = GetElements(tender, "files");
            WriteAttachmentsAuction(connect, attacments, idTender);
            var lotNum = 1;
            var insertLot =
                $"INSERT INTO {AppBuilder.Prefix}lot SET id_tender = @id_tender, lot_number = @lot_number, max_price = @max_price, currency = @currency";
            var cmd18 = new MySqlCommand(insertLot, connect);
            cmd18.Prepare();
            cmd18.Parameters.AddWithValue("@id_tender", idTender);
            cmd18.Parameters.AddWithValue("@lot_number", lotNum);
            cmd18.Parameters.AddWithValue("@max_price", _tn.Nmck);
            cmd18.Parameters.AddWithValue("@currency", "");
            cmd18.ExecuteNonQuery();
            var idLot = (int)cmd18.LastInsertedId;
            _tn.Customers.ForEach(cus =>
            {
                var customerId = 0;
                if (!string.IsNullOrEmpty(cus.CusName))
                {
                    var selectCustomer =
                        $"SELECT id_customer FROM {AppBuilder.Prefix}customer WHERE full_name = @full_name";
                    var cmd13 = new MySqlCommand(selectCustomer, connect);
                    cmd13.Prepare();
                    cmd13.Parameters.AddWithValue("@full_name", cus.CusName);
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
                            $"INSERT INTO {AppBuilder.Prefix}customer SET reg_num = @reg_num, full_name = @full_name, inn = @inn, is223=1";
                        var cmd14 = new MySqlCommand(insertCustomer, connect);
                        cmd14.Prepare();
                        var customerRegNumber = Guid.NewGuid().ToString();
                        cmd14.Parameters.AddWithValue("@reg_num", customerRegNumber);
                        cmd14.Parameters.AddWithValue("@full_name", cus.CusName);
                        cmd14.Parameters.AddWithValue("@inn", cus.CusInn);
                        cmd14.ExecuteNonQuery();
                        customerId = (int)cmd14.LastInsertedId;
                    }
                }

                var delivPl =
                    ((string)tender.SelectToken(
                        "deliveryPlace") ?? "").Trim();
                var delivTerm =
                    ((string)tender.SelectToken(
                        "paymentTerms") ?? "").Trim();
                if (delivTerm != "" || delivPl != "")
                {
                    var insertCustomerRequirement =
                        $"INSERT INTO {AppBuilder.Prefix}customer_requirement SET id_lot = @id_lot, id_customer = @id_customer, max_price = @max_price, delivery_place = @delivery_place, delivery_term = @delivery_term";
                    var cmd16 = new MySqlCommand(insertCustomerRequirement, connect);
                    cmd16.Prepare();
                    cmd16.Parameters.AddWithValue("@id_lot", idLot);
                    cmd16.Parameters.AddWithValue("@id_customer", customerId);
                    cmd16.Parameters.AddWithValue("@delivery_place", delivPl);
                    cmd16.Parameters.AddWithValue("@delivery_term", delivTerm);
                    cmd16.Parameters.AddWithValue("@max_price", _tn.Nmck);
                    cmd16.ExecuteNonQuery();
                }

                var purObjects = GetElements(tender, "items");
                purObjects.ForEach(po =>
                {
                    var okpdName = ((string)po.SelectToken(
                        "okpd.name") ?? "").Trim();
                    var okpdCode = ((string)po.SelectToken(
                        "okpd.code") ?? "").Trim();
                    var okei = ((string)po.SelectToken(
                        "okeiName") ?? "").Trim();
                    var quantity = (decimal?)po.SelectToken(
                        "currentValue") ?? 0.0m;
                    var price = (decimal?)po.SelectToken(
                        "costPerUnit") ?? 0.0m;
                    var sum = (decimal?)po.SelectToken(
                        "totalCost") ?? 0.0m;
                    var poName = ((string)po.SelectToken(
                        "name") ?? "").Trim();
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
            });
            TenderKwords(connect, idTender);
            AddVerNumber(connect, purNum, TypeFz);
        }
    }
}