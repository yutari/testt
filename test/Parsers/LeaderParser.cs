using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using test.Parsers.Base;
using test.Utils;

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
            var parts = BaseParserHelper.SplitByDelimiter(input, '-');

            Parallel.ForEach(parts, raw =>
            {
                if (TryParseOne(raw, out LeaderData data, out string error))
                    result.AddValid(data);
                else
                    result.AddError(error);
            });

            return result;
        }

        private static bool TryParseOne(string raw, out LeaderData leader, out string error)
        {
            leader = null;

            if (string.IsNullOrWhiteSpace(raw))
            {
                error = ValidationHelper.FormatError(raw);
                return false;
            }

            var s = raw.Trim();

            // Parse text
            if (!TextParser.TryParseQuotedText(s, out string text, out error))
                return false;

            // Phần trước dấu " đầu tiên là danh sách điểm
            var firstQuote = s.IndexOf('"');
            var head = s.Substring(0, firstQuote).Trim();
            if (head.EndsWith(",")) head = head.Substring(0, head.Length - 1);

            var tokens = head.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length < 2 || tokens.Length > 3)
            {
                error = ValidationHelper.FormatError(raw);
                return false;
            }

            var pts = new List<Point3d>();
            foreach (var t in tokens)
            {
                if (!PointParser.TryParse(t.Trim(), out _, out var p, true))
                {
                    error = ValidationHelper.ValueError(raw, t);
                    return false;
                }
                pts.Add(p);
            }

            leader = new LeaderData(pts, text);
            return true;
        }
    }
}
