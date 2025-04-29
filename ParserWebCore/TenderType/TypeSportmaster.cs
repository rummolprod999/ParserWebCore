#region

using System.Collections.Generic;

#endregion

namespace ParserWebCore.TenderType
{
    public class TypeSportmaster : AbstractTypeT
    {
        public string Status { get; set; }
        public string PwName { get; set; }
        public Dictionary<string, string> Attach { get; set; }

        public override string ToString()
        {
            return
                $"{base.ToString()}, {nameof(Status)}: {Status}, {nameof(PwName)}: {PwName}, {nameof(Attach)}: {Attach}";
        }
    }
}