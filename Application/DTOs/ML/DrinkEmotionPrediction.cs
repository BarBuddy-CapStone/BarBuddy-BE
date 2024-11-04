using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.ML
{
    public class DrinkEmotionPrediction
    {
        [ColumnName("PredictedLabel")]
        public string EmotionCategory { get; set; }

        [ColumnName("Score")]
        public float[] Score { get; set; }
    }
}
