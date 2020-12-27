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
    public class TenderB2BWeb : TenderAbstract, ITender
    {
        private readonly TypeB2B _tn;

        public TenderB2BWeb(string etpName, string etpUrl, int typeFz, TypeB2B tn) : base(etpName, etpUrl,
            typeFz)
        {
            _tn = tn;
            PlacingWay = tn.PwName;
        }

        public void ParsingTender()
        {
            using (var connect = ConnectToDb.GetDbConnection())
            {
                var dateUpd = DateTime.Now;
                connect.Open();
                if (TenderExist(connect)) return;
                var s = DownloadString.DownLUserAgent(_tn.Href);
                if (string.IsNullOrEmpty(s))
                {
                    Log.Logger("Empty string in ParsingTender()", _tn.Href);
                    return;
                }

                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(s);
                var navigator = (HtmlNodeNavigator) htmlDoc.CreateNavigator();
                UpdateCancelStatus(connect, dateUpd, out var updated, out var cancelStatus);
                var printForm = _tn.Href;
                AddOrganizer(connect, navigator, out var organiserId);
                AddCustomer(connect, out var customerId);
                GetEtp(connect, out var idEtp);
                GetPlacingWay(connect, out var idPlacingWay);
                FillPurName(navigator);
                FillBidAndScorDates(navigator, out var scoringDate, out var biddingDate);
                FillNoticeVer(navigator, out var noticeVer);
                AddTender(connect, organiserId, idPlacingWay, idEtp, scoringDate, biddingDate, cancelStatus, dateUpd,
                    noticeVer, printForm, updated, out var idTender);
                AddAttachments(htmlDoc, connect, idTender);
            }
        }

        private static void AddAttachments(HtmlDocument htmlDoc, MySqlConnection connect, int idTender)
        {
            var docs = htmlDoc.DocumentNode.SelectNodes(
                           "//a[contains(@href, 'https://www.b2b-center.ru/download.html')]") ??
                       new HtmlNodeCollection(null);
            foreach (var dd in docs)
            {
                var urlAtt = (dd?.Attributes["href"]?.Value ?? "").Trim();
                var fName = (dd?.InnerText ?? "").Trim();
                if (string.IsNullOrEmpty(fName)) continue;
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

        private void AddTender(MySqlConnection connect, int organiserId, int idPlacingWay, int idEtp,
            DateTime scoringDate,
            DateTime biddingDate, int cancelStatus, DateTime dateUpd, string noticeVer, string printForm, bool updated,
            out int idTender)
        {
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
            cmd9.Parameters.AddWithValue("@scoring_date", scoringDate);
            cmd9.Parameters.AddWithValue("@bidding_date", biddingDate);
            cmd9.Parameters.AddWithValue("@cancel", cancelStatus);
            cmd9.Parameters.AddWithValue("@date_version", dateUpd);
            cmd9.Parameters.AddWithValue("@num_version", 1);
            cmd9.Parameters.AddWithValue("@notice_version", noticeVer);
            cmd9.Parameters.AddWithValue("@xml", _tn.Href);
            cmd9.Parameters.AddWithValue("@print_form", printForm);
            var resInsertTender = cmd9.ExecuteNonQuery();
            idTender = (int) cmd9.LastInsertedId;
            Counter(resInsertTender, updated);
        }

        private static void FillNoticeVer(HtmlNodeNavigator navigator, out string noticeVer)
        {
            var comments = navigator.SelectSingleNode(
                                   "//td[b[. = 'Комментарии:']]")
                               ?.Value?.Trim() ??
                           "";
            var providingDocumentation = navigator.SelectSingleNode(
                                                 "//td[contains(., 'Порядок предоставления документации по закупке:')]/following-sibling::td")
                                             ?.Value?.Trim() ??
                                         "";
            noticeVer = $"{comments}\nПорядок предоставления документации по закупке: {providingDocumentation}".Trim();
        }

        private static void FillBidAndScorDates(HtmlNodeNavigator navigator, out DateTime scoringDate,
            out DateTime biddingDate)
        {
            var scoringDateT =
                navigator.SelectSingleNode(
                        "//td[contains(., 'Дата и время рассмотрения заявок:') or contains(., 'Дата рассмотрения заявок:')]/following-sibling::td")
                    ?.Value?.Trim() ??
                "";
            scoringDate = scoringDateT.ParseDateUn("dd.MM.yyyy HH:mm");
            var biddingDateT =
                navigator.SelectSingleNode(
                    "//td[contains(., 'Дата начала аукциона')]/following-sibling::td")?.Value?.Trim() ??
                "";
            biddingDate = biddingDateT.ParseDateUn("dd.MM.yyyy HH:mm");
        }

        private void FillPurName(HtmlNodeNavigator navigator)
        {
            if (string.IsNullOrEmpty(_tn.PurName))
            {
                var firstPurName = navigator.SelectSingleNode(
                                           "//div[@class = 's2']")
                                       ?.Value?.Trim() ??
                                   "";
                _tn.PurName = $"{_tn.FullPw} {firstPurName}";
            }
        }

        private void AddCustomer(MySqlConnection connect, out int customerId)
        {
            customerId = 0;
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
                        $"INSERT INTO {Builder.Prefix}customer SET reg_num = @reg_num, full_name = @full_name, is223=1";
                    var cmd14 = new MySqlCommand(insertCustomer, connect);
                    cmd14.Prepare();
                    var customerRegNumber = Guid.NewGuid().ToString();
                    cmd14.Parameters.AddWithValue("@reg_num", customerRegNumber);
                    cmd14.Parameters.AddWithValue("@full_name", _tn.OrgName);
                    cmd14.ExecuteNonQuery();
                    customerId = (int) cmd14.LastInsertedId;
                }
            }
        }

        private void AddOrganizer(MySqlConnection connect, HtmlNodeNavigator navigator, out int organiserId)
        {
            organiserId = 0;
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
                    var phone = navigator.SelectSingleNode(
                                        "//td[contains(., 'Номер контактного телефона заказчика')]/following-sibling::td")
                                    ?.Value?.Trim() ??
                                "";
                    var email = navigator.SelectSingleNode(
                                        "//td[contains(., 'Контактный адрес e-mail:')]/following-sibling::td")
                                    ?.Value
                                    ?.Trim() ??
                                "";
                    var contactPerson = navigator.SelectSingleNode(
                                                "//td[contains(., 'Контактное лицо:') or contains(., 'Ответственное лицо:')]/following-sibling::td")
                                            ?.Value?.Trim() ??
                                        "";
                    var postAddr = navigator.SelectSingleNode(
                                           "//td[contains(., 'Почтовый адрес заказчика:')]/following-sibling::td")
                                       ?.Value?.Trim() ??
                                   "";
                    var address = navigator.SelectSingleNode(
                            "//td[contains(., 'Местонахождение заказчика:')]/following-sibling::td")
                        ?.Value?.Trim() ?? "";
                    var addOrganizer =
                        $"INSERT INTO {Builder.Prefix}organizer SET full_name = @full_name, contact_phone = @contact_phone, contact_person = @contact_person, contact_email = @contact_email, post_address = @post_addres, fact_address = @fact_address";
                    var cmd4 = new MySqlCommand(addOrganizer, connect);
                    cmd4.Prepare();
                    cmd4.Parameters.AddWithValue("@full_name", _tn.OrgName);
                    cmd4.Parameters.AddWithValue("@contact_phone", phone);
                    cmd4.Parameters.AddWithValue("@contact_person", contactPerson);
                    cmd4.Parameters.AddWithValue("@contact_email", email);
                    cmd4.Parameters.AddWithValue("@post_address", postAddr);
                    cmd4.Parameters.AddWithValue("@fact_address", address);
                    cmd4.ExecuteNonQuery();
                    organiserId = (int) cmd4.LastInsertedId;
                }
            }
        }

        private void UpdateCancelStatus(MySqlConnection connect, DateTime dateUpd, out bool updated,
            out int cancelStatus)
        {
            updated = false;
            cancelStatus = 0;
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
                updated = true;
                if (dateUpd >= (DateTime) row["date_version"])
                {
                    row["cancel"] = 1;
                }
                else
                {
                    cancelStatus = 1;
                }
            }

            var commandBuilder =
                new MySqlCommandBuilder(adapter2) {ConflictOption = ConflictOption.OverwriteChanges};
            adapter2.Update(dt2);
        }

        private bool TenderExist(MySqlConnection connect)
        {
            var selectTend =
                $"SELECT id_tender FROM {Builder.Prefix}tender WHERE purchase_number = @purchase_number AND end_date = @end_date AND type_fz = @type_fz AND doc_publish_date = @doc_publish_date";
            var cmd = new MySqlCommand(selectTend, connect);
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@purchase_number", _tn.PurNum);
            cmd.Parameters.AddWithValue("@end_date", _tn.DateEnd);
            cmd.Parameters.AddWithValue("@type_fz", TypeFz);
            cmd.Parameters.AddWithValue("@doc_publish_date", _tn.DatePub);
            var dt = new DataTable();
            var adapter = new MySqlDataAdapter {SelectCommand = cmd};
            adapter.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                return true;
            }

            return false;
        }
    }
}