#region

using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;
using MySql.Data.MySqlClient;
using ParserWebCore.BuilderApp;
using ParserWebCore.Connections;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.TenderType;

#endregion

namespace ParserWebCore.Tender
{
    public class TenderLenReg : TenderAbstract, ITender
    {
        private readonly TypeLenReg _tn;

        public TenderLenReg(string etpName, string etpUrl, int typeFz, TypeLenReg tn) : base(etpName, etpUrl,
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
                    $"SELECT id_tender FROM {AppBuilder.Prefix}tender WHERE purchase_number = @purchase_number AND type_fz = @type_fz AND end_date = @end_date AND date_version = @date_version";
                var cmd = new MySqlCommand(selectTend, connect);
                cmd.Prepare();
                cmd.Parameters.AddWithValue("@purchase_number", _tn.PurNum);
                cmd.Parameters.AddWithValue("@type_fz", TypeFz);
                cmd.Parameters.AddWithValue("@end_date", _tn.DateEnd);
                cmd.Parameters.AddWithValue("@date_version", _tn.DateUpd);
                var dt = new DataTable();
                var adapter = new MySqlDataAdapter { SelectCommand = cmd };
                adapter.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    return;
                }

                var s = DownloadString.DownL(_tn.Href, 5);
                if (string.IsNullOrEmpty(s))
                {
                    Log.Logger("Empty string in ParsingTender()", _tn.Href);
                    return;
                }

                Thread.Sleep(5000);
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(s);
                var navigator = (HtmlNodeNavigator)htmlDoc.CreateNavigator();
                var datePubT = htmlDoc.DocumentNode
                    .SelectSingleNode(
                        "//td[. = 'Дата публикации']/following-sibling::td")
                    ?.InnerText.Replace("МСК", "").Trim() ?? "";
                var datePub = datePubT.ParseDateUnRus();
                if (datePub != DateTime.MinValue)
                {
                    _tn.DatePub = datePub;
                }

