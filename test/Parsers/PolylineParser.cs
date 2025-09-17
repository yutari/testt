using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Autodesk.AutoCAD.Geometry;
using test.Parsers.Base;

namespace test.Parsers
{
    public static class PolylineParser
    {
        // Regex cho vertex: (x;y) hoặc (x;y),A(bulge)
        private static readonly Regex VertexPattern =
            new Regex(@"^\(\s*([0-9]+(?:\.[0-9]+)?)\s*;\s*([0-9]+(?:\.[0-9]+)?)\s*\)(?:\s*,\s*A\(\s*([0-9]+(?:\.[0-9]+)?)\s*\))?$");

        public static ParseResult<PolylineData> ParseAll(string input)
        {
            var result = new ParseResult<PolylineData>();
            foreach (var raw in BaseParserHelper.SplitByDelimiter(input, '-'))
            {
                if (TryParseOne(raw, out PolylineData data, out string error))
                    result.AddValid(data);
                else
                    result.AddError(error);
            }
            return result;
        }

        private static bool TryParseOne(string raw, out PolylineData polyline, out string error)
        {
            polyline = null;
            error = null;

            try
            {
                bool closed = raw.EndsWith(",C", StringComparison.OrdinalIgnoreCase);
                if (closed) raw = raw.Substring(0, raw.Length - 2);

                var vertices = new List<PolylineVertex>();
                var tokens = raw.Split(new[] { ')' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var token in tokens)
                {
                    var t = token.Trim();
                    if (string.IsNullOrEmpty(t)) continue;
                    if (!t.EndsWith(")")) t += ")";

                    var m = VertexPattern.Match(t);
                    if (!m.Success)
                    {
                        error = ParseValidator.FormatError(raw);
                        return false;
                    }

                    double x = double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
                    double y = double.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
                    double bulge = 0;

                    if (m.Groups[3].Success)
                    {
                        if (!PrimitiveParser.TryParseDouble(m.Groups[3].Value, out bulge))
                        {
                            error = ParseValidator.ValueError(raw, m.Groups[3].Value);
                            return false;
                        }
                    }

                    vertices.Add(new PolylineVertex(new Point2d(x, y), bulge));
                }

                if (!ParseValidator.CheckMinVertices(vertices.Count, 2))
                {
                    error = ParseValidator.FormatError(raw);
                    return false;
                }

                polyline = new PolylineData(vertices, closed);
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
