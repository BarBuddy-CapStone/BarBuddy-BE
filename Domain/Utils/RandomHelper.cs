using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Utils
{
    public static class RandomHelper
    {
        private static Random _random;

        public static string GenerateRandomNumberString(int length = 6)
        {
            _random ??= new Random();
            const string chars = "0123456789";
            char[] result = new char[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = chars[_random.Next(chars.Length)];
            }

            return new string(result);
        }
    }
}
