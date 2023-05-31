namespace ParserWebCore.TenderType
{
    public class TypeMetal100 : AbstractTypeT
    {
        public string PwName { get; set; }
        public string Status { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(PwName)}: {PwName}, {nameof(Status)}: {Status}";
        }
    }
}