using Microsoft.ML.Data;

namespace ParserWebCore.MlConformity
{
    public class CheckerPrediction
    {
        [ColumnName("PredictedLabel")]
        public int Con;
    }
}