#region

using System.Collections.Generic;

#endregion

namespace ParserWebCore.TenderType
{
    public class TypeUral2 : AbstractTypeT
    {
        public List<Attachment> Attachments { get; set; }

        public class Attachment
        {
            public string Name { get; set; }
            public string Url { get; set; }
        }
    }
}