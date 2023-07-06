namespace ParserWebCore.TenderType
{
    public class TypeSegz : AbstractTypeT
    {
        public string PwName { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(PwName)}: {PwName}";
        }
    }
}