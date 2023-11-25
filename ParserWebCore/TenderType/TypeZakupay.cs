using System.Collections.Generic;

namespace ParserWebCore.TenderType
{
    public class TypeZakupay : AbstractTypeT
    {
        public string Status { get; set; }
        public string DelivTerm { get; set; }
        public string DelivPlace { get; set; }
        public List<TypeObject> ObjectsPurchase { get; set; }

        public override string ToString()
        {
            return
                $"{base.ToString()}, {nameof(Status)}: {Status}, {nameof(DelivTerm)}: {DelivTerm}, {nameof(DelivPlace)}: {DelivPlace}, {nameof(ObjectsPurchase)}: {ObjectsPurchase}";
        }


        public class TypeObject
        {
            public string Name { get; set; }
            public string Okei { get; set; }
            public string Quantity { get; set; }
            public string OKPD { get; set; }


            public override string ToString()
            {
                return
                    $"{nameof(Name)}: {Name}, {nameof(Okei)}: {Okei}, {nameof(Quantity)}: {Quantity}, {nameof(OKPD)}: {OKPD}";
            }
        }
    }
}