using System;

namespace ParserWebCore.TenderType
{
    public class TypeZmoRts
    {
        public string Id { get; set; }
        public string LotId { get; set; }
        public string PurName { get; set; }
        public string Nmck { get; set; }
        public string CusName { get; set; }
        public string StateString { get; set; }
        public DateTime PublicationDate { get; set; }
        public DateTime EndDate { get; set; }
        public string[] DeliveryKladrRegionName { get; set; }
        public string Host { get; set; }

        public override string ToString()
        {
            return
                $"{nameof(Id)}: {Id}, {nameof(LotId)}: {LotId}, {nameof(PurName)}: {PurName}, {nameof(Nmck)}: {Nmck}, {nameof(CusName)}: {CusName}, {nameof(StateString)}: {StateString}, {nameof(PublicationDate)}: {PublicationDate}, {nameof(EndDate)}: {EndDate}, {nameof(DeliveryKladrRegionName)}: {DeliveryKladrRegionName}, {nameof(Host)}: {Host}";
        }
    }
}