                var dateUpd = _tn.DateUpd;
                var (updated, cancelStatus) = UpdateTenderVersion(connect, _tn.PurNum, dateUpd);
                var printForm = _tn.Href;
                var customerId = 0;
                var organiserId = 0;
                var orgName = EtpName;
                organiserId = AddOrganizer(orgName, connect, organiserId);
                GetEtp(connect, out var idEtp);
                GetPlacingWay(connect, out var idPlacingWay);
                var status = htmlDoc.DocumentNode
                    .SelectSingleNode(
                        "//td[. = 'Статус']/following-sibling::td")
                    ?.InnerText.Trim() ?? "";
                var idRegion = GetRegionFromString("ленинг", connect);
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
                cmd9.Parameters.AddWithValue("@notice_version", status);
                cmd9.Parameters.AddWithValue("@xml", _tn.Href);
                cmd9.Parameters.AddWithValue("@print_form", printForm);
                var resInsertTender = cmd9.ExecuteNonQuery();
                var idTender = (int)cmd9.LastInsertedId;
                Counter(resInsertTender, updated);
                AddAttachments(htmlDoc, connect, idTender);
                var cusName = htmlDoc.DocumentNode
                    .SelectSingleNode(
                        "//td[. = 'Заказчик']/following-sibling::td/a[starts-with(@href, '/Organization')]")
                    ?.InnerText.Trim() ?? "";
                var cusInn = htmlDoc.DocumentNode
                    .SelectSingleNode(
                        "//td[. = 'ИНН/КПП']/following-sibling::td")
                    ?.InnerText.Trim().GetDataFromRegex(@"^(\d+)/") ?? "";
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
                        cmd14.Parameters.AddWithValue("@inn", cusInn);
                        cmd14.ExecuteNonQuery();
                        customerId = (int)cmd14.LastInsertedId;
                    }
                }

                var nmck = htmlDoc.DocumentNode
                    .SelectSingleNode(
                        "//td[contains(., 'Сумма договора')]/following-sibling::td/span")
                    ?.InnerText.Trim().ReplaceHtmlEntyty().ExtractPriceNew() ?? "";
                var currency = "руб.";
                var lotNum = 1;
                var insertLot =
                    $"INSERT INTO {AppBuilder.Prefix}lot SET id_tender = @id_tender, lot_number = @lot_number, max_price = @max_price, currency = @currency, finance_source = @finance_source";
                var cmd18 = new MySqlCommand(insertLot, connect);
                cmd18.Prepare();
                cmd18.Parameters.AddWithValue("@id_tender", idTender);
                cmd18.Parameters.AddWithValue("@lot_number", lotNum);
                cmd18.Parameters.AddWithValue("@max_price", nmck);
                cmd18.Parameters.AddWithValue("@currency", currency);
                cmd18.Parameters.AddWithValue("@finance_source", "");
                cmd18.ExecuteNonQuery();
                var idLot = (int)cmd18.LastInsertedId;
                var poList =
                    htmlDoc.DocumentNode.SelectNodes(
                        "//div[@id = 'ProductRequestGrid']//tbody/tr") ??
                    new HtmlNodeCollection(null);
                if (poList.Count != 0)
                {
                    poList.RemoveAt(0);
                    foreach (var pp in poList)
                    {
                        var namePo = (pp.SelectSingleNode(".//td[1]/a")
                            ?.InnerText ?? "").Trim();
                        var okeiP = "";
                        var quantityP = (pp.SelectSingleNode(".//td[4]")
                            ?.InnerText ?? "").Trim();
                        quantityP = quantityP.Replace("&nbsp;", "").Replace(",", ".");
                        quantityP = Regex.Replace(quantityP, @"\s+", "");
                        var okpd2 = (pp.SelectSingleNode(".//td[2]")
                            ?.InnerText ?? "").Trim();
                        var price = (pp.SelectSingleNode(".//td[3]")
                            ?.InnerText ?? "").Trim().ReplaceHtmlEntyty().ExtractPriceNew();
                        var sum = (pp.SelectSingleNode(".//td[5]")
                            ?.InnerText ?? "").Trim().ReplaceHtmlEntyty().ExtractPriceNew();
                        var insertLotitem =
                            $"INSERT INTO {AppBuilder.Prefix}purchase_object SET id_lot = @id_lot, id_customer = @id_customer, name = @name, quantity_value = @quantity_value, okei = @okei, customer_quantity_value = @customer_quantity_value, okpd2_code = @okpd2_code, sum = @sum, price = @price";
                        var cmd19 = new MySqlCommand(insertLotitem, connect);
                        cmd19.Prepare();
                        cmd19.Parameters.AddWithValue("@id_lot", idLot);
                        cmd19.Parameters.AddWithValue("@id_customer", customerId);
                        cmd19.Parameters.AddWithValue("@name", namePo);
                        cmd19.Parameters.AddWithValue("@quantity_value", quantityP);
                        cmd19.Parameters.AddWithValue("@okei", okeiP);
                        cmd19.Parameters.AddWithValue("@customer_quantity_value", quantityP);
                        cmd19.Parameters.AddWithValue("@sum", sum);
                        cmd19.Parameters.AddWithValue("@okpd2_code", okpd2);
                        cmd19.Parameters.AddWithValue("@price", price);
                        cmd19.ExecuteNonQuery();
                    }
                }
                else
                {
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
                    cmd20.Parameters.AddWithValue("@sum", nmck);
                    cmd20.ExecuteNonQuery();
                }

                var id = _tn.Href.GetDataFromRegex(@"Index/(\d+)");
                var delivPlace =
                    DownloadString.DownL(
                            $"https://zakupki.lenreg.ru/ProductRequestGroup/GetDeliveryAddress?idGroup={id}",
                            5)
                        .Trim('"');
                var delivTerm1 = navigator.SelectSingleNode(
                                         "//td[. = 'Плановая дата заключения договора']/following-sibling::td")
                                     ?.Value?.Trim().DelDoubleWhitespace() ??
                                 "";
                var delivTerm2 = navigator.SelectSingleNode(
                                         "//td[. = 'Срок выполнения работ, оказания услуг, поставки товаров']/following-sibling::td")
                                     ?.Value?.Trim().DelDoubleWhitespace() ??
                                 "";
                var delivTerm = "";
                if (!string.IsNullOrEmpty(delivTerm1))
                {
                    delivTerm = $"{delivTerm}Плановая дата заключения договора: {delivTerm1}";
                }

                if (!string.IsNullOrEmpty(delivTerm2))
                {
                    delivTerm = $"{delivTerm}\nСрок выполнения работ, оказания услуг, поставки товаров: {delivTerm2}";
                }

                if (!string.IsNullOrEmpty(delivPlace) || !string.IsNullOrEmpty(delivTerm))
                {
                    var insertCustomerRequirement =
                        $"INSERT INTO {AppBuilder.Prefix}customer_requirement SET id_lot = @id_lot, id_customer = @id_customer, delivery_place = @delivery_place, max_price = @max_price, delivery_term = @delivery_term";
                    var cmd16 = new MySqlCommand(insertCustomerRequirement, connect);
                    cmd16.Prepare();
                    cmd16.Parameters.AddWithValue("@id_lot", idLot);
                    cmd16.Parameters.AddWithValue("@id_customer", customerId);
                    cmd16.Parameters.AddWithValue("@delivery_place", delivPlace.Trim());
                    cmd16.Parameters.AddWithValue("@max_price", nmck);
                    cmd16.Parameters.AddWithValue("@delivery_term", delivTerm.ReplaceHtmlEntyty().Trim());
                    cmd16.ExecuteNonQuery();
                }

                TenderKwords(connect, idTender);
                AddVerNumber(connect, _tn.PurNum, TypeFz);
            }
        }

        private void AddAttachments(HtmlDocument htmlDoc, MySqlConnection connect, int idTender)
        {
            var docs = htmlDoc.DocumentNode.SelectNodes(
                           "//a[contains(@href, 'files.lenreg.ru')]") ??
                       new HtmlNodeCollection(null);
            foreach (var dd in docs)
            {
                var urlAtt = (dd?.Attributes["href"]?.Value ?? "").Trim();
                var fName = (dd?.InnerText ?? "").Trim();
                if (string.IsNullOrEmpty(fName))
                {
                    continue;
                }

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

        private static int AddOrganizer(string orgName, MySqlConnection connect, int organiserId)
        {
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
                    var phone = "";
                    var email = "";
                    var inn = "";
                    var kpp = "";
                    var contactPerson = "";
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

            return organiserId;
        }
    }
}