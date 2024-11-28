using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Gemini
{
    public class GeminiContent
    {
        public List<GeminiPart> Parts { get; set; }
        public string Role { get; set; }
    }
}
