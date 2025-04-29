#region

using System.Collections.Generic;

#endregion

namespace ParserWebCore.TenderType
{
    public class TypeRuscable : AbstractTypeT
    {
        public string Status { get; set; }
        public string DelivPlace { get; set; }
        public string DelivAddInfo { get; set; }
        public List<PurObj> purObj { get; set; }

        public override string ToString()
        {
            return
                $"{base.ToString()}, {nameof(Status)}: {Status}, {nameof(DelivPlace)}: {DelivPlace}, {nameof(DelivAddInfo)}: {DelivAddInfo}, {nameof(purObj)}: {purObj}";
        }


        public class PurObj
        {
            public string Name { get; set; }
            public string Quant { get; set; }
            public string Okei { get; set; }


            public override string ToString()
            {
                return $"{nameof(Name)}: {Name}, {nameof(Quant)}: {Quant}, {nameof(Okei)}: {Okei}";
            }
        }
    }
}