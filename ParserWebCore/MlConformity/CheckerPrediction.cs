#region

using Microsoft.ML.Data;

#endregion

namespace ParserWebCore.MlConformity
{
    public class CheckerPrediction
    {
        [ColumnName("PredictedLabel")] public int Con;
    }
}