using System;
using System.Text.RegularExpressions;
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
            return Regex.Replace(nmcK.GetDateFromRegex(@"^([\d \.]+)\s"), @"\s+", "");
        }
    }
}