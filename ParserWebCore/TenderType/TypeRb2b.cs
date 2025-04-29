#region

using Newtonsoft.Json.Linq;

#endregion

namespace ParserWebCore.TenderType
{
    public class TypeRb2b : AbstractTypeT
    {
        public string Status { get; set; }
        public JToken JsonT { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(Status)}: {Status}";
        }
    }
}