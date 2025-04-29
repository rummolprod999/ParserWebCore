#region

using System;
using System.Data;
using System.Globalization;
using System.IO;
using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using Microsoft.ML.Transforms.Conversions;
using MySql.Data.MySqlClient;
using ParserWebCore.BuilderApp;
using ParserWebCore.Logger;

#endregion

namespace ParserWebCore.MlConformity
{
    public class ConformityLearner
    {
        private readonly MLContext _mlContext;
        private PredictionEngine<ConformChecker, CheckerPrediction> _predEngine;
        private ITransformer _trainedModel;
        private IDataView _trainingDataView;

        public ConformityLearner()
        {
            CreatePathModels();
            _mlContext = new MLContext(0);
            ModelLearner();
        }

        private string TrainDataPath => Path.Combine(AppBuilder.Path, "Data", "placing_way.tsv");
        private string TestDataPath => Path.Combine(AppBuilder.Path, "Data", "placing_way_test.tsv");
        private string ModelPath => Path.Combine(AppBuilder.Path, "Models", "model.zip");

        private void CreatePathModels()
        {
            if (!Directory.Exists(Path.Combine(AppBuilder.Path, "Models")))
            {
                Directory.CreateDirectory(Path.Combine(AppBuilder.Path, "Models"));
            }
        }

        private void ModelLearner()
        {
            if (new FileInfo(ModelPath).Exists)
            {
                return;
            }

            _trainingDataView = _mlContext.Data.CreateTextReader<ConformChecker>(true).Read(TrainDataPath);
            var pipeline = ProcessData();
            var trainingPipeline = BuildAndTrainModel(_trainingDataView, pipeline);
            Evaluate();
        }

        private EstimatorChain<ITransformer> ProcessData()
        {
            var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("Con", "Label")
                .Append(_mlContext.Transforms.Text.FeaturizeText("Name", "NameFeaturized"))
                .Append(_mlContext.Transforms.Concatenate("Features", "NameFeaturized"))
                .AppendCacheCheckpoint(_mlContext);
            return pipeline;
        }

        private EstimatorChain<KeyToValueMappingTransformer> BuildAndTrainModel(IDataView trainingDataView,
            EstimatorChain<ITransformer> pipeline)
        {
            var trainer = new SdcaMultiClassTrainer(_mlContext);
            var trainingPipeline = pipeline.Append(trainer)
                .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));
            _trainedModel = trainingPipeline.Fit(trainingDataView);
            _predEngine = _trainedModel.CreatePredictionEngine<ConformChecker, CheckerPrediction>(_mlContext);
            var conf = new ConformChecker
            {
                Name = "Электронный аукцион"
            };
            var prediction = _predEngine.Predict(conf);
            SaveModelAsFile(_mlContext, _trainedModel);
            return trainingPipeline;
        }

        private void SaveModelAsFile(MLContext mlContext, ITransformer model)
        {
            using (var fs = new FileStream(ModelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                mlContext.Model.Save(model, fs);
            }

            Console.WriteLine("The model is saved to {0}", ModelPath);
        }

        private void Evaluate()
        {
            Console.WriteLine(
                $"=============== Evaluating to get model's accuracy metrics - Starting time: {DateTime.Now.ToString()} ===============");
            var testDataView = _mlContext.Data.CreateTextReader<ConformChecker>(true).Read(TestDataPath);
            var testMetrics = _mlContext.MulticlassClassification.Evaluate(_trainedModel.Transform(testDataView));
            Console.WriteLine(
                $"=============== Evaluating to get model's accuracy metrics - Ending time: {DateTime.Now.ToString(CultureInfo.InvariantCulture)} ===============");
            Console.WriteLine(
                "*************************************************************************************************************");
            Console.WriteLine("*       Metrics for Multi-class Classification model - Test Data     ");
            Console.WriteLine(
                "*------------------------------------------------------------------------------------------------------------");
            Console.WriteLine($"*       MicroAccuracy:    {testMetrics.AccuracyMicro:0.###}");
            Console.WriteLine($"*       MacroAccuracy:    {testMetrics.AccuracyMacro:0.###}");
            Console.WriteLine($"*       LogLoss:          {testMetrics.LogLoss:#.###}");
            Console.WriteLine($"*       LogLossReduction: {testMetrics.LogLossReduction:#.###}");
            Console.WriteLine(
                "*************************************************************************************************************");
        }

        public void PredictConformity(DataRowCollection dr, MySqlConnection connect)
        {
            ITransformer loadedModel;
            using (var stream = new FileStream(ModelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                loadedModel = _mlContext.Model.Load(stream);
            }


            _predEngine = loadedModel.CreatePredictionEngine<ConformChecker, CheckerPrediction>(_mlContext);
            UpdateConformity(dr, connect);
        }

        private void UpdateConformity(DataRowCollection dr, MySqlConnection connect)
        {
            foreach (DataRow o in dr)
            {
                var idPw = (int)o["id_placing_way"];
                var namePw = (string)o["name"];
                var singleConf = new ConformChecker { Name = namePw };
                var prediction = _predEngine.Predict(singleConf);
                var res = prediction.Con;
                var updateConf =
                    $"UPDATE {AppBuilder.Prefix}placing_way SET conformity = @conformity WHERE id_placing_way = @id_placing_way";
                var cmd2 = new MySqlCommand(updateConf, connect);
                cmd2.Prepare();
                cmd2.Parameters.AddWithValue("@conformity", res);
                cmd2.Parameters.AddWithValue("@id_placing_way", idPw);
                cmd2.ExecuteNonQuery();
                Log.Logger($"Add new conformity for {idPw}   Name: {namePw} Conformity: {res}");
            }
        }
    }
}