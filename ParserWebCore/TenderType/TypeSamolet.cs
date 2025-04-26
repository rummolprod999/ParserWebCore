using System;

namespace ParserWebCore.TenderType
{
    public class TypeSamolet: AbstractTypeT
    {
        public string Id { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(Id)}: {Id}";
        }
    }
}