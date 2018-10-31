using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ParserWebCore.TenderType;

namespace ParserWebCore.Tender
{
    public class TenderNaftan: TenderAbstract, ITender
    {
        private readonly TypeNaftan _tn;

        public TenderNaftan(string etpName, string etpUrl, int typeFz, TypeNaftan tn) : base(etpName, etpUrl,
            typeFz)
        {
            _tn = tn;
        }

        public void ParsingTender()
        {
            Console.WriteLine(JsonConvert.SerializeObject(
                _tn, Formatting.Indented,
                new JsonConverter[] {new StringEnumConverter()}));
        }
    }
}