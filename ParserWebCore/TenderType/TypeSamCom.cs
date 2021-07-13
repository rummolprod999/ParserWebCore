using System.Collections.Generic;

namespace ParserWebCore.TenderType
{
    public class TypeSamCom : AbstractTypeT
    {
        public string Nmck { get; set; }
        public List<Attachment> Attachments { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(Nmck)}: {Nmck}, {nameof(Attachments)}: {Attachments}";
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