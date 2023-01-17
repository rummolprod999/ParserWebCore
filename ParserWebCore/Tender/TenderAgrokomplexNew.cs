using System;
using System.Data;
using System.Threading;
using MySql.Data.MySqlClient;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using ParserWebCore.BuilderApp;
using ParserWebCore.Connections;
using ParserWebCore.Extensions;
using ParserWebCore.TenderType;

namespace ParserWebCore.Tender
{
    public class TenderAgrokomplexNew : TenderAbstract, ITender
    {
        private readonly ChromeDriver _driver;
        private readonly TypeAgrokomplex _tn;

        public TenderAgrokomplexNew(string etpName, string etpUrl, int typeFz, TypeAgrokomplex tn, ChromeDriver driver)
            : base(etpName, etpUrl,
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
                    $"SELECT id_tender FROM {AppBuilder.Prefix}tender WHERE purchase_number = @purchase_number AND end_date = @end_date AND type_fz = @type_fz AND doc_publish_date = @doc_publish_date";
                var cmd = new MySqlCommand(selectTend, connect);
                cmd.Prepare();
                cmd.Parameters.AddWithValue("@purchase_number", _tn.PurNum);
                cmd.Parameters.AddWithValue("@end_date", _tn.DateEnd);
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

                var dateUpd = DateTime.Now;
                var cancelStatus = 0;
                var updated = false;
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
                _driver.Navigate().GoToUrl(_tn.Href);
                Thread.Sleep(3000);
                wait.Until(dr =>
                    dr.FindElement(By.XPath(
                        "//h2[@class = 'main__header-text']")));
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
                        var phone = _tn.Phone;
                        var email = "";
                        var contactPerson = _tn.ContactPerson;
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

                PlacingWay = _driver
                    .FindElementWithoutException(By.XPath("//div[. = 'Тип']/following-sibling::div"))
                    ?.Text.Trim() ?? "";
                GetPlacingWay(connect, out var idPlacingWay);
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
                cmd9.Parameters.AddWithValue("@end_date", _tn.DateEnd);
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
                var lotNum = 1;
                var insertLot =
                    $"INSERT INTO {AppBuilder.Prefix}lot SET id_tender = @id_tender, lot_number = @lot_number, max_price = @max_price, currency = @currency";
                var cmd18 = new MySqlCommand(insertLot, connect);
                cmd18.Prepare();
                cmd18.Parameters.AddWithValue("@id_tender", idTender);
                cmd18.Parameters.AddWithValue("@lot_number", lotNum);
                cmd18.Parameters.AddWithValue("@max_price", "");
                cmd18.Parameters.AddWithValue("@currency", "Рубль");
                cmd18.ExecuteNonQuery();
                var idLot = (int)cmd18.LastInsertedId;
                if (!string.IsNullOrEmpty(_tn.OrgName))
                {
                    var selectCustomer =
                        $"SELECT id_customer FROM {AppBuilder.Prefix}customer WHERE full_name = @full_name";
                    var cmd13 = new MySqlCommand(selectCustomer, connect);
                    cmd13.Prepare();
                    cmd13.Parameters.AddWithValue("@full_name", _tn.OrgName);
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
                        cmd14.Parameters.AddWithValue("@full_name", _tn.OrgName);
                        cmd14.ExecuteNonQuery();
                        customerId = (int)cmd14.LastInsertedId;
                    }
                }

                var delivPlace = "";
                var delivTerm1 = _driver
                    .FindElementWithoutException(By.XPath("//div[. = 'Условия поставки']/following-sibling::div"))
                    ?.Text.Trim() ?? "";
                var delivTerm2 = _driver
                    .FindElementWithoutException(By.XPath("//div[. = 'Срок поставки']/following-sibling::div"))
                    ?.Text.Trim() ?? "";
                var delivTerm3 = _driver
                    .FindElementWithoutException(By.XPath("//div[. = 'Условия оплаты']/following-sibling::div"))
                    ?.Text.Trim() ?? "";
                var delivTerm = "";
                if (!string.IsNullOrEmpty(delivTerm1))
                {
                    delivTerm = $"{delivTerm}Условия поставки: {delivTerm1}";
                }

                if (!string.IsNullOrEmpty(delivTerm2))
                {
                    delivTerm = $"{delivTerm}\nСрок поставки: {delivTerm2}";
                }

                if (!string.IsNullOrEmpty(delivTerm3))
                {
                    delivTerm = $"{delivTerm}\nУсловия оплаты: {delivTerm3}";
                }

                if (!string.IsNullOrEmpty(delivPlace) || !string.IsNullOrEmpty(delivTerm))
                {
                    var insertCustomerRequirement =
                        $"INSERT INTO {AppBuilder.Prefix}customer_requirement SET id_lot = @id_lot, id_customer = @id_customer, delivery_place = @delivery_place, max_price = @max_price, delivery_term = @delivery_term";
                    var cmd16 = new MySqlCommand(insertCustomerRequirement, connect);
                    cmd16.Prepare();
                    cmd16.Parameters.AddWithValue("@id_lot", idLot);
                    cmd16.Parameters.AddWithValue("@id_customer", customerId);
                    cmd16.Parameters.AddWithValue("@delivery_place", delivPlace);
                    cmd16.Parameters.AddWithValue("@max_price", "");
                    cmd16.Parameters.AddWithValue("@delivery_term", delivTerm.ReplaceHtmlEntyty().Trim());
                    cmd16.ExecuteNonQuery();
                }

                _driver.FindElement(By.XPath("//div[@role='tab'][2]")).Click();
                Thread.Sleep(1000);
                _driver.SwitchTo().DefaultContent();

                var lots = _driver.FindElements(By.XPath(
                    "//div[@role='gridcell' and @col-id = 'productName']"));
                for (var i = 0; i < lots.Count; i++)
                {
                    var name =
                        (lots[i]?.Text ?? "")
                        .Trim();
                    var okei = _driver
                        .FindElementWithoutException(
                            By.XPath($"//div[@role='gridcell' and @col-id = 'unitName'][{i + 1}]"))
                        ?.Text.Trim() ?? "";
                    var quantity = _driver
                        .FindElementWithoutException(
                            By.XPath($"//div[@role='gridcell' and @col-id = 'quantity'][{i + 1}]"))
                        ?.Text.Trim() ?? "";
                    var insertLotitem =
                        $"INSERT INTO {AppBuilder.Prefix}purchase_object SET id_lot = @id_lot, id_customer = @id_customer, name = @name, sum = @sum, quantity_value = @quantity_value, customer_quantity_value = @customer_quantity_value, okei = @okei";
                    var cmd19 = new MySqlCommand(insertLotitem, connect);
                    cmd19.Prepare();
                    cmd19.Parameters.AddWithValue("@id_lot", idLot);
                    cmd19.Parameters.AddWithValue("@id_customer", customerId);
                    cmd19.Parameters.AddWithValue("@name", name);
                    cmd19.Parameters.AddWithValue("@sum", "");
                    cmd19.Parameters.AddWithValue("@quantity_value", quantity);
                    cmd19.Parameters.AddWithValue("@customer_quantity_value", quantity);
                    cmd19.Parameters.AddWithValue("@okei", okei);
                    cmd19.ExecuteNonQuery();
                }

                foreach (var v in lots)
                {
                }

                TenderKwords(connect, idTender);
                AddVerNumber(connect, _tn.PurNum, TypeFz);
            }
        }
    }
}