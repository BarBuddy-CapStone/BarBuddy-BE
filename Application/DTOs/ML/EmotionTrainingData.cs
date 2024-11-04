using Microsoft.ML.Data;

public class EmotionTrainingData
{
    [LoadColumn(0)]
    public string Text { get; set; }

    [LoadColumn(1)]
    public string Label { get; set; }
} 