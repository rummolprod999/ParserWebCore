#region

using System;

#endregion

namespace ParserWebCore.TenderType
{
    public class TypeLenReg : AbstractTypeT
    {
        public DateTime DateUpd { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(DateUpd)}: {DateUpd}";
        }
    }
}