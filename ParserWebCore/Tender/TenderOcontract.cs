using System;
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
    public class TenderOcontract : TenderAbstract, ITender
    {
        private readonly TypeOcontract _tn;

        public TenderOcontract(string etpName, string etpUrl, int typeFz, TypeOcontract tn) : base(etpName, etpUrl,
            typeFz)
        {
            _tn = tn;
            PlacingWay = _tn.PlacingWay;
        }

        public void ParsingTender()
        {
            using (var connect = ConnectToDb.GetDbConnection())
            {
                connect.Open();
                var selectTend =
                    $"SELECT id_tender FROM {Builder.Prefix}tender WHERE purchase_number = @purchase_number AND end_date = @end_date AND type_fz = @type_fz AND notice_version = @notice_version";
                var cmd = new MySqlCommand(selectTend, connect);
                cmd.Prepare();
                cmd.Parameters.AddWithValue("@purchase_number", _tn.PurNum);
                cmd.Parameters.AddWithValue("@end_date", _tn.DateEnd);
                cmd.Parameters.AddWithValue("@type_fz", TypeFz);
                cmd.Parameters.AddWithValue("@notice_version", _tn.Status);
                var dt = new DataTable();
                var adapter = new MySqlDataAdapter {SelectCommand = cmd};
                adapter.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    return;
                }

                var dateUpd = DateTime.Now;
                var cancelStatus = 0;
                var updated = false;
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
                    //DateTime dateNew = DateTime.Parse(pr.DatePublished);
                    updated = true;
                    if (dateUpd >= (DateTime) row["date_version"])
                    {
                        row["cancel"] = 1;
                        //row.AcceptChanges();
                        //row.SetModified();
                    }
                    else
                    {
                        cancelStatus = 1;
                    }
                }

                var commandBuilder =
                    new MySqlCommandBuilder(adapter2) {ConflictOption = ConflictOption.OverwriteChanges};
                adapter2.Update(dt2);
                var printForm = _tn.Href;
                var customerId = 0;
                var organiserId = 0;
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
                        var phone = "";
                        var email = "";
                        var inn = "";
                        var kpp = "";
                        var contactPerson = "";
                        var addOrganizer =
                            $"INSERT INTO {Builder.Prefix}organizer SET full_name = @full_name, contact_phone = @contact_phone, contact_person = @contact_person, contact_email = @contact_email, inn = @inn, kpp = @kpp";
                        var cmd4 = new MySqlCommand(addOrganizer, connect);
                        cmd4.Prepare();
                        cmd4.Parameters.AddWithValue("@full_name", _tn.OrgName);
                        cmd4.Parameters.AddWithValue("@contact_phone", phone);
                        cmd4.Parameters.AddWithValue("@contact_person", contactPerson);
                        cmd4.Parameters.AddWithValue("@contact_email", email);
                        cmd4.Parameters.AddWithValue("@inn", inn);
                        cmd4.Parameters.AddWithValue("@kpp", kpp);
                        cmd4.ExecuteNonQuery();
                        organiserId = (int) cmd4.LastInsertedId;
                    }
                }

                GetEtp(connect, out var idEtp);
                GetPlacingWay(connect, out var idPlacingWay);
                var insertTender =
                    $"INSERT INTO {Builder.Prefix}tender SET id_region = @id_region, id_xml = @id_xml, purchase_number = @purchase_number, doc_publish_date = @doc_publish_date, href = @href, purchase_object_info = @purchase_object_info, type_fz = @type_fz, id_organizer = @id_organizer, id_placing_way = @id_placing_way, id_etp = @id_etp, end_date = @end_date, scoring_date = @scoring_date, bidding_date = @bidding_date, cancel = @cancel, date_version = @date_version, num_version = @num_version, notice_version = @notice_version, xml = @xml, print_form = @print_form";
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
                cmd9.Parameters.AddWithValue("@bidding_date", _tn.BiddingDate);
                cmd9.Parameters.AddWithValue("@cancel", cancelStatus);
                cmd9.Parameters.AddWithValue("@date_version", dateUpd);
                cmd9.Parameters.AddWithValue("@num_version", 1);
                cmd9.Parameters.AddWithValue("@notice_version", _tn.Status);
                cmd9.Parameters.AddWithValue("@xml", _tn.Href);
                cmd9.Parameters.AddWithValue("@print_form", printForm);
                var resInsertTender = cmd9.ExecuteNonQuery();
                var idTender = (int) cmd9.LastInsertedId;
                Counter(resInsertTender, updated);
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
                            $"INSERT INTO {Builder.Prefix}customer SET reg_num = @reg_num, full_name = @full_name, is223=1, inn = @inn";
                        var cmd14 = new MySqlCommand(insertCustomer, connect);
                        cmd14.Prepare();
                        var customerRegNumber = Guid.NewGuid().ToString();
                        cmd14.Parameters.AddWithValue("@reg_num", customerRegNumber);
                        cmd14.Parameters.AddWithValue("@full_name", _tn.OrgName);
                        cmd14.Parameters.AddWithValue("@inn", "");
                        cmd14.ExecuteNonQuery();
                        customerId = (int) cmd14.LastInsertedId;
                    }
                }

                var lotNum = 1;
                var insertLot =
                    $"INSERT INTO {Builder.Prefix}lot SET id_tender = @id_tender, lot_number = @lot_number, max_price = @max_price, currency = @currency, finance_source = @finance_source";
                var cmd18 = new MySqlCommand(insertLot, connect);
                cmd18.Prepare();
                cmd18.Parameters.AddWithValue("@id_tender", idTender);
                cmd18.Parameters.AddWithValue("@lot_number", lotNum);
                cmd18.Parameters.AddWithValue("@max_price", _tn.Nmck);
                cmd18.Parameters.AddWithValue("@currency", _tn.Currency);
                cmd18.Parameters.AddWithValue("@finance_source", "");
                cmd18.ExecuteNonQuery();
                var idLot = (int) cmd18.LastInsertedId;
                var delivTerms =
                    $"Условия оплаты: {_tn.DeliveryTerms}\nВремя доставки: {_tn.DeliveryTime}\nУсловия оплаты: {_tn.PaymentTerms}\nФорма оплаты: {_tn.FormOfPayment}"
                        .ReplaceHtmlEntyty();
                var insertCustomerRequirement =
                    $"INSERT INTO {Builder.Prefix}customer_requirement SET id_lot = @id_lot, id_customer = @id_customer, delivery_place = @delivery_place, max_price = @max_price, delivery_term = @delivery_term";
                var cmd16 = new MySqlCommand(insertCustomerRequirement, connect);
                cmd16.Prepare();
                cmd16.Parameters.AddWithValue("@id_lot", idLot);
                cmd16.Parameters.AddWithValue("@id_customer", customerId);
                cmd16.Parameters.AddWithValue("@delivery_place", _tn.DeliveryPlace);
                cmd16.Parameters.AddWithValue("@max_price", "");
                cmd16.Parameters.AddWithValue("@delivery_term",
                    delivTerms);
                cmd16.ExecuteNonQuery();
                try
                {
                    addPurObjects(connect, idLot, customerId);
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }

                TenderKwords(connect, idTender);
                AddVerNumber(connect, _tn.PurNum, TypeFz);
            }
        }

        private void addPurObjects(MySqlConnection connect, int idLot, int customerId)
        {
            var url = $"https://api.onlc.ru/purchases/v1/public/procedures/{_tn.PurNum}/positions";
            var s = DownloadString.DownLUserAgent(url);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger("Empty string in Tender()", url);
            }

            var jObj = JObject.Parse(s);
            var tenders = GetElements(jObj, "data");
            tenders.ForEach(po =>
            {
                var poName = ((string) po.SelectToken("Name") ?? "").Trim();
                var cond = ((string) po.SelectToken("OwnerCondi") ?? "").Trim();
                var ovnerSrok = ((string) po.SelectToken("OwnerSrok") ?? "").Trim();
                var ovnerSklad = ((string) po.SelectToken("OwnerSklad") ?? "").Trim();
                poName =
                    $"{poName}\nТехнические характеристики товара: {cond}\nУсловия оплаты: {ovnerSrok}\nСрок поставки, наличие на складе: {ovnerSklad}"
                        .ReplaceHtmlEntyty();
                var okei = ((string) po.SelectToken("Metr") ?? "").Trim();
                var price = ((string) po.SelectToken("Price") ?? "").Trim();
                var sum = ((string) po.SelectToken("TotalPrice") ?? "").Trim();
                var quant = ((string) po.SelectToken("TCount") ?? "").Trim();
                var insertLotitem =
                    $"INSERT INTO {Builder.Prefix}purchase_object SET id_lot = @id_lot, id_customer = @id_customer, name = @name, sum = @sum, okpd2_code = @okpd2_code, okpd2_group_code = @okpd2_group_code, okpd2_group_level1_code = @okpd2_group_level1_code, okpd_name = @okpd_name, price = @price, quantity_value = @quantity_value, okei = @okei, customer_quantity_value = @customer_quantity_value";
                var cmd19 = new MySqlCommand(insertLotitem, connect);
                cmd19.Prepare();
                cmd19.Parameters.AddWithValue("@id_lot", idLot);
                cmd19.Parameters.AddWithValue("@id_customer", customerId);
                cmd19.Parameters.AddWithValue("@name", poName);
                cmd19.Parameters.AddWithValue("@sum", sum);
                cmd19.Parameters.AddWithValue("@okpd2_code", "");
                cmd19.Parameters.AddWithValue("@okpd2_group_code", "");
                cmd19.Parameters.AddWithValue("@okpd2_group_level1_code", "");
                cmd19.Parameters.AddWithValue("@okpd_name", "");
                cmd19.Parameters.AddWithValue("@price", price);
                cmd19.Parameters.AddWithValue("@quantity_value", quant);
                cmd19.Parameters.AddWithValue("@customer_quantity_value", quant);
                cmd19.Parameters.AddWithValue("@okei", okei);
                cmd19.ExecuteNonQuery();
            });
        }
    }
}