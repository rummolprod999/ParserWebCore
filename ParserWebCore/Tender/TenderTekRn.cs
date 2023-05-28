using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading;
using MySql.Data.MySqlClient;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using ParserWebCore.BuilderApp;
using ParserWebCore.Connections;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.SharedLibraries;
using ParserWebCore.TenderType;

namespace ParserWebCore.Tender
{
    public class TenderTekRn : TenderAbstract, ITender
    {
        private readonly ChromeDriver _driver;
        private readonly TypeTekRn _tn;

        public TenderTekRn(string etpName, string etpUrl, int typeFz, TypeTekRn tn, ChromeDriver driver) : base(etpName,
            etpUrl,
            typeFz)
        {
            _tn = tn;
            _driver = driver;
        }

        public void ParsingTender()
        {
            using (var connect = ConnectToDb.GetDbConnection())
            {
                connect.Open();
                var selectTend =
                    $"SELECT id_tender FROM {AppBuilder.Prefix}tender WHERE purchase_number = @purchase_number AND doc_publish_date = @doc_publish_date AND type_fz = @type_fz AND notice_version = @notice_version";
                var cmd = new MySqlCommand(selectTend, connect);
                cmd.Prepare();
                cmd.Parameters.AddWithValue("@purchase_number", _tn.PurNum);
                cmd.Parameters.AddWithValue("@doc_publish_date", _tn.DatePub);
                cmd.Parameters.AddWithValue("@type_fz", TypeFz);
                cmd.Parameters.AddWithValue("@notice_version", _tn.Status);
                var dt = new DataTable();
                var adapter = new MySqlDataAdapter { SelectCommand = cmd };
                adapter.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    //Log.Logger("This tender is exist in base", PurNum);
                    return;
                }

                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(120));
                _driver.Navigate().GoToUrl(_tn.Href);
                _driver.SwitchTo().DefaultContent();
                Thread.Sleep(5000);
                _driver.SwitchTo().DefaultContent();
                wait.Until(dr =>
                    dr.FindElement(By.XPath(
                        "//h3[. = 'Общие сведения']")));
                var dateUpd = DateTime.Now;

                var (updated, cancelStatus) = UpdateTenderVersion(connect, _tn.PurNum, dateUpd);
                var dateEndT =
                    (_driver.FindElementWithoutException(By.XPath(
                             "//div[. = 'Дата окончания срока подачи технико-коммерческих частей']/following-sibling::time"))
                         ?.Text ??
                     "").Replace("\n", " ").Trim();
                if (dateEndT == "")
                {
                    dateEndT =
                        (_driver.FindElementWithoutException(By.XPath(
                                 "//div[. = 'Дата окончания срока подачи коммерческих частей']/following-sibling::time"))
                             ?.Text ??
                         "").Replace("\n", " ").Trim();
                }

                if (dateEndT == "")
                {
                    dateEndT =
                        (_driver.FindElementWithoutException(By.XPath(
                                 "//div[. = 'Дата окончания срока подачи технических частей']/following-sibling::time"))
                             ?.Text ??
                         "").Replace("\n", " ").Trim();
                }

                var dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy HH:mm 'GMT'z");
                if (dateEnd == DateTime.MinValue)
                {
                    dateEnd = dateEnd.AddDays(2);
                }

                if (_tn.DateEnd == DateTime.MinValue)
                {
                    _tn.DateEnd = dateEnd;
                }

                var printForm = _tn.Href;
                var customerId = 0;
                var organiserId = GetOrganizer(connect);
                PlacingWay =
                    (_driver.FindElementWithoutException(By.XPath(
                            ".//div[. = 'Способ закупки']//following-sibling::div"))
                        ?.Text ?? "")
                    .Trim();
                GetPlacingWay(connect, out var idPlacingWay);
                GetEtp(connect, out var idEtp);
                var purObjInfo =
                    (_driver.FindElementWithoutException(By.XPath(
                            ".//span[contains(@class, 'CardProcedureViewstyled__Title')]"))
                        ?.Text ?? "")
                    .Trim();
                if (purObjInfo == "")
                {
                    purObjInfo =
                        (_driver.FindElementWithoutException(By.XPath(
                                ".//span[contains(normalize-space(),\"Наименование продукции:\")]/following-sibling::*[1]/self::span"))
                            ?.Text ?? "")
                        .Trim();
                }

