using System;

namespace ParserWebCore.TenderType
{
    public class TypeTekRn
    {
        public string PurNum { get; set; }
        public string OrgName { get; set; }
        public string PurName { get; set; }
        public string Href { get; set; }
        public string Nmck { get; set; }
        public string Status { get; set; }
        public DateTime DatePub { get; set; }
        public DateTime DateEnd { get; set; }
        public DateTime Scoring { get; set; }

        public override string ToString()
        {
            return
                $"{nameof(PurNum)}: {PurNum}, {nameof(OrgName)}: {OrgName}, {nameof(PurName)}: {PurName}, {nameof(Href)}: {Href}, {nameof(Nmck)}: {Nmck}, {nameof(Status)}: {Status}, {nameof(DatePub)}: {DatePub}, {nameof(DateEnd)}: {DateEnd}, {nameof(Scoring)}: {Scoring}";
        }
    }
}