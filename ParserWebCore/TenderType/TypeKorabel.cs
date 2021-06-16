namespace ParserWebCore.TenderType
{
    public class TypeKorabel : AbstractTypeT
    {
        public string CusName { get; set; }
        public string DelivTerm { get; set; }
        public string Region { get; set; }

        public override string ToString()
        {
            return
                $"{base.ToString()}, {nameof(CusName)}: {CusName}, {nameof(DelivTerm)}: {DelivTerm}, {nameof(Region)}: {Region}";
        }
    }
}