                if (purObjInfo == "")
                {
                    purObjInfo = _tn.PurName;
                }

                var insertTender =
                    $"INSERT INTO {AppBuilder.Prefix}tender SET id_region = @id_region, id_xml = @id_xml, purchase_number = @purchase_number, doc_publish_date = @doc_publish_date, href = @href, purchase_object_info = @purchase_object_info, type_fz = @type_fz, id_organizer = @id_organizer, id_placing_way = @id_placing_way, id_etp = @id_etp, end_date = @end_date, scoring_date = @scoring_date, bidding_date = @bidding_date, cancel = @cancel, date_version = @date_version, num_version = @num_version, notice_version = @notice_version, xml = @xml, print_form = @print_form";
                var cmd9 = new MySqlCommand(insertTender, connect);
                cmd9.Prepare();
                cmd9.Parameters.AddWithValue("@id_region", 0);
                cmd9.Parameters.AddWithValue("@id_xml", _tn.PurNum);
                cmd9.Parameters.AddWithValue("@purchase_number", _tn.PurNum);
                cmd9.Parameters.AddWithValue("@doc_publish_date", _tn.DatePub);
                cmd9.Parameters.AddWithValue("@href", _tn.Href);
                cmd9.Parameters.AddWithValue("@purchase_object_info", purObjInfo);
                cmd9.Parameters.AddWithValue("@type_fz", TypeFz);
                cmd9.Parameters.AddWithValue("@id_organizer", organiserId);
                cmd9.Parameters.AddWithValue("@id_placing_way", idPlacingWay);
                cmd9.Parameters.AddWithValue("@id_etp", idEtp);
                cmd9.Parameters.AddWithValue("@end_date", _tn.DateEnd);
                cmd9.Parameters.AddWithValue("@scoring_date", _tn.Scoring);
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
                var docs = _driver.FindElements(By.CssSelector("a[href^='/document.php?']"));
                if (docs.Count == 0)
                {
                    try
                    {
                        _driver.FindElement(By.XPath("//div[h3[. = 'Документация по процедуре']]")).Click();
                        docs = _driver.FindElements(By.XPath(
                            "//a[contains(@class, 'Documentstyled__Container-sc')]"));
                    }
                    catch (Exception e)
                    {
                        Log.Logger(e);
                    }
                }

