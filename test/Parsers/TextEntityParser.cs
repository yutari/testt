using Autodesk.AutoCAD.Geometry;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using test.Parsers.Base;
using test.Utils;

namespace test.Parsers
{
    public static class TextEntityParser
    {
        // regex cho kiểu 1: (x1;y1),(x2;y2),"text"D/M
        private static readonly Regex TwoPointPattern =
            new Regex(@"^\(\s*([0-9]+(?:\.[0-9]+)?)\s*;\s*([0-9]+(?:\.[0-9]+)?)\s*\)\s*," +
                      @"\(\s*([0-9]+(?:\.[0-9]+)?)\s*;\s*([0-9]+(?:\.[0-9]+)?)\s*\)\s*," +
                      @"""(.*)""([DM])$");

        // regex cho kiểu 2: (x;y),rotation,length,"text"D/M
        private static readonly Regex RotLenPattern =
            new Regex(@"^\(\s*([0-9]+(?:\.[0-9]+)?)\s*;\s*([0-9]+(?:\.[0-9]+)?)\s*\)\s*," +
                      @"([0-9]+(?:\.[0-9]+)?)\s*," +
                      @"([0-9]+(?:\.[0-9]+)?)\s*," +
                      @"""(.*)""([DM])$");

        public static ParseResult<TextData> ParseAll(string input)
        {
            var result = new ParseResult<TextData>();
            var parts = BaseParserHelper.SplitByDelimiter(input, '-');

            foreach (var raw in parts)
            {
                if (TryParseOne(raw.Trim(), out TextData data, out string error))
                    result.AddValid(data);
                else
                    result.AddError(error);
            }

            return result;
        }

        private static bool TryParseOne(string raw, out TextData text, out string error)
        {
            text = null;
            error = null;

            if (string.IsNullOrWhiteSpace(raw))
            {
                error = ValidationHelper.FormatError(raw);
                return false;
            }

            // Kiểu 1: 2 điểm
            var m1 = TwoPointPattern.Match(raw);
            if (m1.Success)
            {
                try
                {
                    double x1 = double.Parse(m1.Groups[1].Value, CultureInfo.InvariantCulture);
                    double y1 = double.Parse(m1.Groups[2].Value, CultureInfo.InvariantCulture);
                    double x2 = double.Parse(m1.Groups[3].Value, CultureInfo.InvariantCulture);
                    double y2 = double.Parse(m1.Groups[4].Value, CultureInfo.InvariantCulture);

                    string content = m1.Groups[5].Value.Replace("/n", Environment.NewLine);
                    string typeStr = m1.Groups[6].Value;

                    var p1 = new Point3d(x1, y1, 0);
                    var p2 = new Point3d(x2, y2, 0);

                    double dx = p2.X - p1.X;
                    double dy = p2.Y - p1.Y;
                    double rotation = Math.Atan2(dy, dx);
                    double width = Math.Sqrt(dx * dx + dy * dy);

                    var type = typeStr == "D" ? TextType.DBText : TextType.MText;
                    text = new TextData(p1, rotation, width, content, type);
                    return true;
                }
                catch
                {
                    error = ValidationHelper.FormatError(raw);
                    return false;
                }
            }

            // Kiểu 2: (x;y),rotation,length,"text"D/M
            var m2 = RotLenPattern.Match(raw);
            if (m2.Success)
            {
                try
                {
                    double x = double.Parse(m2.Groups[1].Value, CultureInfo.InvariantCulture);
                    double y = double.Parse(m2.Groups[2].Value, CultureInfo.InvariantCulture);
                    double rotDeg = double.Parse(m2.Groups[3].Value, CultureInfo.InvariantCulture);
                    double length = double.Parse(m2.Groups[4].Value, CultureInfo.InvariantCulture);

                    string content = m2.Groups[5].Value.Replace("/n", Environment.NewLine);
                    string typeStr = m2.Groups[6].Value;

                    var p = new Point3d(x, y, 0);
                    double rotation = rotDeg * Math.PI / 180.0;

                    var type = typeStr == "D" ? TextType.DBText : TextType.MText;
                    text = new TextData(p, rotation, length, content, type);
                    return true;
                }
                catch
                {
                    error = ValidationHelper.FormatError(raw);
                    return false;
                }
            }

            // Không match
            error = ValidationHelper.FormatError(raw);
            return false;
        }
    }
}
