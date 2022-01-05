using Abyss.Web.Data;
using Abyss.Web.Helpers.Interfaces;
using Microsoft.Extensions.ML;
using Microsoft.ML.Data;
using System.Diagnostics.CodeAnalysis;

namespace Abyss.Web.Helpers;

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
                Name = prediction.PredictedLabel,
                Message = message
            };
        });
    }
}

public class InputData
{
    [ColumnName("sender_name"), LoadColumn(0)]
    public string? SenderName { get; set; }


    [ColumnName("timestamp_ms"), LoadColumn(1)]
    public float TimestampMS { get; set; }


    [ColumnName("content"), LoadColumn(2)]
    public string? Text { get; set; }


    [ColumnName("type"), LoadColumn(3)]
    public string? Type { get; set; }
}

public class Prediction
{
    [ColumnName("PredictedLabel"), NotNull]
    public string? PredictedLabel { get; set; }
    public float[]? Score { get; set; }
}
