using Abyss.Web.Data;
using Abyss.Web.Helpers.Interfaces;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Api;
using Microsoft.ML.Runtime.Data;
using System.IO;
using System.Threading.Tasks;

namespace Abyss.Web.Helpers
{
    public class WhoSaidHelper : IWhoSaidHelper
    {
        private static readonly string ModelPath = "Models/whosaidit.zip";
        private PredictionFunction<InputData, Prediction> _predictionFunction;
        private async Task<PredictionFunction<InputData, Prediction>> GetPredictionFunction()
        {
            if (_predictionFunction == null)
            {
                await Task.Run(() =>
                {
                    var env = new LocalEnvironment();
                    ITransformer model;
                    using (var stream = File.OpenRead(ModelPath))
                    {
                        model = TransformerChain.LoadFrom(env, stream);
                    }
                    _predictionFunction = model.MakePredictionFunction<InputData, Prediction>(env);
                });
            }
            return _predictionFunction;
        }

        public async Task<WhoSaid> WhoSaid(string message)
        {
            var predictionFunction = await GetPredictionFunction();
            var prediction = predictionFunction.Predict(new InputData { Text = message });
            return new WhoSaid
            {
                Name = prediction.PredictedClass,
                Message = message
            };
        }
    }

    internal class InputData
    {
        public string Label { get; set; }
        public string Text { get; set; }
    }

    internal class Prediction
    {
        [ColumnName("Data")]
        public string PredictedClass { get; set; }
    }
}
