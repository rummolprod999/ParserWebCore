using Microsoft.ML.Data;

namespace ParserWebCore.MlConformity
{
    public class ConformChecker
    {
        [LoadColumn(0)] public int Con { get; set; }
        [LoadColumn(1)] public string Name { get; set; }
    }
}