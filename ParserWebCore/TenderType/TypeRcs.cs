#region

using System.Collections.Generic;

#endregion

namespace ParserWebCore.TenderType
{
    public class TypeRcs : AbstractTypeT
    {
        public string Nmck { get; set; }
        public string Status { get; set; }
        public string Region { get; set; }
        public List<TypeSamCom.Attachment> Attachments { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(Nmck)}: {Nmck}, {nameof(Status)}: {Status}, {nameof(Region)}: {Region}";
        }
    }
}