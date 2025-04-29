#region

using System;

#endregion

namespace ParserWebCore.TenderType
{
    public class TypeTekKom
    {
        public string Href { get; set; }
        public string Status { get; set; }
        public string PurNum { get; set; }
        public DateTime DatePub { get; set; }
        public DateTime DateEnd { get; set; }
    }
}