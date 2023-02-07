using System.Collections.Generic;

namespace ParserWebCore.TenderType
{
    public class TypeUos : AbstractTypeT
    {
        public List<Attachment> Attachments { get; set; }

        public string Notice { get; set; }

        public class Attachment
        {
            public string Name { get; set; }
            public string Url { get; set; }
        }
    }
}