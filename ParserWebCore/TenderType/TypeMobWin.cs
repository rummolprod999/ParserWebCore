using System.Collections.Generic;

namespace ParserWebCore.TenderType
{
    public class TypeMobWin : AbstractTypeT
    {
        public List<TypeSamCom.Attachment> Attachments { get; set; }

        public string Notice { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(Attachments)}: {Attachments}, {nameof(Notice)}: {Notice}";
        }
    }
}