#region

using System;
using System.Data;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using ParserWebCore.BuilderApp;
using ParserWebCore.Connections;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.TenderType;

#endregion

namespace ParserWebCore.Tender
{
    public class TenderSamolet : TenderAbstract, ITender
    {
        private readonly TypeSamolet _tn;

        public TenderSamolet(string etpName, string etpUrl, int typeFz, TypeSamolet tn) : base(etpName, etpUrl, typeFz)
        {
            _tn = tn;
        }

        public void ParsingTender()
        {
            using (var connect = ConnectToDb.GetDbConnection())
            {
                connect.Open();
                var selectTend =
                    $"SELECT id_tender FROM {AppBuilder.Prefix}tender WHERE purchase_number = @purchase_number AND type_fz = @type_fz AND end_date = @end_date";
                var cmd = new MySqlCommand(selectTend, connect);
                cmd.Prepare();
                cmd.Parameters.AddWithValue("@purchase_number", _tn.PurNum);
                cmd.Parameters.AddWithValue("@type_fz", TypeFz);
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
                var result = DownloadString.DownLUserAgent($"https://partner.samolet.ru/api/tender/tenders/{_tn.Id}/");
                var t = JObject.Parse(result);
                var organiserId = 0;
                var orgName = ((string)t.SelectToken("authorities[0].name") ?? "").Trim();
                if (!string.IsNullOrEmpty(orgName))
                {
                    var selectOrg =
                        $"SELECT id_organizer FROM {AppBuilder.Prefix}organizer WHERE full_name = @full_name";
                    var cmd3 = new MySqlCommand(selectOrg, connect);
                    cmd3.Prepare();
                    cmd3.Parameters.AddWithValue("@full_name", orgName);
                    var dt3 = new DataTable();
                    var adapter3 = new MySqlDataAdapter { SelectCommand = cmd3 };
                    adapter3.Fill(dt3);
                    if (dt3.Rows.Count > 0)
                    {
                        organiserId = (int)dt3.Rows[0].ItemArray[0];
                    }
                    else
                    {
                        var phone = ((string)t.SelectToken("phone") ?? "").Trim();
                        var email = ((string)t.SelectToken("email") ?? "").Trim();
                        var inn = ((string)t.SelectToken("authorities[0].inn") ?? "").Trim();
                        var kpp = ((string)t.SelectToken("authorities[0].kpp") ?? "").Trim();
                        var contactPerson = ((string)t.SelectToken("position") ?? "").Trim();
                        var addOrganizer =
                            $"INSERT INTO {AppBuilder.Prefix}organizer SET full_name = @full_name, contact_phone = @contact_phone, contact_person = @contact_person, contact_email = @contact_email, inn = @inn, kpp = @kpp";
                        var cmd4 = new MySqlCommand(addOrganizer, connect);
                        cmd4.Prepare();
                        cmd4.Parameters.AddWithValue("@full_name", orgName);
                        cmd4.Parameters.AddWithValue("@contact_phone", phone);
                        cmd4.Parameters.AddWithValue("@contact_person", contactPerson);
                        cmd4.Parameters.AddWithValue("@contact_email", email);
                        cmd4.Parameters.AddWithValue("@inn", inn);
                        cmd4.Parameters.AddWithValue("@kpp", kpp);
                        cmd4.ExecuteNonQuery();
                        organiserId = (int)cmd4.LastInsertedId;
                    }
                }

                GetEtp(connect, out var idEtp);
                var reg = ((string)t.SelectToken("regions[0].name") ?? "").Trim();
                var idRegion = GetRegionFromString(reg, connect);
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
                cmd9.Parameters.AddWithValue("@id_placing_way", 0);
                cmd9.Parameters.AddWithValue("@id_etp", idEtp);
                cmd9.Parameters.AddWithValue("@end_date", _tn.DateEnd);
                cmd9.Parameters.AddWithValue("@scoring_date", DateTime.MinValue);
                cmd9.Parameters.AddWithValue("@bidding_date", DateTime.MinValue);
                cmd9.Parameters.AddWithValue("@cancel", cancelStatus);
                cmd9.Parameters.AddWithValue("@date_version", dateUpd);
                cmd9.Parameters.AddWithValue("@num_version", 1);
                cmd9.Parameters.AddWithValue("@notice_version", "");
                cmd9.Parameters.AddWithValue("@xml", _tn.Href);
                cmd9.Parameters.AddWithValue("@print_form", _tn.Href);
                var resInsertTender = cmd9.ExecuteNonQuery();
                var idTender = (int)cmd9.LastInsertedId;
                Counter(resInsertTender, updated);
                var attacments = GetElements(t, "planningDocumentation");
                attacments.AddRange(GetElements(t, "draftOfContract"));
                attacments.AddRange(GetElements(t, "additionalDocumentation"));
                foreach (var att in attacments)
                {
                    var name = ((string)att.SelectToken("name") ?? "").Trim();
                    var url = ((string)att.SelectToken("document") ?? "").Trim();
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

                var customerId = 0;
                if (!string.IsNullOrEmpty(orgName))
                {
                    var selectCustomer =
                        $"SELECT id_customer FROM {AppBuilder.Prefix}customer WHERE full_name = @full_name";
                    var cmd13 = new MySqlCommand(selectCustomer, connect);
                    cmd13.Prepare();
                    cmd13.Parameters.AddWithValue("@full_name", orgName);
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
                        cmd14.Parameters.AddWithValue("@full_name", orgName);
                        cmd14.Parameters.AddWithValue("@inn", "");
                        cmd14.ExecuteNonQuery();
                        customerId = (int)cmd14.LastInsertedId;
                    }
                }

                var lots = GetElements(t, "lots");
                for (var i = 0; i < lots.Count; i++)
                {
                    var lotNum = i + 1;
                    var lotName = ((string)lots[i].SelectToken("name") ?? "").Trim();
                    var insertLot =
                        $"INSERT INTO {AppBuilder.Prefix}lot SET id_tender = @id_tender, lot_number = @lot_number, max_price = @max_price, currency = @currency, finance_source = @finance_source, lot_name = @lot_name";
                    var cmd18 = new MySqlCommand(insertLot, connect);
                    cmd18.Prepare();
                    cmd18.Parameters.AddWithValue("@id_tender", idTender);
                    cmd18.Parameters.AddWithValue("@lot_number", lotNum);
                    cmd18.Parameters.AddWithValue("@max_price", "");
                    cmd18.Parameters.AddWithValue("@currency", "");
                    cmd18.Parameters.AddWithValue("@finance_source", "");
                    cmd18.Parameters.AddWithValue("@lot_name", lotName);
                    cmd18.ExecuteNonQuery();
                    var idLot = (int)cmd18.LastInsertedId;
                    var purObjects = GetElements(lots[i], "materials");
                    purObjects.ForEach(po =>
                    {
                        var okpdName = ((string)po.SelectToken(
                            "okpd.name") ?? "").Trim();
                        var okpdCode = ((string)po.SelectToken(
                            "okpd.code") ?? "").Trim();
                        var okei = ((string)po.SelectToken(
                            "unit") ?? "").Trim();
                        var quantity = (string)po.SelectToken(
                            "volume") ?? "";
                        var price = (string)po.SelectToken(
                            "price") ?? "";
                        var sum = (string)po.SelectToken(
                            "max_cost") ?? "";
                        var poName = ((string)po.SelectToken(
                            "materialName") ?? "").Trim();
                        var descr = ((string)po.SelectToken(
                            "classifier") ?? "").Trim();
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
                    var delivTerm = ((string)t.SelectToken("deliveryNotes") ?? "").Trim();
                    if (!string.IsNullOrEmpty(delivTerm))
                    {
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
                    }
                }

                TenderKwords(connect, idTender);
                AddVerNumber(connect, _tn.PurNum, TypeFz);
            }
        }
    }
}