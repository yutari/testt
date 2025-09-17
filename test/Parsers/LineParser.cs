using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Autodesk.AutoCAD.Geometry;
using test.Parsers.Base;

namespace test.Parsers
{
    public static class LineParser
    {
        // regex: (x;y),(x;y) hoặc (x;y),length,angle
        private static readonly Regex TwoPointPattern =
            new Regex(@"^\(\s*([0-9]+(?:\.[0-9]+)?)\s*;\s*([0-9]+(?:\.[0-9]+)?)\s*\)\s*,\s*\(\s*([0-9]+(?:\.[0-9]+)?)\s*;\s*([0-9]+(?:\.[0-9]+)?)\s*\)$");

        private static readonly Regex LenAngPattern =
            new Regex(@"^\(\s*([0-9]+(?:\.[0-9]+)?)\s*;\s*([0-9]+(?:\.[0-9]+)?)\s*\)\s*,\s*([0-9]+(?:\.[0-9]+)?)\s*,\s*([0-9]+(?:\.[0-9]+)?)$");

        public static ParseResult<LineData> ParseAll(string input)
        {
            var result = new ParseResult<LineData>();
            foreach (var raw in BaseParserHelper.SplitByDelimiter(input, '-'))
            {
                if (TryParseOne(raw, out LineData data, out string error))
                    result.AddValid(data);
                else
                    result.AddError(error);
            }
            return result;
        }

        private static bool TryParseOne(string raw, out LineData line, out string error)
        {
            line = null;
            error = null;

            // (x;y),(x;y)
            var m1 = TwoPointPattern.Match(raw);
            if (m1.Success)
            {
                try
                {
                    double x1 = double.Parse(m1.Groups[1].Value, CultureInfo.InvariantCulture);
                    double y1 = double.Parse(m1.Groups[2].Value, CultureInfo.InvariantCulture);
                    double x2 = double.Parse(m1.Groups[3].Value, CultureInfo.InvariantCulture);
                    double y2 = double.Parse(m1.Groups[4].Value, CultureInfo.InvariantCulture);

                    line = new LineData(new Point3d(x1, y1, 0), new Point3d(x2, y2, 0));
                    return true;
                }
                catch
                {
                    error = ParseValidator.FormatError(raw);
                    return false;
                }
            }

            // (x;y),len,ang
            var m2 = LenAngPattern.Match(raw);
            if (m2.Success)
            {
                try
                {
                    double x1 = double.Parse(m2.Groups[1].Value, CultureInfo.InvariantCulture);
                    double y1 = double.Parse(m2.Groups[2].Value, CultureInfo.InvariantCulture);
                    double len = double.Parse(m2.Groups[3].Value, CultureInfo.InvariantCulture);
                    double ang = double.Parse(m2.Groups[4].Value, CultureInfo.InvariantCulture);

                    if (!ParseValidator.CheckAngle(ang))
                    {
                        error = ParseValidator.ValueError(raw, m2.Groups[4].Value);
                        return false;
                    }

                    double rad = ang * Math.PI / 180.0;
                    double x2 = x1 + len * Math.Cos(rad);
                    double y2 = y1 + len * Math.Sin(rad);

                    line = new LineData(new Point3d(x1, y1, 0), new Point3d(x2, y2, 0));
                    return true;
                }
                catch
                {
                    error = ParseValidator.FormatError(raw);
                    return false;
                }
            }

            // Sai format
            error = ParseValidator.FormatError(raw);
            return false;
        }
    }
}
