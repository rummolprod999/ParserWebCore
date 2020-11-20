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

        public override string ToString()
        {
            return
                $"{nameof(Href)}: {Href}, {nameof(PurNum)}: {PurNum}, {nameof(PurName)}: {PurName}, {nameof(DatePub)}: {DatePub}, {nameof(DateEnd)}: {DateEnd}";
        }
    }
}