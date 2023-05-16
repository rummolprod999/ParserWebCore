namespace ParserWebCore.TenderType
{
    public class TypeProgress : AbstractTypeT
    {
        public string Status { get; set; }
        public string PwName { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(Status)}: {Status}, {nameof(PwName)}: {PwName}";
        }
    }
}