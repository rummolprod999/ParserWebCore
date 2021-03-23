namespace ParserWebCore.TenderType
{
    public class TypeKuzocm : AbstractTypeT
    {
        public string CusName { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(CusName)}: {CusName}";
        }
    }
}