namespace ParserWebCore.TenderType
{
    public class TypeEurosib : AbstractTypeT
    {
        public string CusName { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(CusName)}: {CusName}";
        }
    }
}