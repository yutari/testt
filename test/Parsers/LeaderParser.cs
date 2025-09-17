using System.Collections.Generic;
using Autodesk.AutoCAD.Geometry;
using test.Parsers.Base;

namespace test.Parsers
{
    public static class LeaderParser
    {
        // Input hợp lệ:
        // - (x1;y1),(x2;y2),"text"
        // - (x1;y1),(x2;y2),(x3;y3),"text"

        public static ParseResult<LeaderData> ParseAll(string input)
        {
            var result = new ParseResult<LeaderData>();
            foreach (var raw in BaseParserHelper.SplitByDelimiter(input, '-'))
            {
                if (TryParseOne(raw, out LeaderData leader, out string error))
                    result.AddValid(leader);
                else
                    result.AddError(error);
            }
            return result;
        }

        private static bool TryParseOne(string raw, out LeaderData leader, out string error)
        {
            leader = null;
            error = null;

            var parts = raw.Split(',');
            if (parts.Length < 3)
            {
                error = ParseValidator.FormatError(raw);
                return false;
            }

            var pts = new List<Point3d>();

            // parse point1
            if (!PrimitiveParser.TryParsePoint(parts[0], out var p2d, out var p3d, true))
            {
                error = ParseValidator.FormatError(parts[0]);
                return false;
            }
            pts.Add(p3d);

            // parse point2
            if (!PrimitiveParser.TryParsePoint(parts[1], out p2d, out p3d, true))
            {
                error = ParseValidator.FormatError(parts[1]);
                return false;
            }
            pts.Add(p3d);

            int textIndex = 2;

            // có thể có 3 điểm
            if (parts.Length >= 4)
            {
                if (!PrimitiveParser.TryParsePoint(parts[2], out p2d, out p3d, true))
                {
                    error = ParseValidator.FormatError(parts[2]);
                    return false;
                }
                pts.Add(p3d);
                textIndex = 3;
            }

            // parse text
            if (!PrimitiveParser.TryParseQuotedText(parts[textIndex], out string text, out string textError))
            {
                error = textError ?? ParseValidator.FormatError(parts[textIndex]);
                return false;
            }

            leader = new LeaderData(pts, text);
            return true;
        }
    }
}
