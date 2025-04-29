#region

using System;

#endregion

namespace ParserWebCore.TenderType
{
    public class TypeUral1 : AbstractTypeT
    {
        public string PwName { get; set; }
        public string Currency { get; set; }
        public DateTime DateScoring { get; set; }
        public string Person { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }
}