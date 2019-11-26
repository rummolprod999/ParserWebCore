using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.Tender;

namespace ParserWebCore.Parser
{
    public abstract class ParserAbstract
    {
        protected void Parse(Action op)
        {
            Log.Logger("Время начала парсинга");
            op?.Invoke();
            Log.Logger("Добавили Tender", TenderAbstract.Count);
            Log.Logger("Обновили Tender", TenderAbstract.UpCount);
            Log.Logger("Время окончания парсинга");
        }

        protected void ParserTender(ITender t)
        {
            try
            {
                t.ParsingTender();
            }
            catch (Exception e)
            {
                Log.Logger($"Exeption in {t.GetType()}", e);
            }
        }

        protected string GetPriceFromString(string nmcK)
        {
            return Regex.Replace(nmcK.GetDataFromRegex(@"^([\d \.]+)\s"), @"\s+", "");
        }
        
        public List<JToken> GetElements(JToken j, string s)
        {
            var els = new List<JToken>();
            var elsObj = j.SelectToken(s);
            if (elsObj == null || elsObj.Type == JTokenType.Null) return els;
            switch (elsObj.Type)
            {
                case JTokenType.Object:
                    els.Add(elsObj);
                    break;
                case JTokenType.Array:
                    els.AddRange(elsObj);
                    break;
            }

            return els;
        }
    }
}