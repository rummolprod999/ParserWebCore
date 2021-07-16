using System.Collections.Generic;

namespace ParserWebCore.TenderType
{
    public class TypeStniva : AbstractTypeT
    {
        public List<string> Customers { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(Customers)}: {string.Join(", ", Customers)}";
        }
    }
}