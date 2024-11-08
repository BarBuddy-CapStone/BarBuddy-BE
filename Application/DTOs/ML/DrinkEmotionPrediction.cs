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
        [LoadColumn(0)]
        public string? Emotion { get; set; }
        [LoadColumn(1)]
        public string? DrinkName { get; set; }
        //[LoadColumn(2)]
        //public string? AccountName { get; set; }

    }

}
