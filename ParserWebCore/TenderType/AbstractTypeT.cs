using System;

namespace ParserWebCore.TenderType
{
    public abstract class AbstractTypeT
    {
        public string Href { get; set; }
        public string PurNum { get; set; }
        public string PurName { get; set; }
        public DateTime DatePub { get; set; }
        public DateTime DateEnd { get; set; }
    }
}