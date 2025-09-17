using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Autodesk.AutoCAD.Geometry;
using test.Parsers.Base;

namespace test.Parsers
{
    public static class CircleParser
    {
        // Regex: (x;y),radius
        private static readonly Regex CirclePattern =
            new Regex(@"^\(\s*([0-9]+(?:\.[0-9]+)?)\s*;\s*([0-9]+(?:\.[0-9]+)?)\s*\)\s*,\s*([0-9]+(?:\.[0-9]+)?)$");

        public static ParseResult<CircleData> ParseAll(string input)
        {
            var result = new ParseResult<CircleData>();
            foreach (var raw in BaseParserHelper.SplitByDelimiter(input, '-'))
            {
                if (TryParseOne(raw, out CircleData data, out string error))
                    result.AddValid(data);
                else
                    result.AddError(error);
            }
            return result;
        }

        private static bool TryParseOne(string raw, out CircleData circle, out string error)
        {
            circle = null;
            error = null;

            var m = CirclePattern.Match(raw);
            if (!m.Success)
            {
                error = ParseValidator.FormatError(raw);
                return false;
            }

            try
            {
                double x = double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
                double y = double.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
                double r = double.Parse(m.Groups[3].Value, CultureInfo.InvariantCulture);

                if (!ParseValidator.CheckPositive(r))
                {
                    error = ParseValidator.ValueError(raw, m.Groups[3].Value);
                    return false;
                }

                circle = new CircleData(new Point3d(x, y, 0), r);
                return true;
            }
            catch
            {
                error = ParseValidator.FormatError(raw);
                return false;
            }
        }
    }
}
