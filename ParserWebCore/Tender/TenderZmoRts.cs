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
    public class TenderZmoRts : TenderAbstract, ITender
    {
        private readonly TypeZmoRts _tn;
        private readonly int _section;

        public TenderZmoRts(string etpName, string etpUrl, int typeFz, TypeZmoRts tn, int section) : base(etpName,
            etpUrl,
            typeFz)
        {
            _tn = tn;
            this._section = section;
        }

        public void ParsingTender()
        {
            using (var connect = ConnectToDb.GetDbConnection())
            {
                connect.Open();
                var selectTend =
                    $"SELECT id_tender FROM {Builder.Prefix}tender WHERE purchase_number = @purchase_number AND doc_publish_date = @doc_publish_date AND type_fz = @type_fz AND notice_version = @notice_version AND end_date = @end_date";
                var cmd = new MySqlCommand(selectTend, connect);
                cmd.Prepare();
                cmd.Parameters.AddWithValue("@purchase_number", _tn.Id);
                cmd.Parameters.AddWithValue("@doc_publish_date", _tn.PublicationDate);
                cmd.Parameters.AddWithValue("@type_fz", TypeFz);
                cmd.Parameters.AddWithValue("@notice_version", _tn.StateString);
                cmd.Parameters.AddWithValue("@end_date", _tn.EndDate);
                var dt = new DataTable();
                var adapter = new MySqlDataAdapter {SelectCommand = cmd};
                adapter.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    return;
                }

                var url = $"https://zmo-new-webapi.rts-tender.ru/market/api/v1/trades/{_tn.Id}";
                var s = DownloadString.DownLRtsZmo(url, null, _section);
                if (string.IsNullOrEmpty(s))
                {
                    Log.Logger(
                        $"Empty string in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                        url);
                    return;
                }

                var tender = JObject.Parse(s);
                var dateUpd = DateTime.Now;
                var (updated, cancelStatus) = UpdateTenderVersion(connect, _tn.Id, dateUpd);
                var organiserId = 0;
                var orgName = _tn.CusName;
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
                        var phone = ((string) tender.SelectToken("data.ContactPerson.Phone") ?? "").Trim();
                        var email = ((string) tender.SelectToken("data.ContactPerson.ContactEmail") ?? "").Trim();
                        var contactPerson = ((string) tender.SelectToken("data.ContactPerson.Name") ?? "").Trim();
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
                var regionName = ((string) tender.SelectToken("data.DeliveryKladrRegionName") ?? "").Trim();
                var idRegion = GetRegionFromString(regionName, connect);
                var href = "";
                switch (_section)
                {
                    case 162:
                        href = $"https://rzdshop.rts-tender.ru/search/plans/zapros/{_tn.Id}/request";
                        break;
                    case 168:
                        href = $"https://pochtashop.rts-tender.ru/zapros/{_tn.Id}/request";
                        break;
                    case 208:
                        href = $"https://kokb45shop.rts-tender.ru/zapros/{_tn.Id}/request";
                        break;
                    case 198:
                        href = $"https://fskshop.rts-tender.ru/search/plans/zapros/{_tn.Id}/request";
                        break;
                    case 220:
                        href = $"https://apteka74.rts-tender.ru/zapros/{_tn.Id}/request";
                        break;
                    case 190:
                        href = $"https://rusnanomarket.rts-tender.ru/zapros/{_tn.Id}/request";
                        break;
                    case 218:
                        href = $"https://ffoms.rts-tender.ru/zapros/{_tn.Id}/request";
                        break;
                    case 216:
                        href = $"https://gkb1-74.rts-tender.ru/zapros/{_tn.Id}/request";
                        break;
                    case 222:
                        href = $"https://rtmarket.rts-tender.ru/zapros/{_tn.Id}/request";
                        break;
                    default:
                        Log.Logger("Bad url", url);
                        return;
                }

                var insertTender =
                    $"INSERT INTO {Builder.Prefix}tender SET id_region = @id_region, id_xml = @id_xml, purchase_number = @purchase_number, doc_publish_date = @doc_publish_date, href = @href, purchase_object_info = @purchase_object_info, type_fz = @type_fz, id_organizer = @id_organizer, id_placing_way = @id_placing_way, id_etp = @id_etp, end_date = @end_date, scoring_date = @scoring_date, bidding_date = @bidding_date, cancel = @cancel, date_version = @date_version, num_version = @num_version, notice_version = @notice_version, xml = @xml, print_form = @print_form";
                var cmd9 = new MySqlCommand(insertTender, connect);
                cmd9.Prepare();
                cmd9.Parameters.AddWithValue("@id_region", idRegion);
                cmd9.Parameters.AddWithValue("@id_xml", _tn.Id);
                cmd9.Parameters.AddWithValue("@purchase_number", _tn.Id);
                cmd9.Parameters.AddWithValue("@doc_publish_date", _tn.PublicationDate);
                cmd9.Parameters.AddWithValue("@href", href);
                cmd9.Parameters.AddWithValue("@purchase_object_info", _tn.PurName);
                cmd9.Parameters.AddWithValue("@type_fz", TypeFz);
                cmd9.Parameters.AddWithValue("@id_organizer", organiserId);
                cmd9.Parameters.AddWithValue("@id_placing_way", idPlacingWay);
                cmd9.Parameters.AddWithValue("@id_etp", idEtp);
                cmd9.Parameters.AddWithValue("@end_date", _tn.EndDate);
                cmd9.Parameters.AddWithValue("@scoring_date", DateTime.MinValue);
                cmd9.Parameters.AddWithValue("@bidding_date", DateTime.MinValue);
                cmd9.Parameters.AddWithValue("@cancel", cancelStatus);
                cmd9.Parameters.AddWithValue("@date_version", dateUpd);
                cmd9.Parameters.AddWithValue("@num_version", 1);
                cmd9.Parameters.AddWithValue("@notice_version", _tn.StateString);
                cmd9.Parameters.AddWithValue("@xml", href);
                cmd9.Parameters.AddWithValue("@print_form", href);
                var resInsertTender = cmd9.ExecuteNonQuery();
                var idTender = (int) cmd9.LastInsertedId;
                Counter(resInsertTender, updated);
                var attacments = GetElements(tender, "data.OtherFiles");
                WriteAttachments(connect, attacments, idTender);
                var lotNum = 1;
                var insertLot =
                    $"INSERT INTO {Builder.Prefix}lot SET id_tender = @id_tender, lot_number = @lot_number, max_price = @max_price, currency = @currency";
                var cmd18 = new MySqlCommand(insertLot, connect);
                cmd18.Prepare();
                cmd18.Parameters.AddWithValue("@id_tender", idTender);
                cmd18.Parameters.AddWithValue("@lot_number", lotNum);
                cmd18.Parameters.AddWithValue("@max_price", _tn.Nmck);
                cmd18.Parameters.AddWithValue("@currency", "");
                cmd18.ExecuteNonQuery();
                var idLot = (int) cmd18.LastInsertedId;
                var customerId = 0;
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
                        cmd14.Parameters.AddWithValue("@reg_num", customerRegNumber);
                        cmd14.Parameters.AddWithValue("@full_name", _tn.CusName);
                        cmd14.Parameters.AddWithValue("@inn", "");
                        cmd14.ExecuteNonQuery();
                        customerId = (int) cmd14.LastInsertedId;
                    }
                }

                var delivTerm = ((string) tender.SelectToken("data.DeliveryTerms") ?? "").Trim();
                foreach (var dp in _tn.DeliveryKladrRegionName)
                {
                    if (delivTerm != "" || dp != "")
                    {
                        var insertCustomerRequirement =
                            $"INSERT INTO {Builder.Prefix}customer_requirement SET id_lot = @id_lot, id_customer = @id_customer, max_price = @max_price, delivery_place = @delivery_place, delivery_term = @delivery_term";
                        var cmd16 = new MySqlCommand(insertCustomerRequirement, connect);
                        cmd16.Prepare();
                        cmd16.Parameters.AddWithValue("@id_lot", idLot);
                        cmd16.Parameters.AddWithValue("@id_customer", customerId);
                        cmd16.Parameters.AddWithValue("@delivery_place", dp);
                        cmd16.Parameters.AddWithValue("@delivery_term", delivTerm);
                        cmd16.Parameters.AddWithValue("@max_price", _tn.Nmck);
                        cmd16.ExecuteNonQuery();
                    }
                }

                var purObjects = GetElements(tender, "data.Products");
                purObjects.ForEach(po =>
                {
                    var okpdName = "";
                    var okpdCode = ((string) po.SelectToken(
                        "ClassificatorCode") ?? "").Trim();
                    var okei = ((string) po.SelectToken(
                        "OkeiName") ?? "").Trim();
                    var quantity = (decimal?) po.SelectToken(
                        "Quantity") ?? 0.0m;
                    var price = (decimal?) po.SelectToken(
                        "Price") ?? 0.0m;
                    var sum = (decimal?) po.SelectToken(
                        "Sum") ?? 0.0m;
                    var poName = ((string) po.SelectToken(
                        "Name") ?? "").Trim();
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
                AddVerNumber(connect, _tn.Id, TypeFz);
            }
        }

        private void WriteAttachments(MySqlConnection connect, List<JToken> attachments, int idTender)
        {
            foreach (var att in attachments)
            {
                var name = ((string) att.SelectToken("Name") ?? "").Trim();
                var fileGuid = ((string) att.SelectToken("FileGuid") ?? "").Trim();
                var webPath = $"https://zmo-new-webapi.rts-tender.ru/market/api/v1/files//{fileGuid}";
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(webPath))
                {
                    var insertAttach =
                        $"INSERT INTO {Builder.Prefix}attachment SET id_tender = @id_tender, file_name = @file_name, url = @url";
                    var cmd10 = new MySqlCommand(insertAttach, connect);
                    cmd10.Prepare();
                    cmd10.Parameters.AddWithValue("@id_tender", idTender);
                    cmd10.Parameters.AddWithValue("@file_name", name);
                    cmd10.Parameters.AddWithValue("@url", webPath);
                    cmd10.ExecuteNonQuery();
                }
            }
        }
    }
}