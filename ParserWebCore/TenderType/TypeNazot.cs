#region

using System;

#endregion

namespace ParserWebCore.TenderType
{
    public class TypeNazot : AbstractTypeT
    {
        public DateTime DateBind { get; set; }

        public string DelivTerm { get; set; }
        public string Notice { get; set; }

        public override string ToString()
        {
            return
                $"{base.ToString()}, {nameof(DateBind)}: {DateBind}, {nameof(DelivTerm)}: {DelivTerm}, {nameof(Notice)}: {Notice}";
        }
    }
}