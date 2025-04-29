#region

using System.Collections.Generic;

#endregion

namespace ParserWebCore.TenderType
{
    public class TypeZakupMos : AbstractTypeT
    {
        public string Status { get; set; }
        public int Id { get; set; }
        public int NeedId { get; set; }
        public int TenderId { get; set; }
        public int AuctionId { get; set; }
        public string RegionName { get; set; }
        public string OrgName { get; set; }
        public string OrgInn { get; set; }
        public decimal Nmck { get; set; }
        public List<Customer> Customers { get; set; }

        public class Customer
        {
            public Customer(string cusName, string cusInn)
            {
                CusName = cusName;
                CusInn = cusInn;
            }

            public string CusName { get; set; }
            public string CusInn { get; set; }
        }
    }
}