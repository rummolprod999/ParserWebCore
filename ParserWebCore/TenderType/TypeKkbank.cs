using System;

namespace ParserWebCore.TenderType
{
    public class TypeKkbank : AbstractTypeT
    {
        public DateTime ScoringDate { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(ScoringDate)}: {ScoringDate}";
        }
    }
}