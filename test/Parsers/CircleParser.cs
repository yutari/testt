using System;
using System.Globalization;
using Autodesk.AutoCAD.Geometry;
using test.Parsers.Base;
using test.Utils;

namespace test.Parsers
{
    public static class CircleParser
    {
        public static ParseResult<CircleData> ParseAll(string input)
        {
            var result = new ParseResult<CircleData>();
            foreach (var raw in BaseParserHelper.SplitByDelimiter(input, '-'))
            {
                if (TryParseOne(raw, out CircleData circle, out string error))
                    result.AddValid(circle);
                else
                    result.AddError(error);
            }
            return result;
        }

        private static bool TryParseOne(string input, out CircleData circle, out string error)
        {
            circle = null;
            error = null;

            try
            {
                var parts = input.Split(',');
                if (parts.Length != 2)
                {
                    error = ValidationHelper.FormatError(input);
                    return false;
                }

                var center = ParsePoint(parts[0]);
                if (!double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var radius) || radius <= 0)
                {
                    error = ValidationHelper.ValueError(input, parts[1]);
                    return false;
                }

                circle = new CircleData(center, radius);
                return true;
            }
            catch
            {
                error = ValidationHelper.FormatError(input);
                return false;
            }
        }

        private static Point3d ParsePoint(string s)
        {
            s = s.Trim('(', ')');
            var xy = s.Split(';');
            if (xy.Length != 2) throw new FormatException("Sai định dạng điểm");
            return new Point3d(
                double.Parse(xy[0], CultureInfo.InvariantCulture),
                double.Parse(xy[1], CultureInfo.InvariantCulture),
                0);
        }
    }
}
