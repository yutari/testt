using System;
using System.Globalization;

namespace test.Parsers.Base
{
    public static class NumberParser
    {
        public static bool TryParsePositive(string s, out double value)
        {
            if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            {
                return value > 0;
            }
            return false;
        }

        public static bool TryParseDouble(string s, out double value)
        {
            return double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }
    }
}

