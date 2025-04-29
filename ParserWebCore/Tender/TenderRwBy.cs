#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Xml.XPath;
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
    public class TenderRwBy : TenderAbstract, ITender
    {
        private readonly TypeRwBy _tn;

        public TenderRwBy(string etpName, string etpUrl, int typeFz, TypeRwBy tn) : base(etpName, etpUrl,
            typeFz)
        {
            _tn = tn;
        }

        public void ParsingTender()
        {
            var s = DownloadString.DownLUserAgent(_tn.Href);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger("Empty string in ParsingTender()", _tn.Href);
                return;
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(s);
            var navigator = (HtmlNodeNavigator)htmlDoc.CreateNavigator();
            var dateUpd = DateTime.Now;
            var dateEndT =
                navigator
                    .SelectSingleNode(
                        "//td[contains(text(),  'Дата и время окончания приема предложений')]/following-sibling::td")
                    ?.Value?.ReplaceHtmlEntyty()?.DelDoubleWhitespace().Trim() ?? throw new Exception(
                    $"cannot find dateEndT in {_tn.Href}");
            var dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy HH:mm");
            if (dateEnd == DateTime.MinValue)
            {
                Log.Logger("Empty dateEnd", _tn.Href);
                return;
            }

            using (var connect = ConnectToDb.GetDbConnection())
            {
                connect.Open();
                var selectTend =
                    $"SELECT id_tender FROM {AppBuilder.Prefix}tender WHERE purchase_number = @purchase_number AND end_date = @end_date AND type_fz = @type_fz AND doc_publish_date = @doc_publish_date";
                var cmd = new MySqlCommand(selectTend, connect);
                cmd.Prepare();
                cmd.Parameters.AddWithValue("@purchase_number", _tn.PurNum);
                cmd.Parameters.AddWithValue("@end_date", dateEnd);
                cmd.Parameters.AddWithValue("@type_fz", TypeFz);
                cmd.Parameters.AddWithValue("@doc_publish_date", _tn.DatePub);
                var dt = new DataTable();
                var adapter = new MySqlDataAdapter { SelectCommand = cmd };
                adapter.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    //Log.Logger("This tender is exist in base", PurNum);
                    return;
                }

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
                var inn = "";
                var orgName =
                    navigator.SelectSingleNode(
                            "//td[contains(text(),  'Полное наименование заказчика, место нахождения организации, УНП')]/following-sibling::td/text()[1]")
                        ?.Value?.ReplaceHtmlEntyty()?.Trim() ?? "";
                if (!string.IsNullOrEmpty(orgName) || orgName != "-")
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
                        inn = navigator.SelectSingleNode(
                                "//td[contains(text(), 'Полное наименование заказчика, место нахождения организации, УНП')]/following-sibling::td/text()[3]")
                            ?.Value?.Trim() ?? "";
                        inn = inn.GetDataFromRegex(@"(\d{9})");
                        var kpp = "";
                        var contactPerson = navigator.SelectSingleNode(
                                "//td[contains(text(), 'Фамилии, имена и отчества, номера телефонов работников заказчика')]/following-sibling::td")
                            ?.Value?.Trim() ?? "";
                        var address = navigator.SelectSingleNode(
                                "//td[contains(text(), 'Полное наименование заказчика, место нахождения организации, УНП')]/following-sibling::td/text()[2]")
                            ?.Value?.Trim() ?? "";
                        var addOrganizer =
                            $"INSERT INTO {AppBuilder.Prefix}organizer SET full_name = @full_name, contact_phone = @contact_phone, contact_person = @contact_person, contact_email = @contact_email, inn = @inn, kpp = @kpp, fact_address = @fact_address";
                        var cmd4 = new MySqlCommand(addOrganizer, connect);
                        cmd4.Prepare();
                        cmd4.Parameters.AddWithValue("@full_name", orgName);
                        cmd4.Parameters.AddWithValue("@contact_phone", phone);
                        cmd4.Parameters.AddWithValue("@contact_person", contactPerson);
                        cmd4.Parameters.AddWithValue("@contact_email", email);
                        cmd4.Parameters.AddWithValue("@inn", inn);
                        cmd4.Parameters.AddWithValue("@kpp", kpp);
                        cmd4.Parameters.AddWithValue("@fact_address", address);
                        cmd4.ExecuteNonQuery();
                        organiserId = (int)cmd4.LastInsertedId;
                    }
                }

                var idPlacingWay = 0;
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
                cmd9.Parameters.AddWithValue("@end_date", dateEnd);
                cmd9.Parameters.AddWithValue("@scoring_date", DateTime.MinValue);
                cmd9.Parameters.AddWithValue("@bidding_date", DateTime.MinValue);
                cmd9.Parameters.AddWithValue("@cancel", cancelStatus);
                cmd9.Parameters.AddWithValue("@date_version", dateUpd);
                cmd9.Parameters.AddWithValue("@num_version", 1);
                cmd9.Parameters.AddWithValue("@notice_version", "");
                cmd9.Parameters.AddWithValue("@xml", _tn.Href);
                cmd9.Parameters.AddWithValue("@print_form", printForm);
                var resInsertTender = cmd9.ExecuteNonQuery();
                var idTender = (int)cmd9.LastInsertedId;
                Counter(resInsertTender, updated);
                if (!string.IsNullOrEmpty(orgName) || orgName != "-")
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
                        cmd14.Parameters.AddWithValue("@inn", inn);
                        cmd14.ExecuteNonQuery();
                        customerId = (int)cmd14.LastInsertedId;
                    }
                }

                GetDocs(htmlDoc, connect, idTender);

                var requirements = new List<Req>();
                var req1 = navigator.SelectSingleNode(
                        "//td[contains(text(), 'Требования к составу участников')]/following-sibling::td")
                    ?.Value?.Trim() ?? "";
                var req2 = navigator.SelectSingleNode(
                        "//td[contains(text(), 'Квалификационные требования')]/following-sibling::td")
                    ?.Value?.Trim() ?? "";
                if (!string.IsNullOrEmpty(req1))
                {
                    requirements.Add(new Req { Name = "Требования к составу участников", Content = req1 });
                }

                if (!string.IsNullOrEmpty(req2))
                {
                    requirements.Add(new Req { Name = "Квалификационные требования", Content = req2 });
                }

                GetLots(htmlDoc, connect, idTender, requirements, customerId);
                TenderKwords(connect, idTender);
                AddVerNumber(connect, _tn.PurNum, TypeFz);
            }
        }

        private void GetLots(HtmlDocument htmlDoc, MySqlConnection connect, int idTender, List<Req> requirements,
            int customerId)
        {
            var lots = htmlDoc.DocumentNode.SelectNodes(
                           "//table[@id = 'lots_list']/tbody/tr[position() > 1 and starts-with(@id, 'lotRow')]") ??
                       new HtmlNodeCollection(null);
            foreach (var lot in lots)
            {
                var lotNav = (HtmlNodeNavigator)lot.CreateNavigator();
                var lotNumT = lotNav.SelectSingleNode(
                        "./td[1]")
                    ?.Value?.Trim() ?? "1";
                int.TryParse(lotNumT, out var lotNum);
                var finSource = lotNav.SelectSingleNode(
                        "./following-sibling::tr/th[contains(text(), 'Источник финансирования')]/following-sibling::td/div")
                    ?.Value?.Trim() ?? "";
                var insertLot =
                    $"INSERT INTO {AppBuilder.Prefix}lot SET id_tender = @id_tender, lot_number = @lot_number, max_price = @max_price, currency = @currency, finance_source = @finance_source";
                var cmd18 = new MySqlCommand(insertLot, connect);
                cmd18.Prepare();
                cmd18.Parameters.AddWithValue("@id_tender", idTender);
                cmd18.Parameters.AddWithValue("@lot_number", lotNum);
                cmd18.Parameters.AddWithValue("@max_price", "");
                cmd18.Parameters.AddWithValue("@currency", "");
                cmd18.Parameters.AddWithValue("@finance_source", finSource);
                cmd18.ExecuteNonQuery();
                var idLot = (int)cmd18.LastInsertedId;
                GetRequirements(requirements, connect, idLot);
                GetCusRequirements(lotNav, connect, idLot, customerId);
                GetPurchaseObjects(lotNav, connect, idLot, customerId);
            }
        }

        private void GetPurchaseObjects(HtmlNodeNavigator lotNav, MySqlConnection connect,
            int idLot,
            int customerId)
        {
            var okpd2 = (lotNav
                .SelectSingleNode(
                    "./following-sibling::tr/th[.contains(text(), 'Код ОКРБ')]/following-sibling::td")
                ?.Value ?? "").Trim();
            var purObjects = lotNav.Select(
                "./td[2]/text()");
            if (purObjects is null)
            {
                return;
            }

            foreach (XPathNavigator po in purObjects)
            {
                var namePo = po?.Value?.ReplaceHtmlEntyty()?.Trim() ?? "";
                var insertLotitem =
                    $"INSERT INTO {AppBuilder.Prefix}purchase_object SET id_lot = @id_lot, id_customer = @id_customer, name = @name, quantity_value = @quantity_value, okei = @okei, customer_quantity_value = @customer_quantity_value, price = @price, sum = @sum, okpd2_code = @okpd2_code";
                var cmd19 = new MySqlCommand(insertLotitem, connect);
                cmd19.Prepare();
                cmd19.Parameters.AddWithValue("@id_lot", idLot);
                cmd19.Parameters.AddWithValue("@id_customer", customerId);
                cmd19.Parameters.AddWithValue("@name", namePo);
                cmd19.Parameters.AddWithValue("@quantity_value", "");
                cmd19.Parameters.AddWithValue("@okei", "");
                cmd19.Parameters.AddWithValue("@customer_quantity_value", "");
                cmd19.Parameters.AddWithValue("@price", "");
                cmd19.Parameters.AddWithValue("@sum", "");
                cmd19.Parameters.AddWithValue("@okpd2_code", okpd2);
                cmd19.ExecuteNonQuery();
            }
        }

        private void GetCusRequirements(HtmlNodeNavigator lotNav, MySqlConnection connect, int idLot, int customerId)
        {
            var delivPlace = (lotNav
                .SelectSingleNode(
                    "./following-sibling::tr/th[contains(text(), 'Место поставки товара, выполнения работ, оказания услуг')]/following-sibling::td/div")
                ?.Value ?? "").ReplaceHtmlEntyty().Trim();
            var delivTerm = (lotNav
                .SelectSingleNode(
                    "./following-sibling::tr/th[contains(text(), 'Срок поставки')]/following-sibling::td")
                ?.Value ?? "").Trim();
            if (!string.IsNullOrEmpty(delivTerm) || !string.IsNullOrEmpty(delivPlace))
            {
                var insertCustomerRequirement =
                    $"INSERT INTO {AppBuilder.Prefix}customer_requirement SET id_lot = @id_lot, id_customer = @id_customer, delivery_place = @delivery_place, max_price = @max_price, delivery_term = @delivery_term";
                var cmd16 = new MySqlCommand(insertCustomerRequirement, connect);
                cmd16.Prepare();
                cmd16.Parameters.AddWithValue("@id_lot", idLot);
                cmd16.Parameters.AddWithValue("@id_customer", customerId);
                cmd16.Parameters.AddWithValue("@delivery_place", delivPlace);
                cmd16.Parameters.AddWithValue("@max_price", "");
                cmd16.Parameters.AddWithValue("@delivery_term", delivTerm);
                cmd16.ExecuteNonQuery();
            }
        }

        private void GetRequirements(IEnumerable<Req> requirements, MySqlConnection connect,
            int idLot)
        {
            foreach (var req in requirements)
            {
                var insertReq =
                    $"INSERT INTO {AppBuilder.Prefix}requirement SET id_lot = @id_lot, content = @content, name = @name";
                var cmd19 = new MySqlCommand(insertReq, connect);
                cmd19.Prepare();
                cmd19.Parameters.AddWithValue("@id_lot", idLot);
                cmd19.Parameters.AddWithValue("@content", req.Content);
                cmd19.Parameters.AddWithValue("@name", req.Name);
                cmd19.ExecuteNonQuery();
            }
        }

        private void GetDocs(HtmlDocument htmlDoc, MySqlConnection connect, int idTender)
        {
            var docs = htmlDoc.DocumentNode.SelectNodes(
                           "//td/p/a[starts-with(@href, '/uploads/userfiles/')]") ??
                       new HtmlNodeCollection(null);
            foreach (var doc in docs)
            {
                var urlAttT = (doc?.Attributes["href"]?.Value ?? "").Trim();
                var fName = doc?.InnerText?.Trim() ?? "";
                var urlAtt = $"https://www.rw.by{urlAttT}";
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

        private class Req
        {
            public string Name { get; set; }
            public string Content { get; set; }
        }
    }
}