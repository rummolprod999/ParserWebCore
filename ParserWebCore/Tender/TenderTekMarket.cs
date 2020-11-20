using System;
using System.Data;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using MySql.Data.MySqlClient;
using ParserWebCore.BuilderApp;
using ParserWebCore.Connections;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.TenderType;

namespace ParserWebCore.Tender
{
    public class TenderTekMarket : TenderAbstract, ITender
    {
        private readonly TypeTekMarket _tn;

        public TenderTekMarket(string etpName, string etpUrl, int typeFz, TypeTekMarket tn) : base(etpName, etpUrl,
            typeFz)
        {
            _tn = tn;
            PlacingWay = tn.PwName;
        }

        public void ParsingTender()
        {
            var noticeVersion = _tn.Status;
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
                cmd.Parameters.AddWithValue("@notice_version", noticeVersion);
                var dt = new DataTable();
                var adapter = new MySqlDataAdapter {SelectCommand = cmd};
                adapter.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    //Log.Logger("This tender is exist in base", PurNum);
                    return;
                }

                var s = DownloadString.DownLTektorg(_tn.Href);
                if (String.IsNullOrEmpty(s))
                {
                    Log.Logger(
                        $"Empty string in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                        _tn.Href);
                    return;
                }

                var parser = new HtmlParser();
                var document = parser.Parse(s);
                var dateUpd = DateTime.Now;
                var (updated, cancelStatus) = UpdateTenderVersion(connect, _tn.PurNum, dateUpd);
                var printForm = _tn.Href;
                var customerId = 0;
                var organiserId = 0;
                organiserId = GetOrganizer(document, connect);
                GetPlacingWay(connect, out var idPlacingWay);
                GetEtp(connect, out var idEtp);
                var regPlace =
                    (document.QuerySelector("td:contains('Регион поставки:') + td")?.TextContent ?? "").Trim();
                var idRegion = GetRegionFromString(regPlace, connect);
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
                cmd9.Parameters.AddWithValue("@bidding_date", DateTime.MinValue);
                cmd9.Parameters.AddWithValue("@cancel", cancelStatus);
                cmd9.Parameters.AddWithValue("@date_version", dateUpd);
                cmd9.Parameters.AddWithValue("@num_version", 1);
                cmd9.Parameters.AddWithValue("@notice_version", noticeVersion);
                cmd9.Parameters.AddWithValue("@xml", _tn.Href);
                cmd9.Parameters.AddWithValue("@print_form", printForm);
                var resInsertTender = cmd9.ExecuteNonQuery();
                var idTender = (int) cmd9.LastInsertedId;
                Counter(resInsertTender, updated);
                var docs = document.QuerySelectorAll(
                    "#documentation > a");
                if (docs.Length == 0)
                {
                    docs = document.QuerySelectorAll(
                        "div.procedure__item--documents a");
                }

                GetDocs(docs, connect, idTender);
                GetLots(connect, idTender, customerId, document);
                TenderKwords(connect, idTender);
                AddVerNumber(connect, _tn.PurNum, TypeFz);
            }
        }

