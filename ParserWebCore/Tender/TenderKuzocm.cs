using System;
using System.Data;
using HtmlAgilityPack;
using MySql.Data.MySqlClient;
using ParserWebCore.BuilderApp;
using ParserWebCore.Connections;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.TenderType;

namespace ParserWebCore.Tender
{
    public class TenderKuzocm : TenderAbstract, ITender
    {
        private readonly TypeKuzocm _tn;

        public TenderKuzocm(string etpName, string etpUrl, int typeFz, TypeKuzocm tn) : base(etpName, etpUrl,
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
            var navigator = (HtmlNodeNavigator) htmlDoc.CreateNavigator();
            var datePubT =
                (navigator.SelectSingleNode("//td[span[contains(., 'Начало подачи заявок')]]/following-sibling::td")
                    ?.Value ?? "").ReplaceHtmlEntyty().Trim();
            datePubT = datePubT.GetDateWithMonth();
            _tn.DatePub = datePubT.DelDoubleWhitespace().ParseDateUn("dd MM yyyy HH:mm");
            var dateEndT =
                (navigator.SelectSingleNode("//td[span[contains(., 'Окончание подачи заявок')]]/following-sibling::td")
                    ?.Value ?? "").ReplaceHtmlEntyty().Trim();
            dateEndT = dateEndT.GetDateWithMonth();
            _tn.DateEnd = dateEndT.DelDoubleWhitespace().GetDataFromRegex(@"(\d{2}\s\d{2}\s\d{4}\s\d{2}:\d{2})")
                .ParseDateUn("dd MM yyyy HH:mm");
            if (_tn.DatePub == DateTime.MinValue)
            {
                Log.Logger("Empty datePub", _tn.Href);
                return;
            }

            if (_tn.DateEnd == DateTime.MinValue)
            {
                Log.Logger("Empty dateEnd", _tn.Href);
                return;
            }

            var status =
                (navigator.SelectSingleNode("//td[contains(., 'Состояние лота')]/following-sibling::td")
                    ?.Value ?? "").ReplaceHtmlEntyty().Trim();
            using (var connect = ConnectToDb.GetDbConnection())
            {
                connect.Open();
                var selectTend =
                    $"SELECT id_tender FROM {Builder.Prefix}tender WHERE purchase_number = @purchase_number AND type_fz = @type_fz AND doc_publish_date = @doc_publish_date AND end_date = @end_date AND notice_version = @notice_version";
                var cmd = new MySqlCommand(selectTend, connect);
                cmd.Prepare();
                cmd.Parameters.AddWithValue("@purchase_number", _tn.PurNum);
                cmd.Parameters.AddWithValue("@type_fz", TypeFz);
                cmd.Parameters.AddWithValue("@doc_publish_date", _tn.DatePub);
                cmd.Parameters.AddWithValue("@end_date", _tn.DateEnd);
                cmd.Parameters.AddWithValue("@notice_version", status);
                var dt = new DataTable();
                var adapter = new MySqlDataAdapter {SelectCommand = cmd};
                adapter.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    return;
                }

                var dateUpd = DateTime.Now;
                var (updated, cancelStatus) = UpdateTenderVersion(connect, _tn.PurNum, dateUpd);
                var printForm = _tn.Href;
                var customerId = 0;
                var organiserId = 0;
                var orgName = EtpName;
                organiserId = AddOrganizer(orgName, connect, organiserId);
                PlacingWay = (navigator.SelectSingleNode("//td[contains(., 'Способ')]/following-sibling::td")
                    ?.Value ?? "").ReplaceHtmlEntyty().Trim();
                GetPlacingWay(connect, out var idPlacingWay);
                GetEtp(connect, out var idEtp);
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
                cmd9.Parameters.AddWithValue("@bidding_date", DateTime.MinValue);
                cmd9.Parameters.AddWithValue("@cancel", cancelStatus);
                cmd9.Parameters.AddWithValue("@date_version", dateUpd);
                cmd9.Parameters.AddWithValue("@num_version", 1);
                cmd9.Parameters.AddWithValue("@notice_version", status);
                cmd9.Parameters.AddWithValue("@xml", _tn.Href);
                cmd9.Parameters.AddWithValue("@print_form", printForm);
                var resInsertTender = cmd9.ExecuteNonQuery();
                var idTender = (int) cmd9.LastInsertedId;
                Counter(resInsertTender, updated);
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
                            $"INSERT INTO {Builder.Prefix}customer SET reg_num = @reg_num, full_name = @full_name, is223=1, inn = @inn";
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

                var docs = htmlDoc.DocumentNode.SelectNodes(
                               "//a[contains(@id, 'GenericLink') and @class = 'simpleLink']") ??
                           new HtmlNodeCollection(null);
                foreach (var doc in docs)
                {
                    var urlAttT = (doc?.Attributes["href"]?.Value ?? "").Trim();
                    var fName = doc.InnerHtml.Trim();
                    var urlAtt = $"https://etp.kuzocm.ru{urlAttT}";
                    if (!string.IsNullOrEmpty(fName))
                    {
                        var insertAttach =
                            $"INSERT INTO {Builder.Prefix}attachment SET id_tender = @id_tender, file_name = @file_name, url = @url";
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
                    $"INSERT INTO {Builder.Prefix}lot SET id_tender = @id_tender, lot_number = @lot_number, max_price = @max_price, currency = @currency, finance_source = @finance_source";
                var cmd18 = new MySqlCommand(insertLot, connect);
                cmd18.Prepare();
                cmd18.Parameters.AddWithValue("@id_tender", idTender);
                cmd18.Parameters.AddWithValue("@lot_number", lotNum);
                cmd18.Parameters.AddWithValue("@max_price", "");
                cmd18.Parameters.AddWithValue("@currency", "");
                cmd18.Parameters.AddWithValue("@finance_source", "");
                cmd18.ExecuteNonQuery();
                var idLot = (int) cmd18.LastInsertedId;
                var insertLotitem =
                    $"INSERT INTO {Builder.Prefix}purchase_object SET id_lot = @id_lot, id_customer = @id_customer, name = @name, quantity_value = @quantity_value, okei = @okei, customer_quantity_value = @customer_quantity_value, price = @price, sum = @sum";
                var cmd20 = new MySqlCommand(insertLotitem, connect);
                cmd20.Prepare();
                cmd20.Parameters.AddWithValue("@id_lot", idLot);
                cmd20.Parameters.AddWithValue("@id_customer", customerId);
                cmd20.Parameters.AddWithValue("@name", _tn.PurName);
                cmd20.Parameters.AddWithValue("@quantity_value", "");
                cmd20.Parameters.AddWithValue("@okei", "");
                cmd20.Parameters.AddWithValue("@customer_quantity_value", "");
                cmd20.Parameters.AddWithValue("@price", "");
                cmd20.Parameters.AddWithValue("@sum", "");
                cmd20.ExecuteNonQuery();
                var delivTerm1 = (navigator
                    .SelectSingleNode(
                        "//td[contains(., 'Требование обеспечения заявки на участие')]/following-sibling::td")
                    ?.Value ?? "").Trim();
                var delivTerm2 = (navigator
                    .SelectSingleNode(
                        "//td[contains(., 'Требование обеспечения исполнения договора')]/following-sibling::td")
                    ?.Value ?? "").Trim();
                var delivTerm =
                    $"Требование обеспечения заявки на участие: {delivTerm1}\nТребование обеспечения исполнения договора: {delivTerm2}"
                        .Trim();
                if (!string.IsNullOrEmpty(delivTerm))
                {
                    var insertCustomerRequirement =
                        $"INSERT INTO {Builder.Prefix}customer_requirement SET id_lot = @id_lot, id_customer = @id_customer, delivery_place = @delivery_place, max_price = @max_price, delivery_term = @delivery_term";
                    var cmd16 = new MySqlCommand(insertCustomerRequirement, connect);
                    cmd16.Prepare();
                    cmd16.Parameters.AddWithValue("@id_lot", idLot);
                    cmd16.Parameters.AddWithValue("@id_customer", customerId);
                    cmd16.Parameters.AddWithValue("@delivery_place", "");
                    cmd16.Parameters.AddWithValue("@max_price", "");
                    cmd16.Parameters.AddWithValue("@delivery_term", delivTerm);
                    cmd16.ExecuteNonQuery();
                }

                TenderKwords(connect, idTender);
                AddVerNumber(connect, _tn.PurNum, TypeFz);
            }
        }

        private static int AddOrganizer(string orgName, MySqlConnection connect, int organiserId)
        {
            if (!string.IsNullOrEmpty(orgName))
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
                    var inn = "";
                    var kpp = "";
                    var contactPerson = "";
                    var addOrganizer =
                        $"INSERT INTO {Builder.Prefix}organizer SET full_name = @full_name, contact_phone = @contact_phone, contact_person = @contact_person, contact_email = @contact_email, inn = @inn, kpp = @kpp";
                    var cmd4 = new MySqlCommand(addOrganizer, connect);
                    cmd4.Prepare();
                    cmd4.Parameters.AddWithValue("@full_name", orgName);
                    cmd4.Parameters.AddWithValue("@contact_phone", phone);
                    cmd4.Parameters.AddWithValue("@contact_person", contactPerson);
                    cmd4.Parameters.AddWithValue("@contact_email", email);
                    cmd4.Parameters.AddWithValue("@inn", inn);
                    cmd4.Parameters.AddWithValue("@kpp", kpp);
                    cmd4.ExecuteNonQuery();
                    organiserId = (int) cmd4.LastInsertedId;
                }
            }

            return organiserId;
        }
    }
}