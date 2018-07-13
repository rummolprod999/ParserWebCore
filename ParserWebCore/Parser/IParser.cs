using ParserWebCore.Tender;

namespace ParserWebCore.Parser
{
    public interface IParser
    {
        void Parsing();
        void ParserTender(ITender t);
    }
}