        private void GetLots(MySqlConnection connect, int idTender, int customerId, IHtmlDocument document)
        {
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
            var purObjs = document.QuerySelectorAll(
                "div.procedure__item table.tableUnit tbody tr");
            foreach (var purObj in purObjs)
            {
                var pN1 = (purObj.QuerySelector("td:nth-child(2)")?.TextContent ?? "").Trim();
                var pN2 = (purObj.QuerySelector("td:nth-child(5)")?.TextContent ?? "").Trim();
                var pN = $"{pN1} {pN2}".Trim();
                var okei = (purObj.QuerySelector("td:nth-child(3)")?.TextContent ?? "").Trim();
                var quant = (purObj.QuerySelector("td:nth-child(4)")?.TextContent ?? "").Trim();
                if (!string.IsNullOrEmpty(pN))
                {
                    var insertLotitem =
                        $"INSERT INTO {Builder.Prefix}purchase_object SET id_lot = @id_lot, id_customer = @id_customer, name = @name, quantity_value = @quantity_value, okei = @okei, customer_quantity_value = @customer_quantity_value";
                    var cmd19 = new MySqlCommand(insertLotitem, connect);
                    cmd19.Prepare();
                    cmd19.Parameters.AddWithValue("@id_lot", idLot);
                    cmd19.Parameters.AddWithValue("@id_customer", customerId);
                    cmd19.Parameters.AddWithValue("@name", pN);
                    cmd19.Parameters.AddWithValue("@quantity_value", quant);
                    cmd19.Parameters.AddWithValue("@okei", okei);
                    cmd19.Parameters.AddWithValue("@customer_quantity_value", quant);
                    cmd19.ExecuteNonQuery();
                }
            }

            var delivDate = (document.QuerySelector("td:contains('Дата поставки:') + td")?.TextContent ?? "").Trim();
            var delivPlace = (document.QuerySelector("td:contains('Адрес поставки:') + td")?.TextContent ?? "").Trim();
            var delivTerm =
                (document.QuerySelector("td:contains('Условия оплаты и доставки:') + td")?.TextContent ?? "").Trim();
            delivTerm = $"Дата поставки: {delivDate}\n{delivTerm}".Trim();
            var insertCustomerRequirement =
                $"INSERT INTO {Builder.Prefix}customer_requirement SET id_lot = @id_lot, id_customer = @id_customer, delivery_term = @delivery_term, delivery_place = @delivery_place";
            var cmd16 = new MySqlCommand(insertCustomerRequirement, connect);
            cmd16.Prepare();
            cmd16.Parameters.AddWithValue("@id_lot", idLot);
            cmd16.Parameters.AddWithValue("@id_customer", customerId);
            cmd16.Parameters.AddWithValue("@delivery_term", delivTerm);
            cmd16.Parameters.AddWithValue("@delivery_place", delivPlace);
            cmd16.ExecuteNonQuery();
        }

        private void GetDocs(IHtmlCollection<IElement> docs, MySqlConnection connect, int idTender)
        {
            foreach (var doc in docs)
            {
                var fName = (doc?.TextContent ?? "").Trim();
                var urlAttT = (doc?.GetAttribute("href") ?? "").Trim();
                var urlAtt = $"https://www.tektorg.ru{urlAttT}";
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
        }

        private int GetOrganizer(IHtmlDocument document, MySqlConnection connect)
        {
            var organiserId = 0;
            var orgFullName =
                (document.QuerySelector("td:contains('Наименование организатора:') +  td")?.TextContent ?? "")
                .Trim();
            if (!string.IsNullOrEmpty(orgFullName))
            {
                var selectOrg =
                    $"SELECT id_organizer FROM {Builder.Prefix}organizer WHERE full_name = @full_name";
                var cmd3 = new MySqlCommand(selectOrg, connect);
                cmd3.Prepare();
                cmd3.Parameters.AddWithValue("@full_name", orgFullName);
                var dt3 = new DataTable();
                var adapter3 = new MySqlDataAdapter {SelectCommand = cmd3};
                adapter3.Fill(dt3);
                if (dt3.Rows.Count > 0)
                {
                    organiserId = (int) dt3.Rows[0].ItemArray[0];
                }
                else
                {
                    var phone = (document.QuerySelector("td:contains('Контактный телефон:') +  td")?.TextContent ??
                                 "")
                        .Trim();
                    var email = (document.QuerySelector("td:contains('Адрес электронной почты:') +  td")
                            ?.TextContent ?? "")
                        .Trim();
                    var contactPerson =
                        (document.QuerySelector("td:contains('ФИО контактного лица:') +  td")?.TextContent ?? "")
                        .Trim();
                    var addOrganizer =
                        $"INSERT INTO {Builder.Prefix}organizer SET full_name = @full_name, contact_phone = @contact_phone, contact_person = @contact_person, contact_email = @contact_email";
                    var cmd4 = new MySqlCommand(addOrganizer, connect);
                    cmd4.Prepare();
                    cmd4.Parameters.AddWithValue("@full_name", orgFullName);
                    cmd4.Parameters.AddWithValue("@contact_phone", phone);
                    cmd4.Parameters.AddWithValue("@contact_person", contactPerson);
                    cmd4.Parameters.AddWithValue("@contact_email", email);
                    cmd4.ExecuteNonQuery();
                    organiserId = (int) cmd4.LastInsertedId;
                }
            }

            return organiserId;
        }
    }
}