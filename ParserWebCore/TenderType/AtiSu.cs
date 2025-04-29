#region

using System;

#endregion

namespace ParserWebCore.TenderType
{
    public class AtiSu : AbstractTypeT
    {
        public string Nmck { get; set; }
        public DateTime BiddingDate { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(Nmck)}: {Nmck}, {nameof(BiddingDate)}: {BiddingDate}";
        }
    }
}