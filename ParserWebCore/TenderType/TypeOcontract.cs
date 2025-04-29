#region

using System;

#endregion

namespace ParserWebCore.TenderType
{
    public class TypeOcontract : AbstractTypeT
    {
        public string OrgName { get; set; }
        public string Currency { get; set; }
        public string Nmck { get; set; }
        public string DeliveryPlace { get; set; }
        public string DeliveryTime { get; set; }
        public string DeliveryTerms { get; set; }
        public string Status { get; set; }
        public DateTime BiddingDate { get; set; }
        public string FormOfPayment { get; set; }
        public string PaymentTerms { get; set; }
        public string PlacingWay { get; set; }

        public override string ToString()
        {
            return
                $"{base.ToString()}, {nameof(OrgName)}: {OrgName}, {nameof(Currency)}: {Currency}, {nameof(Nmck)}: {Nmck}, {nameof(DeliveryPlace)}: {DeliveryPlace}, {nameof(DeliveryTime)}: {DeliveryTime}, {nameof(DeliveryTerms)}: {DeliveryTerms}, {nameof(Status)}: {Status}, {nameof(BiddingDate)}: {BiddingDate}, {nameof(FormOfPayment)}: {FormOfPayment}, {nameof(PaymentTerms)}: {PaymentTerms}, {nameof(PlacingWay)}: {PlacingWay}";
        }
    }
}