using Abyss.Web.Data;
using Abyss.Web.Helpers.Interfaces;
using Microsoft.Extensions.ML;
using Microsoft.ML.Data;
using System.Threading.Tasks;

namespace Abyss.Web.Helpers
{
    public class WhoSaidHelper : IWhoSaidHelper
    {
        private readonly PredictionEnginePool<InputData, Prediction> _predictionEnginePool;

        public WhoSaidHelper(PredictionEnginePool<InputData, Prediction> predictionEnginePool)
        {
            _predictionEnginePool = predictionEnginePool;
        }

        public async Task<WhoSaid> WhoSaid(string message)
        {
            return await Task.Run(() =>
            {
                var prediction = _predictionEnginePool.Predict(new InputData { Text = message });
                return new WhoSaid
                {
                    Name = prediction.PredictedClass,
                    Message = message
                };
            });
        }
    }

    public class InputData
    {
        public string Label { get; set; }
        public string Text { get; set; }
    }

    public class Prediction
    {
        [ColumnName("Data")]
        public string PredictedClass { get; set; }
    }
}
