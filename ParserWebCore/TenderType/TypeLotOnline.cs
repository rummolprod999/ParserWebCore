namespace ParserWebCore.TenderType
{
    public class TypeLotOnline : AbstractTypeT
    {
        public string OrgName { get; set; }
        public string OrgInn { get; set; }
        public string RegionName { get; set; }
        public string Status { get; set; }
        public string Nmck { get; set; }

        public override string ToString()
        {
            return
                $"{base.ToString()}, {nameof(OrgName)}: {OrgName}, {nameof(OrgInn)}: {OrgInn}, {nameof(RegionName)}: {RegionName}, {nameof(Status)}: {Status}";
        }
    }
}