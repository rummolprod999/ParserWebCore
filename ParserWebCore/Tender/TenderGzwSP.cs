using System;
using System.Data;
using System.Net;
using System.Threading;
using HtmlAgilityPack;
using MySql.Data.MySqlClient;
using ParserWebCore.BuilderApp;
using ParserWebCore.Connections;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Parser;
using ParserWebCore.TenderType;

namespace ParserWebCore.Tender
{
    public class TenderGzwSp : TenderAbstract, ITender
    {
        private readonly Arguments _arg;
        private readonly TypeMzVoron _tn;
        private string _baseUrl;

        public TenderGzwSp(string etpName, string etpUrl, int typeFz, TypeMzVoron tn, string baseurl, Arguments arg) :
            base(etpName,
                etpUrl,
                typeFz)
        {
            _tn = tn;
            _baseUrl = baseurl;
            _arg = arg;
        }

        public void ParsingTender()
        {
            var dateUpd = DateTime.Now;
            int idRegion;
            using (var connect = ConnectToDb.GetDbConnection())
            {
                connect.Open();
                var selectTend =
                    $"SELECT id_tender FROM {AppBuilder.Prefix}tender WHERE purchase_number = @purchase_number AND end_date = @end_date AND type_fz = @type_fz AND doc_publish_date = @doc_publish_date AND notice_version = @notice_version";
                var cmd = new MySqlCommand(selectTend, connect);
                cmd.Prepare();
                cmd.Parameters.AddWithValue("@purchase_number", _tn.PurNum);
                cmd.Parameters.AddWithValue("@end_date", _tn.DateEnd);
                cmd.Parameters.AddWithValue("@type_fz", TypeFz);
                cmd.Parameters.AddWithValue("@doc_publish_date", _tn.DatePub);
                cmd.Parameters.AddWithValue("@notice_version", _tn.Status);
                var dt = new DataTable();
                var adapter = new MySqlDataAdapter { SelectCommand = cmd };
                adapter.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    //Log.Logger("This tender is exist in base", PurNum);
                    return;
                }

                Thread.Sleep(5000);
                string s;
                if (_arg == Arguments.Smol || _arg == Arguments.Ufin || _arg == Arguments.Kurg ||
                    _arg == Arguments.Udmurt || _arg == Arguments.Samar || _arg == Arguments.Kalug ||
                    _arg == Arguments.Dvina || _arg == Arguments.Mordov || _arg == Arguments.UdmurtProp ||
                    _arg == Arguments.Tver || _arg == Arguments.Tverzmo || _arg == Arguments.Mzvoron)
                {
                    var col = new CookieCollection();
                    col.Add(new Cookie("ebudget", ParserGzwSp.AuthCookieValue));
                    col.Add(new Cookie("ebudget_mz", ParserGzwSp.AuthCookieValue));
                    s = DownloadString.DownLHttpPostWithCookiesAll(_tn.Href, _baseUrl, col);
                    ;
                }
                else
                {
                    s = DownloadString.DownL(_tn.Href);
                }

                if (string.IsNullOrEmpty(s))
                {
                    Log.Logger("Empty string in ParsingTender()", _tn.Href);
                    return;
                }

                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(s);
                var navigator = (HtmlNodeNavigator)htmlDoc.CreateNavigator();
                var cancelStatus = 0;
                var updated = false;
                var selectDateT =
                    $"SELECT id_tender, date_version, cancel FROM {AppBuilder.Prefix}tender WHERE purchase_number = @purchase_number AND type_fz = @type_fz";
                var cmd2 = new MySqlCommand(selectDateT, connect);
                cmd2.Prepare();
                cmd2.Parameters.AddWithValue("@purchase_number", _tn.PurNum);
                cmd2.Parameters.AddWithValue("@type_fz", TypeFz);
                var adapter2 = new MySqlDataAdapter { SelectCommand = cmd2 };
                var dt2 = new DataTable();
                adapter2.Fill(dt2);
                foreach (DataRow row in dt2.Rows)
                {
                    //DateTime dateNew = DateTime.Parse(pr.DatePublished);
                    updated = true;
                    if (dateUpd >= (DateTime)row["date_version"])
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
                    new MySqlCommandBuilder(adapter2) { ConflictOption = ConflictOption.OverwriteChanges };
                adapter2.Update(dt2);
                var printForm = _tn.Href;
                var customerId = 0;
                var organiserId = 0;
                if (!string.IsNullOrEmpty(_tn.CusName))
                {
                    var selectOrg =
                        $"SELECT id_organizer FROM {AppBuilder.Prefix}organizer WHERE full_name = @full_name";
                    var cmd3 = new MySqlCommand(selectOrg, connect);
                    cmd3.Prepare();
                    cmd3.Parameters.AddWithValue("@full_name", _tn.CusName);
                    var dt3 = new DataTable();
                    var adapter3 = new MySqlDataAdapter { SelectCommand = cmd3 };
                    adapter3.Fill(dt3);
                    if (dt3.Rows.Count > 0)
                    {
                        organiserId = (int)dt3.Rows[0].ItemArray[0];
                    }
                    else
                    {
                        var phone = (navigator
                            .SelectSingleNode(
                                "//td[. = 'Телефон']/following-sibling::td")
                            ?.Value ?? "").Trim();
                        var email = (navigator
                            .SelectSingleNode(
                                "//td[. = 'Электронная почта']/following-sibling::td")
                            ?.Value ?? "").Trim();
                        var contactPerson = (navigator
                            .SelectSingleNode(
                                "//td[. = 'Контактное лицо']/following-sibling::td")
                            ?.Value ?? "").Trim();
                        var addOrganizer =
                            $"INSERT INTO {AppBuilder.Prefix}organizer SET full_name = @full_name, contact_phone = @contact_phone, contact_person = @contact_person, contact_email = @contact_email, inn = @inn";
                        var cmd4 = new MySqlCommand(addOrganizer, connect);
                        cmd4.Prepare();
                        cmd4.Parameters.AddWithValue("@full_name", _tn.CusName);
                        cmd4.Parameters.AddWithValue("@contact_phone", phone);
                        cmd4.Parameters.AddWithValue("@contact_person", contactPerson);
                        cmd4.Parameters.AddWithValue("@contact_email", email);
                        cmd4.Parameters.AddWithValue("@inn", _tn.CusInn);
                        cmd4.ExecuteNonQuery();
                        organiserId = (int)cmd4.LastInsertedId;
                    }
                }

                GetEtp(connect, out var idEtp);
                PlacingWay = (navigator
                    .SelectSingleNode(
                        "//td[. = 'Способ закупки']/following-sibling::td")
                    ?.Value ?? "").Trim();
                if (_arg == Arguments.Midural)
                {
                    PlacingWay = "коммерческое предложение";
                }

                GetPlacingWay(connect, out var idPlacingWay);
                switch (_arg)
                {
                    case Arguments.Tver:
                        idRegion = GetRegionFromString("твер", connect);
                        break;
                    case Arguments.Murman:
                        idRegion = GetRegionFromString("мурман", connect);
                        break;
                    case Arguments.Kalug:
                        idRegion = GetRegionFromString("калужск", connect);
                        break;
                    case Arguments.Smol:
                        idRegion = GetRegionFromString("смолен", connect);
                        break;
                    case Arguments.Samar:
                        idRegion = GetRegionFromString("самар", connect);
                        break;
                    case Arguments.Udmurt:
                    case Arguments.UdmurtProp:
                        idRegion = GetRegionFromString("удмурт", connect);
                        break;
                    case Arguments.Midural:
                        idRegion = GetRegionFromString("свердл", connect);
                        break;
                    case Arguments.Mordov:
                        idRegion = GetRegionFromString("мордов", connect);
                        break;
                    default:
                        idRegion = 0;
                        break;
                }

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
                cmd9.Parameters.AddWithValue("@print_form", printForm);
                var resInsertTender = cmd9.ExecuteNonQuery();
                var idTender = (int)cmd9.LastInsertedId;
                Counter(resInsertTender, updated);
                if (!string.IsNullOrEmpty(_tn.CusName))
                {
                    var selectCustomer =
                        $"SELECT id_customer FROM {AppBuilder.Prefix}customer WHERE full_name = @full_name";
                    var cmd13 = new MySqlCommand(selectCustomer, connect);
                    cmd13.Prepare();
                    cmd13.Parameters.AddWithValue("@full_name", _tn.CusName);
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
                        cmd14.Parameters.AddWithValue("@full_name", _tn.CusName);
                        cmd14.Parameters.AddWithValue("@inn", _tn.CusInn);
                        cmd14.ExecuteNonQuery();
                        customerId = (int)cmd14.LastInsertedId;
                    }
                }

                var docs = htmlDoc.DocumentNode.SelectNodes(
                               "//table[thead[tr[th[. = 'Прикрепленные документы']]]]/tbody//td/a") ??
                           new HtmlNodeCollection(null);
                foreach (var dd in docs)
                {
                    var urlAttT = (dd?.Attributes["href"]?.Value ?? "").Trim();
                    var fName = (dd?.InnerText ?? "").Trim();
                    var urlAtt = $"{_baseUrl}{urlAttT}";
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

                var lotNum = 1;
                var insertLot =
                    $"INSERT INTO {AppBuilder.Prefix}lot SET id_tender = @id_tender, lot_number = @lot_number, max_price = @max_price, currency = @currency, finance_source = @finance_source";
                var cmd18 = new MySqlCommand(insertLot, connect);
                cmd18.Prepare();
                cmd18.Parameters.AddWithValue("@id_tender", idTender);
                cmd18.Parameters.AddWithValue("@lot_number", lotNum);
                cmd18.Parameters.AddWithValue("@max_price", _tn.Nmck);
                cmd18.Parameters.AddWithValue("@currency", "");
                cmd18.Parameters.AddWithValue("@finance_source", "");
                cmd18.ExecuteNonQuery();
                var idLot = (int)cmd18.LastInsertedId;
                var delivPlace = (navigator
                    .SelectSingleNode(
                        "//td[contains(., 'Место доставки')]/following-sibling::td")
                    ?.Value ?? "").Trim();
                var delivTerm1 = (navigator
                    .SelectSingleNode(
                        "//td[contains(., 'Срок и условия')]/following-sibling::td")
                    ?.Value ?? "").Trim();
                var delivTerm2 = (navigator
                    .SelectSingleNode(
                        "//td[contains(., 'Сроки поставки товаров')]/following-sibling::td")
                    ?.Value ?? "").Trim();
                var delivTerm3 = (navigator
                    .SelectSingleNode(
                        "//td[contains(., 'Сведения о включенных')]/following-sibling::td")
                    ?.Value ?? "").Trim();
                var delivTerm = $"{delivTerm1}\n{delivTerm2}\n{delivTerm3}".Trim();
                if (!string.IsNullOrEmpty(delivTerm) || !string.IsNullOrEmpty(delivPlace))
                {
                    var insertCustomerRequirement =
                        $"INSERT INTO {AppBuilder.Prefix}customer_requirement SET id_lot = @id_lot, id_customer = @id_customer, delivery_place = @delivery_place, max_price = @max_price, delivery_term = @delivery_term";
                    var cmd16 = new MySqlCommand(insertCustomerRequirement, connect);
                    cmd16.Prepare();
                    cmd16.Parameters.AddWithValue("@id_lot", idLot);
                    cmd16.Parameters.AddWithValue("@id_customer", customerId);
                    cmd16.Parameters.AddWithValue("@delivery_place", delivPlace);
                    cmd16.Parameters.AddWithValue("@max_price", _tn.Nmck);
                    cmd16.Parameters.AddWithValue("@delivery_term", delivTerm);
                    cmd16.ExecuteNonQuery();
                }

                if (_arg == Arguments.Midural)
                {
                    var poList =
                        htmlDoc.DocumentNode.SelectNodes("//table[thead[tr[th[. = 'Количество']]]]/tbody/tr") ??
                        new HtmlNodeCollection(null);
                    if (poList.Count != 0)
                    {
                        //poList.RemoveAt(poList.Count - 1);
                        foreach (var pp in poList)
                        {
                            var namePo = (pp.SelectSingleNode(".//td[2]")
                                ?.InnerText ?? "").Trim();
                            if (string.IsNullOrEmpty(namePo))
                            {
                                continue;
                            }

                            var okeiP = (pp.SelectSingleNode(".//td[5]")
                                ?.InnerText ?? "").Trim();
                            var priceP = "";
                            var sumP = "";
                            var quantityP = (pp.SelectSingleNode(".//td[4]")
                                ?.InnerText.Replace(",", ".").DelAllWhitespace() ?? "").Trim();
                            var insertLotitem =
                                $"INSERT INTO {AppBuilder.Prefix}purchase_object SET id_lot = @id_lot, id_customer = @id_customer, name = @name, quantity_value = @quantity_value, okei = @okei, customer_quantity_value = @customer_quantity_value, price = @price, sum = @sum";
                            var cmd19 = new MySqlCommand(insertLotitem, connect);
                            cmd19.Prepare();
                            cmd19.Parameters.AddWithValue("@id_lot", idLot);
                            cmd19.Parameters.AddWithValue("@id_customer", customerId);
                            cmd19.Parameters.AddWithValue("@name", namePo);
                            cmd19.Parameters.AddWithValue("@quantity_value", quantityP);
                            cmd19.Parameters.AddWithValue("@okei", okeiP);
                            cmd19.Parameters.AddWithValue("@customer_quantity_value", quantityP);
                            cmd19.Parameters.AddWithValue("@price", priceP);
                            cmd19.Parameters.AddWithValue("@sum", sumP);
                            cmd19.ExecuteNonQuery();
                        }
                    }
                }
                else
                {
                    var poList =
                        htmlDoc.DocumentNode.SelectNodes("//table[thead[tr[th[. = 'Количество']]]]/tbody/tr") ??
                        new HtmlNodeCollection(null);
                    if (poList.Count != 0)
                    {
                        //poList.RemoveAt(poList.Count - 1);
                        foreach (var pp in poList)
                        {
                            var namePo = (pp.SelectSingleNode(".//td[2]")
                                ?.InnerText ?? "").Trim();
                            if (string.IsNullOrEmpty(namePo))
                            {
                                continue;
                            }

                            var okeiP = (pp.SelectSingleNode(".//td[3]")
                                ?.InnerText ?? "").Trim();
                            var priceP = (pp.SelectSingleNode(".//td[4]")
                                ?.InnerText.Replace(",", ".").DelAllWhitespace() ?? "").Trim();
                            var sumP = (pp.SelectSingleNode(".//td[6]")
                                ?.InnerText.Replace(",", ".").DelAllWhitespace() ?? "").Trim();
                            var quantityP = (pp.SelectSingleNode(".//td[5]")
                                ?.InnerText.Replace(",", ".").DelAllWhitespace() ?? "").Trim();
                            var insertLotitem =
                                $"INSERT INTO {AppBuilder.Prefix}purchase_object SET id_lot = @id_lot, id_customer = @id_customer, name = @name, quantity_value = @quantity_value, okei = @okei, customer_quantity_value = @customer_quantity_value, price = @price, sum = @sum";
                            var cmd19 = new MySqlCommand(insertLotitem, connect);
                            cmd19.Prepare();
                            cmd19.Parameters.AddWithValue("@id_lot", idLot);
                            cmd19.Parameters.AddWithValue("@id_customer", customerId);
                            cmd19.Parameters.AddWithValue("@name", namePo);
                            cmd19.Parameters.AddWithValue("@quantity_value", quantityP);
                            cmd19.Parameters.AddWithValue("@okei", okeiP);
                            cmd19.Parameters.AddWithValue("@customer_quantity_value", quantityP);
                            cmd19.Parameters.AddWithValue("@price", priceP);
                            cmd19.Parameters.AddWithValue("@sum", sumP);
                            cmd19.ExecuteNonQuery();
                        }
                    }
                }


                TenderKwords(connect, idTender);
                AddVerNumber(connect, _tn.PurNum, TypeFz);
            }
        }
    }
}