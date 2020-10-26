using System.Collections.Generic;

namespace ParserWebCore.TenderType
{
    public class TypeTpta: AbstractTypeT
    {
        public List<TypeObjectTpta> ObjectsPurchase { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(ObjectsPurchase)}: {ObjectsPurchase}";
        }
    }
}