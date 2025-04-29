#region

using System;

#endregion

namespace ParserWebCore.TenderType
{
    public class TypeUdsOil : AbstractTypeT
    {
        public string DelivPlace { get; set; }
        public DateTime DateScoring { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(DelivPlace)}: {DelivPlace}, {nameof(DateScoring)}: {DateScoring}";
        }
    }
}