                GetDocs(docs, connect, idTender);
                var lots = _driver.FindElements(By.XPath(
                    "//div[contains(@class, 'Tabsstyled__TabsItem-sc')]"));
                GetLots(lots, connect, idTender, customerId, purObjInfo);
                TenderKwords(connect, idTender);
                AddVerNumber(connect, _tn.PurNum, TypeFz);
            }
        }

        private void GetLots(ReadOnlyCollection<IWebElement> lots, MySqlConnection connect, int idTender,
            int customerId,
            string purObjInfo)
        {
            foreach (var lot in lots)
            {
                var lotNumT =
                    (lot.FindElementWithoutException(
                        By.XPath(".//span[contains(@class, 'CardProcedureViewstyled__Title-sc')]"))?.Text ?? "")
                    .Trim();
                lotNumT = lotNumT.GetDataFromRegex(@"Лот\s+(\d+)");
                int.TryParse(lotNumT, out var lotNum);
                if (lotNum == 0) lotNum = 1;
                var nmckT = (lot.FindElementWithoutException(By.XPath(
                            ".//div[contains(@class, 'ProcedurePricestyled__Container-sc')]"))
                        ?.Text ?? "0.0")
                    .Trim();
                var currency = nmckT.GetDataFromRegex(@"[\D]$");
                if (currency == "")
                {
                    currency = "₽";
                }

                var nmck = nmckT.ExtractPriceNew();
                if (nmck == "")
                {
                    nmck = _tn.Nmck;
                }

                var purName =
                    (lot.FindElementWithoutException(By.XPath(
                            ".//td[contains(normalize-space(),\"Предмет договора:\")]/following-sibling::*[1]/self::td"))
                        ?.Text ?? "").Trim();
                if (string.IsNullOrEmpty(purName)) purName = purObjInfo;
                var insertLot =
                    $"INSERT INTO {AppBuilder.Prefix}lot SET id_tender = @id_tender, lot_number = @lot_number, max_price = @max_price, currency = @currency, lot_name = @lot_name";
                var cmd18 = new MySqlCommand(insertLot, connect);
                cmd18.Prepare();
                cmd18.Parameters.AddWithValue("@id_tender", idTender);
                cmd18.Parameters.AddWithValue("@lot_number", lotNum);
                cmd18.Parameters.AddWithValue("@max_price", nmck);
                cmd18.Parameters.AddWithValue("@currency", currency);
                cmd18.Parameters.AddWithValue("@lot_name", purName);
                cmd18.ExecuteNonQuery();
                var idLot = (int)cmd18.LastInsertedId;
                var customerFullName =
                    (lot.FindElementWithoutException(
                            By.XPath(
                                ".//div[. = 'Заказчик']//following-sibling::div"))
                        ?.Text ?? "").Trim();
                if (!string.IsNullOrEmpty(customerFullName))
                {
                    var selectCustomer =
                        $"SELECT id_customer FROM {AppBuilder.Prefix}customer WHERE full_name = @full_name";
                    var cmd13 = new MySqlCommand(selectCustomer, connect);
                    cmd13.Prepare();
                    cmd13.Parameters.AddWithValue("@full_name", customerFullName);
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
                            $"INSERT INTO {AppBuilder.Prefix}customer SET reg_num = @reg_num, full_name = @full_name, is223=1";
                        var cmd14 = new MySqlCommand(insertCustomer, connect);
                        cmd14.Prepare();
                        var customerRegNumber = Guid.NewGuid().ToString();
                        cmd14.Parameters.AddWithValue("@reg_num", customerRegNumber);
                        cmd14.Parameters.AddWithValue("@full_name", customerFullName);
                        cmd14.ExecuteNonQuery();
                        customerId = (int)cmd14.LastInsertedId;
                    }
                }

                var okpd2Temp =
                    (lot.FindElementWithoutException(By.XPath(
                            ".//p[. = 'ОКДП/ОКДП2']//following-sibling::div/div"))
                        ?.Text ?? "")
                    .Trim();
                var okpd2Code = okpd2Temp.GetDataFromRegex(@"^(\d[\.|\d]*\d)");
                var okpd2GroupCode = 0;
                var okpd2GroupLevel1Code = "";
                if (!String.IsNullOrEmpty(okpd2Code))
                {
                    GetOkpd(okpd2Code, out okpd2GroupCode, out okpd2GroupLevel1Code);
                }

                var okpdName = okpd2Temp.GetDataFromRegex(@"^\d[\.|\d]*\d (.*)$");
                if (!string.IsNullOrEmpty(purName))
                {
                    var insertLotitem =
                        $"INSERT INTO {AppBuilder.Prefix}purchase_object SET id_lot = @id_lot, id_customer = @id_customer, name = @name, sum = @sum, okpd2_code = @okpd2_code, okpd2_group_code = @okpd2_group_code, okpd2_group_level1_code = @okpd2_group_level1_code, okpd_name = @okpd_name";
                    var cmd19 = new MySqlCommand(insertLotitem, connect);
                    cmd19.Prepare();
                    cmd19.Parameters.AddWithValue("@id_lot", idLot);
                    cmd19.Parameters.AddWithValue("@id_customer", customerId);
                    cmd19.Parameters.AddWithValue("@name", purName);
                    cmd19.Parameters.AddWithValue("@sum", nmck);
                    cmd19.Parameters.AddWithValue("@okpd2_code", okpd2Code);
                    cmd19.Parameters.AddWithValue("@okpd2_group_code", okpd2GroupCode);
                    cmd19.Parameters.AddWithValue("@okpd2_group_level1_code", okpd2GroupLevel1Code);
                    cmd19.Parameters.AddWithValue("@okpd_name", okpdName);
                    cmd19.ExecuteNonQuery();
                }

                var appGuarAt =
                    (lot.FindElementWithoutException(By.XPath(
                            ".//p[. = 'Обеспечение заявки']//following-sibling::div"))
                        ?.Text ?? "")
                    .Trim();
                var appGuarA = SharedTekTorg.ParsePrice(appGuarAt);
                if (appGuarA != 0.0m)
                {
                    var insertCustomerRequirement =
                        $"INSERT INTO {AppBuilder.Prefix}customer_requirement SET id_lot = @id_lot, id_customer = @id_customer, application_guarantee_amount = @application_guarantee_amount, max_price = @max_price";
                    var cmd16 = new MySqlCommand(insertCustomerRequirement, connect);
                    cmd16.Prepare();
                    cmd16.Parameters.AddWithValue("@id_lot", idLot);
                    cmd16.Parameters.AddWithValue("@id_customer", customerId);
                    cmd16.Parameters.AddWithValue("@application_guarantee_amount", appGuarA);
                    cmd16.Parameters.AddWithValue("@max_price", nmck);
                    cmd16.ExecuteNonQuery();
                }
            }
        }

        private void GetDocs(ReadOnlyCollection<IWebElement> docs, MySqlConnection connect, int idTender)
        {
            foreach (var doc in docs)
            {
                var fName = (doc?.FindElementWithoutException(By.XPath(
                        ".//p"))
                    ?.Text ?? "").Trim();
                var urlAtt = (doc?.GetAttribute("href") ?? "").Trim();
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
        }

        private int GetOrganizer(MySqlConnection connect)
        {
            var organiserId = 0;
            if (_tn.OrgName == "")
            {
                _tn.OrgName =
                    (_driver.FindElementWithoutException(By.XPath(
                            ".//p[. = 'Наименование организатора']//following-sibling::div"))
                        ?.Text ?? "")
                    .Trim();
            }

            if (!string.IsNullOrEmpty(_tn.OrgName))
            {
                var selectOrg =
                    $"SELECT id_organizer FROM {AppBuilder.Prefix}organizer WHERE full_name = @full_name";
                var cmd3 = new MySqlCommand(selectOrg, connect);
                cmd3.Prepare();
                cmd3.Parameters.AddWithValue("@full_name", _tn.OrgName);
                var dt3 = new DataTable();
                var adapter3 = new MySqlDataAdapter { SelectCommand = cmd3 };
                adapter3.Fill(dt3);
                if (dt3.Rows.Count > 0)
                {
                    organiserId = (int)dt3.Rows[0].ItemArray[0];
                }
                else
                {
                    var phone = (_driver.FindElementWithoutException(By.XPath(
                                         (".//p[. = 'Контактный телефон']//following-sibling::div")))
                                     ?.Text ??
                                 "")
                        .Trim();
                    var email = (_driver
                            .FindElementWithoutException(
                                By.XPath(".//p[. = 'Адрес электронной почты']//following-sibling::div"))
                            ?.Text ?? "")
                        .Trim();
                    var contactPerson =
                        (_driver.FindElementWithoutException(By.XPath(
                                ".//p[. = 'ФИО контактного лица']//following-sibling::div"))
                            ?.Text ?? "")
                        .Trim();
                    var addOrganizer =
                        $"INSERT INTO {AppBuilder.Prefix}organizer SET full_name = @full_name, contact_phone = @contact_phone, contact_person = @contact_person, contact_email = @contact_email";
                    var cmd4 = new MySqlCommand(addOrganizer, connect);
                    cmd4.Prepare();
                    cmd4.Parameters.AddWithValue("@full_name", _tn.OrgName);
                    cmd4.Parameters.AddWithValue("@contact_phone", phone);
                    cmd4.Parameters.AddWithValue("@contact_person", contactPerson);
                    cmd4.Parameters.AddWithValue("@contact_email", email);
                    cmd4.ExecuteNonQuery();
                    organiserId = (int)cmd4.LastInsertedId;
                }
            }

            return organiserId;
        }
    }
}