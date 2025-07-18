#region

using System;
using System.Data;
using MySql.Data.MySqlClient;
using ParserWebCore.BuilderApp;
using ParserWebCore.Connections;
using ParserWebCore.TenderType;

#endregion

namespace ParserWebCore.Tender
{
    public class TenderRb2b : TenderAbstract, ITender
    {
        private readonly TypeRb2b _tn;

        public TenderRb2b(string etpName, string etpUrl, int typeFz, TypeRb2b tn) : base(etpName, etpUrl,
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
                    //Log.Logger("This tender is exist in base", PurNum);
                    return;
                }

                var dateUpd = DateTime.Now;
                var (updated, cancelStatus) = UpdateTenderVersion(connect, _tn.PurNum, dateUpd);
                var printForm = _tn.Href;
                var organiserId = 0;
                var orgName = ((string)_tn.JsonT.SelectToken("organizer.@Name") ?? "").Trim();
                if (orgName != "")
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
                        var phone = ((string)_tn.JsonT.SelectToken("organizer.@Phone") ?? "").Trim();
                        var email = ((string)_tn.JsonT.SelectToken("organizer.@Email") ?? "").Trim();
                        var contactPerson = "";
                        var inn = ((string)_tn.JsonT.SelectToken("organizer.@INN") ?? "").Trim();
                        var kpp = ((string)_tn.JsonT.SelectToken("organizer.@KPP") ?? "").Trim();
                        var postAddr = ((string)_tn.JsonT.SelectToken("organizer.@Address") ?? "").Trim();
                        var addOrganizer =
                            $"INSERT INTO {AppBuilder.Prefix}organizer SET full_name = @full_name, contact_phone = @contact_phone, contact_person = @contact_person, contact_email = @contact_email, inn = @inn, kpp = @kpp, post_address = @post_address";
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
                        organiserId = (int)cmd4.LastInsertedId;
                    }
                }

                GetEtp(connect, out var idEtp);
                PlacingWay = ((string)_tn.JsonT.SelectToken("@ContractType") ?? "").Trim();
                GetPlacingWay(connect, out var idPlacingWay);
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
                var attachments = GetElements(_tn.JsonT, "docs.doc");
                foreach (var att in attachments)
                {
                    var fName = ((string)att.SelectToken("@Name") ?? "").Trim();
                    var urlAtt = ((string)att.SelectToken("@Link") ?? "").Trim();
                    if (!string.IsNullOrEmpty(fName))
                    {
                        var insertAttach =
                            $"INSERT INTO {AppBuilder.Prefix}attachment SET id_tender = @id_tender, file_name = @file_name, url = @url";
                        var cmd10 = new MySqlCommand(insertAttach, connect);
                        cmd10.Prepare();
                        cmd10.Parameters.AddWithValue("@id_tender", idTender);
                        cmd10.Parameters.AddWithValue("@file_name", fName);
                        cmd10.Parameters.AddWithValue("@url", urlAtt);
                        cmd10.ExecuteNonQuery();
                    }
                }

                var lots = GetElements(_tn.JsonT, "lots.lot");
                var lotNum = 1;
                foreach (var lot in lots)
                {
                    var nmck = ((string)lot.SelectToken("@Price") ?? "").Trim();
                    var currency = ((string)lot.SelectToken("@Currency") ?? "").Trim();
                    var lotName = ((string)lot.SelectToken("@Name") ?? "").Trim();
                    var insertLot =
                        $"INSERT INTO {AppBuilder.Prefix}lot SET id_tender = @id_tender, lot_number = @lot_number, max_price = @max_price, currency = @currency, lot_name = @lot_name";
                    var cmd18 = new MySqlCommand(insertLot, connect);
                    cmd18.Prepare();
                    cmd18.Parameters.AddWithValue("@id_tender", idTender);
                    cmd18.Parameters.AddWithValue("@lot_number", lotNum);
                    cmd18.Parameters.AddWithValue("@max_price", nmck);
                    cmd18.Parameters.AddWithValue("@currency", currency);
                    cmd18.Parameters.AddWithValue("@lot_name", lotName);
                    cmd18.ExecuteNonQuery();
                    var idLot = (int)cmd18.LastInsertedId;
                    lotNum++;
                    var customerId = 0;
                    var cusName = ((string)lot.SelectToken("customer.@Name") ?? "").Trim();
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
                                $"INSERT INTO {AppBuilder.Prefix}customer SET reg_num = @reg_num, full_name = @full_name, inn = @inn, is223=1";
                            var cmd14 = new MySqlCommand(insertCustomer, connect);
                            cmd14.Prepare();
                            var customerRegNumber = Guid.NewGuid().ToString();
                            var inn = ((string)lot.SelectToken("customer.@INN") ?? "").Trim();
                            cmd14.Parameters.AddWithValue("@reg_num", customerRegNumber);
                            cmd14.Parameters.AddWithValue("@full_name", cusName);
                            cmd14.Parameters.AddWithValue("@inn", inn);
                            cmd14.ExecuteNonQuery();
                            customerId = (int)cmd14.LastInsertedId;
                        }
                    }

                    var purObs = GetElements(lot, "goods.good");
                    foreach (var po in purObs)
                    {
                        var poName = ((string)po.SelectToken("@Name") ?? "").Trim();
                        var poPrice = ((string)po.SelectToken("@Price") ?? "").Trim();
                        var poOkei = ((string)po.SelectToken("@OKEI") ?? "").Trim();
                        var poQuant = ((string)po.SelectToken("@Quantity") ?? "").Trim();
                        var poOkpd = ((string)po.SelectToken("@OKDP") ?? "").Trim();
                        var insertLotitem =
                            $"INSERT INTO {AppBuilder.Prefix}purchase_object SET id_lot = @id_lot, id_customer = @id_customer, name = @name, sum = @sum, okpd2_code = @okpd2_code, okpd2_group_code = @okpd2_group_code, okpd2_group_level1_code = @okpd2_group_level1_code, okpd_name = @okpd_name, price = @price, quantity_value = @quantity_value, okei = @okei, customer_quantity_value = @customer_quantity_value";
                        var cmd19 = new MySqlCommand(insertLotitem, connect);
                        cmd19.Prepare();
                        cmd19.Parameters.AddWithValue("@id_lot", idLot);
                        cmd19.Parameters.AddWithValue("@id_customer", customerId);
                        cmd19.Parameters.AddWithValue("@name", poName);
                        cmd19.Parameters.AddWithValue("@sum", nmck);
                        cmd19.Parameters.AddWithValue("@okpd2_code", poOkpd);
                        cmd19.Parameters.AddWithValue("@okpd2_group_code", "");
                        cmd19.Parameters.AddWithValue("@okpd2_group_level1_code", "");
                        cmd19.Parameters.AddWithValue("@okpd_name", "");
                        cmd19.Parameters.AddWithValue("@price", poPrice);
                        cmd19.Parameters.AddWithValue("@quantity_value", poQuant);
                        cmd19.Parameters.AddWithValue("@customer_quantity_value", poQuant);
                        cmd19.Parameters.AddWithValue("@okei", poOkei);
                        cmd19.ExecuteNonQuery();
                    }

                    if (purObs.Count == 0)
                    {
                        var insertLotitem =
                            $"INSERT INTO {AppBuilder.Prefix}purchase_object SET id_lot = @id_lot, id_customer = @id_customer, name = @name, sum = @sum, okpd2_code = @okpd2_code, okpd2_group_code = @okpd2_group_code, okpd2_group_level1_code = @okpd2_group_level1_code, okpd_name = @okpd_name";
                        var cmd19 = new MySqlCommand(insertLotitem, connect);
                        cmd19.Prepare();
                        cmd19.Parameters.AddWithValue("@id_lot", idLot);
                        cmd19.Parameters.AddWithValue("@id_customer", customerId);
                        cmd19.Parameters.AddWithValue("@name", lotName);
                        cmd19.Parameters.AddWithValue("@sum", nmck);
                        cmd19.Parameters.AddWithValue("@okpd2_code", "");
                        cmd19.Parameters.AddWithValue("@okpd2_group_code", "");
                        cmd19.Parameters.AddWithValue("@okpd2_group_level1_code", "");
                        cmd19.Parameters.AddWithValue("@okpd_name", "");
                        cmd19.ExecuteNonQuery();
                    }
                }

                TenderKwords(connect, idTender);
                AddVerNumber(connect, _tn.PurNum, TypeFz);
            }
        }
    }
}