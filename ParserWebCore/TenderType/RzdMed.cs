using Newtonsoft.Json.Linq;

namespace ParserWebCore.TenderType
{
    public class RzdMed : AbstractTypeT
    {
        public string Status { get; set; }

        public JToken token { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(Status)}: {Status}";
        }
    }
}