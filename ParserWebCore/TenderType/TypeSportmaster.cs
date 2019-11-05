using System.Collections.Generic;

namespace ParserWebCore.TenderType
{
    public class TypeSportmaster: AbstractTypeT
    {
        public string Status { get; set; }
        public Dictionary<string, string> Attach { get; set; }
    }
}