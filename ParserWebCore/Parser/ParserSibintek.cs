namespace ParserWebCore.Parser
{
    public class ParserSibintek: ParserAbstract, IParser
    {
        private const int Count = 5;

        public void Parsing()
        {
            Parse(ParsingSibintek);
        }

        private void ParsingSibintek()
        {
        }
    }
}