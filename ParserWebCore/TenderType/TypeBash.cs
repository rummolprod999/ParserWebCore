#region

using System;

#endregion

namespace ParserWebCore.TenderType
{
    public class TypeBash : AbstractTypeT
    {
        public string Nmck { get; set; }

        public string DelivPlace { get; set; }

        public string Status { get; set; }

        public DateTime ContractDate { get; set; }

        public string Id { get; set; }

        public override string ToString()
        {
            return
                $"{base.ToString()}, {nameof(Nmck)}: {Nmck}, {nameof(DelivPlace)}: {DelivPlace}, {nameof(Status)}: {Status}, {nameof(ContractDate)}: {ContractDate}, {nameof(Id)}: {Id}";
        }
    }
}