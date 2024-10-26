using System;

namespace ParserWebCore.TenderType
{
    public class TypeTekMarket : AbstractTypeT
    {
        public string Status { get; set; }
        public string PwName { get; set; }
        public string Down { get; set; }
        public DateTime DateBid { get; set; }
        public DateTime DateScor { get; set; }

        public override string ToString()
        {
            return
                $"{base.ToString()}, {nameof(Status)}: {Status}, {nameof(PwName)}: {PwName}, {nameof(Down)}: {Down}, {nameof(DateBid)}: {DateBid}, {nameof(DateScor)}: {DateScor}";
        }
    }
}