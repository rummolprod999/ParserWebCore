#region

using System.Collections.Generic;

#endregion

namespace ParserWebCore.TenderType
{
    public class TypeFbRts : AbstractTypeT
    {
        public string Status { get; set; }

        public string Nmck { get; set; }

        public string OrgName { get; set; }

        public string CusName { get; set; }

        public string Region { get; set; }

        public string PrintForm { get; set; }

        public List<TypeFbRtsPos> positions { get; set; }

        public override string ToString()
        {
            return
                $"{base.ToString()}, {nameof(Status)}: {Status}, {nameof(Nmck)}: {Nmck}, {nameof(OrgName)}: {OrgName}, {nameof(CusName)}: {CusName}, {nameof(Region)}: {Region}, {nameof(positions)}: {positions}";
        }
    }
}