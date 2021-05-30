using System.Collections.Generic;

namespace ParserWebCore.TenderType
{
    public class TypeUralair : AbstractTypeT
    {
        public List<Attachment> Attachments { get; set; }
        public string Status { get; set; }

        public override string ToString()
        {
            return
                $"{base.ToString()}, {nameof(Attachments)}: {string.Join(" ", Attachments)}, {nameof(Status)}: {Status}";
        }

        public class Attachment
        {
            public string Name { get; set; }
            public string Url { get; set; }

            public override string ToString()
            {
                return $"{nameof(Name)}: {Name}, {nameof(Url)}: {Url}";
            }
        }
    }
}