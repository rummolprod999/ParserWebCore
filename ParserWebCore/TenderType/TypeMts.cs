namespace ParserWebCore.TenderType
{
    public class TypeMts : AbstractTypeT
    {
        public string PlacingWay { get; set; }
        public string Status { get; set; }
        public string Region { get; set; }

        public override string ToString()
        {
            return
                $"{base.ToString()}, {nameof(PlacingWay)}: {PlacingWay}, {nameof(Status)}: {Status}, {nameof(Region)}: {Region}";
        }
    }
}