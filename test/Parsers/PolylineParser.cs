using System;
using System.Collections.Generic;
using System.Globalization;
using Autodesk.AutoCAD.Geometry;
using test.Parsers.Base;
using test.Utils;

namespace test.Parsers
{
    public static class PolylineParser
    {
        // Input: -(x;y),(x;y),A(bulge),(x;y),C-
        public static ParseResult<PolylineData> ParseAll(string input)
        {
            var result = new ParseResult<PolylineData>();
            foreach (var raw in BaseParserHelper.SplitByDelimiter(input, '-'))
            {
                if (TryParseOne(raw, out PolylineData poly, out string error))
                    result.AddValid(poly);
                else
                    result.AddError(error);
            }
            return result;
        }

        private static bool TryParseOne(string raw, out PolylineData poly, out string error)
        {
            poly = null;
            error = null;

            try
            {
                var tokens = raw.Split(',');
                var vertices = new List<PolylineVertex>();
                bool isClosed = false;

                foreach (var tokenRaw in tokens)
                {
                    var token = tokenRaw.Trim();

                    if (string.Equals(token, "C", StringComparison.OrdinalIgnoreCase))
                    {
                        isClosed = true;
                        continue;
                    }

                    if (token.StartsWith("A(", StringComparison.OrdinalIgnoreCase) && token.EndsWith(")"))
                    {
                        var inner = token.Substring(2, token.Length - 3);
                        if (!double.TryParse(inner, NumberStyles.Float, CultureInfo.InvariantCulture, out var bulge))
                        {
                            error = ValidationHelper.ValueError(raw, token);
                            return false;
                        }

                        if (vertices.Count == 0)
                        {
                            error = ValidationHelper.FormatError(raw);
                            return false;
                        }

                        var last = vertices[vertices.Count - 1];
                        vertices[vertices.Count - 1] = new PolylineVertex(last.Point, bulge);
                        continue;
                    }

                    if (token.StartsWith("(") && token.EndsWith(")"))
                    {
                        var inner = token.Trim('(', ')');
                        var xy = inner.Split(';');
                        if (xy.Length != 2)
                        {
                            error = ValidationHelper.FormatError(raw);
                            return false;
                        }

                        if (!double.TryParse(xy[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x) ||
                            !double.TryParse(xy[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
                        {
                            error = ValidationHelper.ValueError(raw, token);
                            return false;
                        }

                        vertices.Add(new PolylineVertex(new Point2d(x, y)));
                        continue;
                    }

                    error = ValidationHelper.FormatError(raw);
                    return false;
                }

                if (!ValidationHelper.CheckMinVertices(vertices.Count, 2))
                {
                    error = ValidationHelper.FormatError(raw);
                    return false;
                }

                poly = new PolylineData(vertices, isClosed);
                return true;
            }
            catch
            {
                error = ValidationHelper.FormatError(raw);
                return false;
            }
        }
    }
}

