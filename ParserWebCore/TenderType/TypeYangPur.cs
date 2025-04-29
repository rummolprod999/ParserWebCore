#region

using System.Collections.Generic;

#endregion

namespace ParserWebCore.TenderType
{
    public class TypeYangPur : AbstractTypeT
    {
        public List<(string, string)> Attachments { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(Attachments)}: {Attachments}";
        }
    }
}