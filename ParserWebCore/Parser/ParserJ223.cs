using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using MySql.Data.MySqlClient;
using ParserWebCore.Connections;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserJ223 : ParserAbstract, IParser
    {
        private const int PageCount = 20;

        private readonly List<string> _listUrls = new List<string>
        {
            "https://zakupki.gov.ru/epz/order/extendedsearch/results.html?searchString=&morphology=on&search-filter=Дате+размещения&search-filter=&sortDirection=false&recordsPerPage=_50&showLotsInfoHidden=false&fz223=on&sortBy=PUBLISH_DATE&okpd2Ids=&okpd2IdsCodes=&af=on&placingWaysList=&placingWaysList223=&placingChildWaysList=&publishDateFrom=&publishDateTo=&applSubmissionCloseDateFrom=&applSubmissionCloseDateTo=&priceFromGeneral=&priceFromGWS=&priceFromUnitGWS=&priceToGeneral=&priceToGWS=&priceToUnitGWS=&currencyIdGeneral=-1&customerTitle=&customerCode=&customerFz94id=&customerFz223id=&customerInn=&orderPlacement94_0=&orderPlacement94_1=&orderPlacement94_2=&npaHidden=&restrictionsToPurchase44=&pageNumber=",
            "https://zakupki.gov.ru/epz/order/extendedsearch/results.html?searchString=&morphology=on&search-filter=Дате+размещения&search-filter=&sortDirection=false&recordsPerPage=_50&showLotsInfoHidden=false&fz223=on&sortBy=PUBLISH_DATE&okpd2Ids=&okpd2IdsCodes=&af=on&placingWaysList=&placingWaysList223=&placingChildWaysList=&publishDateFrom=&publishDateTo=&applSubmissionCloseDateFrom=&applSubmissionCloseDateTo=&priceFromGeneral=&priceFromGWS=&priceFromUnitGWS=&priceToGeneral=&priceToGWS=&priceToUnitGWS=&currencyIdGeneral=-1&customerTitle=&customerCode=&customerFz94id=&customerFz223id=&customerInn=&customerPlaceWithNested=on&customerPlace=5277317&customerPlaceCodes=OKER30&orderPlacement94_0=&orderPlacement94_1=&orderPlacement94_2=&npaHidden=&restrictionsToPurchase44=&pageNumber=",
            $"https://zakupki.gov.ru/epz/order/extendedsearch/results.html?searchString=&morphology=on&search-filter=Дате+размещения&search-filter=&sortDirection=false&recordsPerPage=_50&showLotsInfoHidden=false&fz223=on&sortBy=PUBLISH_DATE&okpd2Ids=&okpd2IdsCodes=&af=on&placingWaysList=&placingWaysList223=&placingChildWaysList=&applSubmissionCloseDateFrom=&applSubmissionCloseDateTo=&priceFromGeneral=&priceFromGWS=&priceFromUnitGWS=&priceToGeneral=&priceToGWS=&priceToUnitGWS=&currencyIdGeneral=-1&customerTitle=&customerCode=&customerFz94id=&customerFz223id=&customerInn=&orderPlacement94_0=&orderPlacement94_1=&orderPlacement94_2=&npaHidden=&restrictionsToPurchase44=&publishDateFrom={DateTime.Now.AddDays(-1):dd.MM.yyyy}+-+{DateTime.Now.AddDays(+1):dd.MM.yyyy}&publishDateTo={DateTime.Now.AddDays(+1):dd.MM.yyyy}&pageNumber=",
            "https://zakupki.gov.ru/epz/order/extendedsearch/results.html?searchString=&morphology=on&search-filter=Дате+размещения&search-filter=&sortDirection=false&recordsPerPage=_50&showLotsInfoHidden=false&fz223=on&sortBy=PUBLISH_DATE&okpd2Ids=&okpd2IdsCodes=&af=on&placingWaysList=&placingWaysList223=&placingChildWaysList=&publishDateFrom=&publishDateTo=&applSubmissionCloseDateFrom=&applSubmissionCloseDateTo=&priceFromGeneral=&priceFromGWS=&priceFromUnitGWS=&priceToGeneral=&priceToGWS=&priceToUnitGWS=&currencyIdGeneral=-1&customerTitle=&customerCode=&customerFz94id=&customerFz223id=&customerInn=&customerPlaceWithNested=on&customerPlace=5277336&customerPlaceCodes=OKER31&orderPlacement94_0=&orderPlacement94_1=&orderPlacement94_2=&npaHidden=&restrictionsToPurchase44=&pageNumber=",
            "https://zakupki.gov.ru/epz/order/extendedsearch/results.html?searchString=&morphology=on&search-filter=Дате+размещения&search-filter=&sortDirection=false&recordsPerPage=_50&showLotsInfoHidden=false&fz223=on&sortBy=PUBLISH_DATE&okpd2Ids=&okpd2IdsCodes=&af=on&placingWaysList=&placingWaysList223=&placingChildWaysList=&publishDateFrom=&publishDateTo=&applSubmissionCloseDateFrom=&applSubmissionCloseDateTo=&priceFromGeneral=&priceFromGWS=&priceFromUnitGWS=&priceToGeneral=&priceToGWS=&priceToUnitGWS=&currencyIdGeneral=-1&customerTitle=&customerCode=&customerFz94id=&customerFz223id=&customerInn=&F=on&S=on&M=on&NOT_FSM=on&orderPlacement94_0=&orderPlacement94_1=&orderPlacement94_2=&npaHidden=&restrictionsToPurchase44=&pageNumber=",
            "https://zakupki.gov.ru/epz/order/extendedsearch/results.html?searchString=&morphology=on&search-filter=Дате+размещения&search-filter=&sortDirection=false&recordsPerPage=_50&showLotsInfoHidden=false&fz223=on&sortBy=UPDATE_DATE&okpd2Ids=&okpd2IdsCodes=&af=on&placingWaysList=&placingWaysList223=&placingChildWaysList=&publishDateFrom=&publishDateTo=&applSubmissionCloseDateFrom=&applSubmissionCloseDateTo=&priceFromGeneral=&priceFromGWS=&priceFromUnitGWS=&priceToGeneral=&priceToGWS=&priceToUnitGWS=&currencyIdGeneral=-1&customerTitle=&customerCode=&customerFz94id=&customerFz223id=&customerInn=&F=on&S=on&M=on&NOT_FSM=on&orderPlacement94_0=&orderPlacement94_1=&orderPlacement94_2=&npaHidden=&restrictionsToPurchase44=&pageNumber=",
            "https://zakupki.gov.ru/epz/order/extendedsearch/results.html?searchString=&morphology=on&search-filter=Дате+размещения&search-filter=&sortDirection=false&recordsPerPage=_50&showLotsInfoHidden=false&fz223=on&sortBy=PUBLISH_DATE&okpd2Ids=&okpd2IdsCodes=&af=on&placingWaysList=&placingWaysList223=&placingChildWaysList=&publishDateFrom=&publishDateTo=&applSubmissionCloseDateFrom=&applSubmissionCloseDateTo=&priceFromGeneral=&priceFromGWS=&priceFromUnitGWS=&priceToGeneral=&priceToGWS=&priceToUnitGWS=&currencyIdGeneral=-1&customerTitle=&customerCode=&customerFz94id=&customerFz223id=&customerInn=&customerPlaceWithNested=on&customerPlace=5277377&customerPlaceCodes=OKER34&orderPlacement94_0=&orderPlacement94_1=&orderPlacement94_2=&npaHidden=&restrictionsToPurchase44=&pageNumber=",
            "https://zakupki.gov.ru/epz/order/extendedsearch/results.html?searchString=&morphology=on&search-filter=Дате+размещения&search-filter=&sortDirection=false&recordsPerPage=_50&showLotsInfoHidden=false&fz223=on&sortBy=PUBLISH_DATE&okpd2Ids=&okpd2IdsCodes=&af=on&placingWaysList=&placingWaysList223=&placingChildWaysList=&publishDateFrom=&publishDateTo=&applSubmissionCloseDateFrom=&applSubmissionCloseDateTo=&priceFromGeneral=&priceFromGWS=&priceFromUnitGWS=&priceToGeneral=&priceToGWS=&priceToUnitGWS=&currencyIdGeneral=-1&customerTitle=&customerCode=&customerFz94id=&customerFz223id=&customerInn=&customerPlaceWithNested=on&customerPlace=9409197&customerPlaceCodes=OKER38&orderPlacement94_0=&orderPlacement94_1=&orderPlacement94_2=&npaHidden=&restrictionsToPurchase44=&pageNumber=",
            "https://zakupki.gov.ru/epz/order/extendedsearch/results.html?searchString=&morphology=on&search-filter=Дате+размещения&search-filter=&sortDirection=false&recordsPerPage=_50&showLotsInfoHidden=false&fz223=on&sortBy=PUBLISH_DATE&okpd2Ids=&okpd2IdsCodes=&af=on&placingWaysList=&placingWaysList223=&placingChildWaysList=&publishDateFrom=&publishDateTo=&applSubmissionCloseDateFrom=&applSubmissionCloseDateTo=&priceFromGeneral=&priceFromGWS=&priceFromUnitGWS=&priceToGeneral=&priceToGWS=&priceToUnitGWS=&currencyIdGeneral=-1&customerTitle=&customerCode=&customerFz94id=&customerFz223id=&customerInn=&customerPlaceWithNested=on&customerPlace=5277399&customerPlaceCodes=OKER36&orderPlacement94_0=&orderPlacement94_1=&orderPlacement94_2=&npaHidden=&restrictionsToPurchase44=&pageNumber=",
            "https://zakupki.gov.ru/epz/order/extendedsearch/results.html?searchString=&morphology=on&search-filter=Дате+размещения&search-filter=&sortDirection=false&recordsPerPage=_50&showLotsInfoHidden=false&fz223=on&sortBy=PUBLISH_DATE&okpd2Ids=&okpd2IdsCodes=&af=on&placingWaysList=&placingWaysList223=&placingChildWaysList=&publishDateFrom=&publishDateTo=&applSubmissionCloseDateFrom=&applSubmissionCloseDateTo=&priceFromGeneral=&priceFromGWS=&priceFromUnitGWS=&priceToGeneral=&priceToGWS=&priceToUnitGWS=&currencyIdGeneral=-1&customerTitle=&customerCode=&customerFz94id=&customerFz223id=&customerInn=&customerPlaceWithNested=on&customerPlace=5277384&customerPlaceCodes=OKER35&orderPlacement94_0=&orderPlacement94_1=&orderPlacement94_2=&npaHidden=&restrictionsToPurchase44=&pageNumber=",
            "https://zakupki.gov.ru/epz/order/extendedsearch/results.html?searchString=&morphology=on&search-filter=Дате+размещения&search-filter=&sortDirection=false&recordsPerPage=_50&showLotsInfoHidden=false&fz223=on&sortBy=PUBLISH_DATE&okpd2Ids=&okpd2IdsCodes=&af=on&placingWaysList=&placingWaysList223=&placingChildWaysList=&publishDateFrom=&publishDateTo=&applSubmissionCloseDateFrom=&applSubmissionCloseDateTo=&priceFromGeneral=&priceFromGWS=&priceFromUnitGWS=&priceToGeneral=&priceToGWS=&priceToUnitGWS=&currencyIdGeneral=-1&customerTitle=&customerCode=&customerFz94id=&customerFz223id=&customerInn=&customerPlaceWithNested=on&customerPlace=5277362&customerPlaceCodes=OKER33&orderPlacement94_0=&orderPlacement94_1=&orderPlacement94_2=&npaHidden=&restrictionsToPurchase44=&pageNumber=",
            "https://zakupki.gov.ru/epz/order/extendedsearch/results.html?searchString=&morphology=on&search-filter=Дате+размещения&search-filter=&sortDirection=false&recordsPerPage=_50&showLotsInfoHidden=false&fz223=on&sortBy=PUBLISH_DATE&okpd2Ids=&okpd2IdsCodes=&af=on&placingWaysList=&placingWaysList223=&placingChildWaysList=&publishDateFrom=&publishDateTo=&applSubmissionCloseDateFrom=&applSubmissionCloseDateTo=&priceFromGeneral=&priceFromGWS=&priceFromUnitGWS=&priceToGeneral=&priceToGWS=&priceToUnitGWS=&currencyIdGeneral=-1&customerTitle=&customerCode=&customerFz94id=&customerFz223id=&customerInn=&customerPlaceWithNested=on&customerPlace=9371527&customerPlaceCodes=OKER40&orderPlacement94_0=&orderPlacement94_1=&orderPlacement94_2=&npaHidden=&restrictionsToPurchase44=&pageNumber=",
            "https://zakupki.gov.ru/epz/order/extendedsearch/results.html?searchString=&morphology=on&search-filter=Дате+размещения&search-filter=&sortDirection=false&recordsPerPage=_50&showLotsInfoHidden=false&fz223=on&sortBy=PUBLISH_DATE&okpd2Ids=&okpd2IdsCodes=&af=on&placingWaysList=&placingWaysList223=&placingChildWaysList=&publishDateFrom=&publishDateTo=&applSubmissionCloseDateFrom=&applSubmissionCloseDateTo=&priceFromGeneral=&priceFromGWS=&priceFromUnitGWS=&priceToGeneral=&priceToGWS=&priceToUnitGWS=&currencyIdGeneral=-1&customerTitle=&customerCode=&customerFz94id=&customerFz223id=&customerInn=&customerPlaceWithNested=on&customerPlace=5277409&customerPlaceCodes=OKER40&orderPlacement94_0=&orderPlacement94_1=&orderPlacement94_2=&npaHidden=&restrictionsToPurchase44=&pageNumber=",
            "https://zakupki.gov.ru/epz/order/extendedsearch/results.html?searchString=&morphology=on&search-filter=Дате+размещения&search-filter=&sortDirection=false&recordsPerPage=_50&showLotsInfoHidden=false&fz223=on&sortBy=PUBLISH_DATE&okpd2IdsWithNested=on&okpd2IdsSeveral=on&okpd2Ids=8873863%2C8873862%2C8873861%2C8873871%2C8873870%2C8873869%2C8873868%2C8873867%2C8873866%2C8873865%2C8873864%2C8873879%2C8873878%2C8873877%2C8873876%2C8873875%2C8873874%2C8873873%2C8873872%2C8873881%2C8873880&okpd2IdsCodes=C%2CB%2CA%2CK%2CJ%2CI%2CH%2CG%2CF%2CE%2CD%2CS%2CR%2CQ%2CP%2CO%2CN%2CM%2CL%2CU%2CT&af=on&placingWaysList=&placingWaysList223=&placingChildWaysList=&publishDateFrom=&publishDateTo=&applSubmissionCloseDateFrom=&applSubmissionCloseDateTo=&priceFromGeneral=&priceFromGWS=&priceFromUnitGWS=&priceToGeneral=&priceToGWS=&priceToUnitGWS=&currencyIdGeneral=-1&customerTitle=&customerCode=&customerFz94id=&customerFz223id=&customerInn=&orderPlacement94_0=&orderPlacement94_1=&orderPlacement94_2=&npaHidden=&restrictionsToPurchase44=&pageNumber=",
            "https://zakupki.gov.ru/epz/order/extendedsearch/results.html?searchString=&morphology=on&search-filter=Дате+размещения&search-filter=&sortDirection=false&recordsPerPage=_50&showLotsInfoHidden=false&fz223=on&sortBy=PUBLISH_DATE&okpd2Ids=&okpd2IdsCodes=&af=on&placingWaysList=&placingWaysList223=&placingChildWaysList=&publishDateFrom=&publishDateTo=&applSubmissionCloseDateFrom=&applSubmissionCloseDateTo=&priceFromGeneral=&priceFromGWS=&priceFromUnitGWS=&priceToGeneral=&priceToGWS=&priceToUnitGWS=&currencyIdGeneral=-1&customerTitle=&customerCode=&customerFz94id=&customerFz223id=&customerInn=&delKladrIdsWithNested=on&delKladrIds=5277409%2C5277377%2C9409197%2C5277317%2C5277399&delKladrIdsCodes=99000000000%2COKER34%2COKER38%2COKER30%2COKER36&orderPlacement94_0=&orderPlacement94_1=&orderPlacement94_2=&npaHidden=&restrictionsToPurchase44=&pageNumber=",
            "https://zakupki.gov.ru/epz/order/extendedsearch/results.html?searchString=&morphology=on&search-filter=Дате+размещения&search-filter=&sortDirection=false&recordsPerPage=_50&showLotsInfoHidden=false&fz223=on&sortBy=PUBLISH_DATE&okpd2Ids=&okpd2IdsCodes=&af=on&placingWaysList=&placingWaysList223=&placingChildWaysList=&publishDateFrom=&publishDateTo=&applSubmissionCloseDateFrom=&applSubmissionCloseDateTo=&priceFromGeneral=&priceFromGWS=&priceFromUnitGWS=&priceToGeneral=&priceToGWS=&priceToUnitGWS=&currencyIdGeneral=-1&customerTitle=&customerCode=&customerFz94id=&customerFz223id=&customerInn=&delKladrIdsWithNested=on&delKladrIds=5277362%2C5277336%2C5277384%2C9371527&delKladrIdsCodes=OKER33%2COKER31%2COKER35%2COKER40&orderPlacement94_0=&orderPlacement94_1=&orderPlacement94_2=&npaHidden=&restrictionsToPurchase44=&pageNumber=",
            $"https://zakupki.gov.ru/epz/order/extendedsearch/results.html?searchString=&morphology=on&search-filter=Дате+размещения&search-filter=&sortDirection=false&recordsPerPage=_50&showLotsInfoHidden=false&fz223=on&sortBy=UPDATE_DATE&okpd2Ids=&okpd2IdsCodes=&af=on&placingWaysList=&placingWaysList223=&placingChildWaysList=&publishDateFrom={DateTime.Now.AddDays(-1):dd.MM.yyyy}+-+{DateTime.Now.AddDays(+1):dd.MM.yyyy}&publishDateTo={DateTime.Now.AddDays(+1):dd.MM.yyyy}&applSubmissionCloseDateFrom=&applSubmissionCloseDateTo=&priceFromGeneral=&priceFromGWS=&priceFromUnitGWS=&priceToGeneral=&priceToGWS=&priceToUnitGWS=&currencyIdGeneral=-1&customerTitle=&customerCode=&customerFz94id=&customerFz223id=&customerInn=&orderPlacement94_0=&orderPlacement94_1=&orderPlacement94_2=&npaHidden=&restrictionsToPurchase44=&pageNumber=",
            $"https://zakupki.gov.ru/epz/order/extendedsearch/results.html?searchString=&morphology=on&search-filter=Дате+размещения&search-filter=&sortDirection=false&recordsPerPage=_50&showLotsInfoHidden=false&fz223=on&sortBy=PUBLISH_DATE&okpd2Ids=&okpd2IdsCodes=&af=on&placingWaysList=&placingWaysList223=&placingChildWaysList=&publishDateFrom={DateTime.Now:dd.MM.yyyy}+-+{DateTime.Now:dd.MM.yyyy}&publishDateTo={DateTime.Now:dd.MM.yyyy}&applSubmissionCloseDateFrom=&applSubmissionCloseDateTo=&priceFromGeneral=&priceFromGWS=&priceFromUnitGWS=&priceToGeneral=&priceToGWS=&priceToUnitGWS=&currencyIdGeneral=-1&customerTitle=&customerCode=&customerFz94id=&customerFz223id=&customerInn=&orderPlacement94_0=&orderPlacement94_1=&orderPlacement94_2=&npaHidden=&restrictionsToPurchase44=&pageNumber=",
            $"https://zakupki.gov.ru/epz/order/extendedsearch/results.html?searchString=&morphology=on&search-filter=Дате+размещения&search-filter=&sortDirection=true&recordsPerPage=_50&showLotsInfoHidden=false&fz223=on&sortBy=PUBLISH_DATE&okpd2Ids=&okpd2IdsCodes=&af=on&placingWaysList=&placingWaysList223=&placingChildWaysList=&publishDateFrom={DateTime.Now:dd.MM.yyyy}+-+{DateTime.Now:dd.MM.yyyy}&publishDateTo={DateTime.Now:dd.MM.yyyy}&applSubmissionCloseDateFrom=&applSubmissionCloseDateTo=&priceFromGeneral=&priceFromGWS=&priceFromUnitGWS=&priceToGeneral=&priceToGWS=&priceToUnitGWS=&currencyIdGeneral=-1&customerTitle=&customerCode=&customerFz94id=&customerFz223id=&customerInn=&orderPlacement94_0=&orderPlacement94_1=&orderPlacement94_2=&npaHidden=&restrictionsToPurchase44=&pageNumber=",
            "https://zakupki.gov.ru/epz/order/extendedsearch/results.html?searchString=&morphology=on&search-filter=Дате+размещения&search-filter=&sortDirection=false&recordsPerPage=_50&showLotsInfoHidden=false&fz223=on&sortBy=UPDATE_DATE&okpd2Ids=&okpd2IdsCodes=&af=on&placingWaysList=&placingWaysList223=&placingChildWaysList=&publishDateFrom=&publishDateTo=&applSubmissionCloseDateFrom=&applSubmissionCloseDateTo=&priceFromGeneral=&priceFromGWS=&priceFromUnitGWS=&priceToGeneral=&priceToGWS=&priceToUnitGWS=&currencyIdGeneral=-1&customerTitle=&customerCode=&customerFz94id=&customerFz223id=&customerInn=&orderPlacement94_0=&orderPlacement94_1=&orderPlacement94_2=&npaHidden=&restrictionsToPurchase44=&pageNumber=",
            $"https://zakupki.gov.ru/epz/order/extendedsearch/results.html?searchString=&morphology=on&search-filter=Дате+размещения&search-filter=&sortDirection=false&recordsPerPage=_50&showLotsInfoHidden=false&fz223=on&sortBy=UPDATE_DATE&okpd2Ids=&okpd2IdsCodes=&af=on&placingWaysList=&placingWaysList223=&placingChildWaysList=&publishDateFrom=&publishDateTo=&applSubmissionCloseDateFrom=&applSubmissionCloseDateTo=&updateDateFrom={DateTime.Now.AddDays(-1):dd.MM.yyyy}+-+{DateTime.Now:dd.MM.yyyy}&updateDateTo={DateTime.Now:dd.MM.yyyy}&priceFromGeneral=&priceFromGWS=&priceFromUnitGWS=&priceToGeneral=&priceToGWS=&priceToUnitGWS=&currencyIdGeneral=-1&customerTitle=&customerCode=&customerFz94id=&customerFz223id=&customerInn=&orderPlacement94_0=&orderPlacement94_1=&orderPlacement94_2=&npaHidden=&restrictionsToPurchase44=&pageNumber=",
            $"https://zakupki.gov.ru/epz/order/extendedsearch/results.html?searchString=&morphology=on&search-filter=Дате+размещения&search-filter=&sortDirection=true&recordsPerPage=_50&showLotsInfoHidden=false&fz223=on&sortBy=UPDATE_DATE&okpd2Ids=&okpd2IdsCodes=&af=on&placingWaysList=&placingWaysList223=&placingChildWaysList=&publishDateFrom=&publishDateTo=&applSubmissionCloseDateFrom=&applSubmissionCloseDateTo=&updateDateFrom={DateTime.Now.AddDays(-1):dd.MM.yyyy}+-+{DateTime.Now:dd.MM.yyyy}&updateDateTo={DateTime.Now:dd.MM.yyyy}&priceFromGeneral=&priceFromGWS=&priceFromUnitGWS=&priceToGeneral=&priceToGWS=&priceToUnitGWS=&currencyIdGeneral=-1&customerTitle=&customerCode=&customerFz94id=&customerFz223id=&customerInn=&orderPlacement94_0=&orderPlacement94_1=&orderPlacement94_2=&npaHidden=&restrictionsToPurchase44=&pageNumber=",
            $"https://zakupki.gov.ru/epz/order/extendedsearch/results.html?searchString=&morphology=on&search-filter=Дате+размещения&search-filter=&sortDirection=false&recordsPerPage=_50&showLotsInfoHidden=false&fz223=on&sortBy=PRICE&okpd2Ids=&okpd2IdsCodes=&af=on&placingWaysList=&placingWaysList223=&placingChildWaysList=&publishDateFrom=&publishDateTo=&applSubmissionCloseDateFrom=&applSubmissionCloseDateTo=&updateDateFrom={DateTime.Now.AddDays(-1):dd.MM.yyyy}+-+{DateTime.Now:dd.MM.yyyy}&updateDateTo={DateTime.Now:dd.MM.yyyy}&priceFromGeneral=&priceFromGWS=&priceFromUnitGWS=&priceToGeneral=&priceToGWS=&priceToUnitGWS=&currencyIdGeneral=-1&customerTitle=&customerCode=&customerFz94id=&customerFz223id=&customerInn=&orderPlacement94_0=&orderPlacement94_1=&orderPlacement94_2=&npaHidden=&restrictionsToPurchase44=&pageNumber=",
            $"https://zakupki.gov.ru/epz/order/extendedsearch/results.html?searchString=&morphology=on&search-filter=Дате+размещения&search-filter=&sortDirection=true&recordsPerPage=_50&showLotsInfoHidden=false&fz223=on&sortBy=PRICE&okpd2Ids=&okpd2IdsCodes=&af=on&placingWaysList=&placingWaysList223=&placingChildWaysList=&publishDateFrom=&publishDateTo=&applSubmissionCloseDateFrom=&applSubmissionCloseDateTo=&updateDateFrom={DateTime.Now.AddDays(-1):dd.MM.yyyy}+-+{DateTime.Now:dd.MM.yyyy}&updateDateTo={DateTime.Now:dd.MM.yyyy}&priceFromGeneral=&priceFromGWS=&priceFromUnitGWS=&priceToGeneral=&priceToGWS=&priceToUnitGWS=&currencyIdGeneral=-1&customerTitle=&customerCode=&customerFz94id=&customerFz223id=&customerInn=&orderPlacement94_0=&orderPlacement94_1=&orderPlacement94_2=&npaHidden=&restrictionsToPurchase44=&pageNumber=",
            $"https://zakupki.gov.ru/epz/order/extendedsearch/results.html?searchString=&morphology=on&search-filter=Дате+размещения&search-filter=&sortDirection=false&recordsPerPage=_50&showLotsInfoHidden=false&fz223=on&sortBy=RELEVANCE&okpd2Ids=&okpd2IdsCodes=&af=on&placingWaysList=&placingWaysList223=&placingChildWaysList=&publishDateFrom=&publishDateTo=&applSubmissionCloseDateFrom=&applSubmissionCloseDateTo=&updateDateFrom={DateTime.Now.AddDays(-1):dd.MM.yyyy}+-+{DateTime.Now:dd.MM.yyyy}&updateDateTo={DateTime.Now:dd.MM.yyyy}&priceFromGeneral=&priceFromGWS=&priceFromUnitGWS=&priceToGeneral=&priceToGWS=&priceToUnitGWS=&currencyIdGeneral=-1&customerTitle=&customerCode=&customerFz94id=&customerFz223id=&customerInn=&orderPlacement94_0=&orderPlacement94_1=&orderPlacement94_2=&npaHidden=&restrictionsToPurchase44=&pageNumber=",
            $"https://zakupki.gov.ru/epz/order/extendedsearch/results.html?searchString=&morphology=on&search-filter=Дате+размещения&search-filter=&sortDirection=false&recordsPerPage=_50&showLotsInfoHidden=false&fz223=on&sortBy=RELEVANCE&okpd2Ids=&okpd2IdsCodes=&af=on&placingWaysList=&placingWaysList223=&placingChildWaysList=&publishDateFrom={DateTime.Now.AddDays(-1):dd.MM.yyyy}+-+{DateTime.Now:dd.MM.yyyy}&publishDateTo={DateTime.Now:dd.MM.yyyy}&applSubmissionCloseDateFrom=&applSubmissionCloseDateTo=&priceFromGeneral=&priceFromGWS=&priceFromUnitGWS=&priceToGeneral=&priceToGWS=&priceToUnitGWS=&currencyIdGeneral=-1&customerTitle=&customerCode=&customerFz94id=&customerFz223id=&customerInn=&orderPlacement94_0=&orderPlacement94_1=&orderPlacement94_2=&npaHidden=&restrictionsToPurchase44=&pageNumber=",
            $"https://zakupki.gov.ru/epz/order/extendedsearch/results.html?searchString=&morphology=on&search-filter=Дате+размещения&search-filter=&sortDirection=true&recordsPerPage=_50&showLotsInfoHidden=false&fz223=on&sortBy=RELEVANCE&okpd2Ids=&okpd2IdsCodes=&af=on&placingWaysList=&placingWaysList223=&placingChildWaysList=&publishDateFrom={DateTime.Now.AddDays(-1):dd.MM.yyyy}+-+{DateTime.Now:dd.MM.yyyy}&publishDateTo={DateTime.Now:dd.MM.yyyy}&applSubmissionCloseDateFrom=&applSubmissionCloseDateTo=&priceFromGeneral=&priceFromGWS=&priceFromUnitGWS=&priceToGeneral=&priceToGWS=&priceToUnitGWS=&currencyIdGeneral=-1&customerTitle=&customerCode=&customerFz94id=&customerFz223id=&customerInn=&orderPlacement94_0=&orderPlacement94_1=&orderPlacement94_2=&npaHidden=&restrictionsToPurchase44=&pageNumber=",
            $"https://zakupki.gov.ru/epz/order/extendedsearch/results.html?searchString=&morphology=on&search-filter=Дате+размещения&search-filter=&sortDirection=true&recordsPerPage=_50&showLotsInfoHidden=false&fz223=on&sortBy=PRICE&okpd2Ids=&okpd2IdsCodes=&af=on&placingWaysList=&placingWaysList223=&placingChildWaysList=&publishDateFrom={DateTime.Now.AddDays(-1):dd.MM.yyyy}+-+{DateTime.Now:dd.MM.yyyy}&publishDateTo={DateTime.Now:dd.MM.yyyy}&applSubmissionCloseDateFrom=&applSubmissionCloseDateTo=&priceFromGeneral=&priceFromGWS=&priceFromUnitGWS=&priceToGeneral=&priceToGWS=&priceToUnitGWS=&currencyIdGeneral=-1&customerTitle=&customerCode=&customerFz94id=&customerFz223id=&customerInn=&orderPlacement94_0=&orderPlacement94_1=&orderPlacement94_2=&npaHidden=&restrictionsToPurchase44=&pageNumber=",
            $"https://zakupki.gov.ru/epz/order/extendedsearch/results.html?searchString=&morphology=on&search-filter=Дате+размещения&search-filter=&sortDirection=false&recordsPerPage=_50&showLotsInfoHidden=false&fz223=on&sortBy=PRICE&okpd2Ids=&okpd2IdsCodes=&af=on&placingWaysList=&placingWaysList223=&placingChildWaysList=&publishDateFrom={DateTime.Now.AddDays(-1):dd.MM.yyyy}+-+{DateTime.Now:dd.MM.yyyy}&publishDateTo={DateTime.Now:dd.MM.yyyy}&applSubmissionCloseDateFrom=&applSubmissionCloseDateTo=&priceFromGeneral=&priceFromGWS=&priceFromUnitGWS=&priceToGeneral=&priceToGWS=&priceToUnitGWS=&currencyIdGeneral=-1&customerTitle=&customerCode=&customerFz94id=&customerFz223id=&customerInn=&orderPlacement94_0=&orderPlacement94_1=&orderPlacement94_2=&npaHidden=&restrictionsToPurchase44=&pageNumber="
        };

        public void Parsing()
        {
            Parse(() => _listUrls.ForEach(ParsingPage));
        }

        private void ParsingPage(string u)
        {
            var maxP = MaxPage(Uri.EscapeUriString($"{u}1"));
            for (var i = 1; i <= maxP; i++)
            {
                var url =
                    Uri.EscapeUriString($"{u}{i}");
                try
                {
                    ParserPage(url);
                }
                catch (Exception e)
                {
                    Log.Logger("Error in ParserJ44.ParserPage", e);
                }
            }
        }

        private void ParserPage(string url)
        {
            if (DownloadString.MaxDownload > 1000) return;
            var s = DownloadString.DownLUserAgentEis(url);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger("Empty string in ParserPage()", url);
                return;
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(s);
            var tens = htmlDoc.DocumentNode.SelectNodes(
                           "//div[contains(@class, 'search-registry-entry-block')]/div[contains(@class, 'row')][1]") ??
                       new HtmlNodeCollection(null);
            foreach (var a in tens)
            {
                try
                {
                    ParserLink(a);
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
            }
        }

        private void ParserLink(HtmlNode n)
        {
            if (DownloadString.MaxDownload > 1000) return;
            var url =
                (n.SelectSingleNode(".//div[contains(@class, 'registry-entry__header-mid__number')]/a")
                    ?.Attributes["href"]?.Value ?? "").Trim();
            var purNumT = (n.SelectSingleNode(".//div[contains(@class, 'registry-entry__header-mid__number')]/a")
                ?.InnerText.Replace("№", "") ?? "").Trim();
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(purNumT)) return;
            var purNum = purNumT;
            if (purNum == "")
            {
                Log.Logger("purNum not found");
                return;
            }

            if (DownloadString.MaxDownload > 1000) return;
            url = "https://zakupki.gov.ru" + url.Replace("common-info.html", "event-journal.html");
            var s = DownloadString.DownLUserAgentEis(url);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger("Empty string in ParserLink()", url);
                return;
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(s);
            var sid = s.GetDataFromRegex(@"sid: '(\d+)',");
            if (string.IsNullOrEmpty(sid))
            {
                Log.Logger("Empty sid in ", url);
                return;
            }

            if (DownloadString.MaxDownload > 1000) return;
            var urlEvent =
                $"https://zakupki.gov.ru/epz/order/notice/card/event/journal/list.html?number=&sid={sid}&entityId=&defaultEntityTypes=false&page=1&pageSize=100&qualifier=order223EventJournalService&sorted=false";
            var s2 = DownloadString.DownLUserAgentEis(urlEvent);
            if (string.IsNullOrEmpty(s2))
            {
                Log.Logger("Empty string in ParserLink()", urlEvent);
                return;
            }

            var htmlDoc2 = new HtmlDocument();
            htmlDoc2.LoadHtml(s2);
            var tens =
                htmlDoc2.DocumentNode.SelectNodes(
                    "//table[@id = 'event']//tbody/tr") ??
                new HtmlNodeCollection(null);

            using (var connect = ConnectToDb.GetDbConnection())
            {
                connect.Open();
                var selectTender =
                    $"SELECT count(*) FROM event_log WHERE   notification_number = @notification_number";
                var cmd = new MySqlCommand(selectTender, connect);
                cmd.Prepare();
                cmd.Parameters.AddWithValue("@notification_number", purNum);
                var count = (Int64)cmd.ExecuteScalar();
                if (count == tens.Count)
                {
                    return;
                }
            }

            foreach (var t in tens)
            {
                var ev = (htmlDoc2.DocumentNode.SelectSingleNode("//td[2]")?.InnerText ?? "")
                    .Trim();
                var timezone = (htmlDoc2.DocumentNode.SelectSingleNode("//td[1]/span")?.InnerText ?? "")
                    .Trim();
                var dtime = (htmlDoc2.DocumentNode.SelectSingleNode("//td[1]")?.InnerText ?? "")
                    .Trim();
                dtime = dtime.Replace(timezone, "").Trim();
                var dateTime = dtime.ParseDateUn("dd.MM.yyyy HH:mm");
                var tn = new TypeJ()
                {
                    NotificationNumber = purNum, DateTime = dateTime, TimeZone = timezone, TypeFz = "223", Event = ev
                };
                var tender = new TenderJ(tn);
                ParserTender(tender);
            }
        }

        protected int MaxPage(string u)
        {
            if (DownloadString.MaxDownload >= 1000) return 1;
            var s = DownloadString.DownLUserAgentEis(u);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger("cannot get first page from EIS", u);
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(s);
            var maxPageS =
                htmlDoc.DocumentNode.SelectSingleNode("//ul[@class = 'pages']/li[last()]/a/span")?.InnerText ?? "1";
            if (int.TryParse(maxPageS, out var maxP))
            {
                return maxP;
            }

            return 10;
        }
    }
}