using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Autodesk.AutoCAD.Geometry;
using test.Parsers.Base;

namespace test.Parsers
{
    public static class ArcParser
    {
        // regex dạng 3 điểm: (x;y),(x;y),(x;y)
        private static readonly Regex ThreePointPattern =
            new Regex(@"^\(\s*([0-9]+(?:\.[0-9]+)?)\s*;\s*([0-9]+(?:\.[0-9]+)?)\s*\)," +
                      @"\(\s*([0-9]+(?:\.[0-9]+)?)\s*;\s*([0-9]+(?:\.[0-9]+)?)\s*\)," +
                      @"\(\s*([0-9]+(?:\.[0-9]+)?)\s*;\s*([0-9]+(?:\.[0-9]+)?)\s*\)$");

        // regex dạng tâm + bán kính + góc
        private static readonly Regex CenterRadAnglePattern =
            new Regex(@"^\(\s*([0-9]+(?:\.[0-9]+)?)\s*;\s*([0-9]+(?:\.[0-9]+)?)\s*\)," +
                      @"([0-9]+(?:\.[0-9]+)?)," +
                      @"([0-9]+(?:\.[0-9]+)?)," +
                      @"([0-9]+(?:\.[0-9]+)?)$");

        public static ParseResult<ArcData> ParseAll(string input)
        {
            var result = new ParseResult<ArcData>();
            foreach (var raw in BaseParserHelper.SplitByDelimiter(input, '-'))
            {
                if (TryParseOne(raw, out ArcData arc, out string error))
                    result.AddValid(arc);
                else
                    result.AddError(error);
            }
            return result;
        }

        private static bool TryParseOne(string raw, out ArcData arc, out string error)
        {
            arc = null;
            error = null;

            // Dạng 3 điểm
            var m1 = ThreePointPattern.Match(raw);
            if (m1.Success)
            {
                try
                {
                    var p1 = new Point2d(
                        double.Parse(m1.Groups[1].Value, CultureInfo.InvariantCulture),
                        double.Parse(m1.Groups[2].Value, CultureInfo.InvariantCulture));
                    var p2 = new Point2d(
                        double.Parse(m1.Groups[3].Value, CultureInfo.InvariantCulture),
                        double.Parse(m1.Groups[4].Value, CultureInfo.InvariantCulture));
                    var p3 = new Point2d(
                        double.Parse(m1.Groups[5].Value, CultureInfo.InvariantCulture),
                        double.Parse(m1.Groups[6].Value, CultureInfo.InvariantCulture));

                    var circle = CircleFrom3Points(p1, p2, p3, out double startAng, out double endAng);
                    arc = new ArcData(new Point3d(circle.Center.X, circle.Center.Y, 0), circle.Radius, startAng, endAng);
                    return true;
                }
                catch
                {
                    error = ParseValidator.FormatError(raw);
                    return false;
                }
            }

            // Dạng tâm + bán kính + góc
            var m2 = CenterRadAnglePattern.Match(raw);
            if (m2.Success)
            {
                try
                {
                    double cx = double.Parse(m2.Groups[1].Value, CultureInfo.InvariantCulture);
                    double cy = double.Parse(m2.Groups[2].Value, CultureInfo.InvariantCulture);
                    double r = double.Parse(m2.Groups[3].Value, CultureInfo.InvariantCulture);
                    double startAng = double.Parse(m2.Groups[4].Value, CultureInfo.InvariantCulture);
                    double endAng = double.Parse(m2.Groups[5].Value, CultureInfo.InvariantCulture);

                    if (!ParseValidator.CheckPositive(r))
                    {
                        error = ParseValidator.ValueError(raw, m2.Groups[3].Value);
                        return false;
                    }
                    if (!ParseValidator.CheckAngle(startAng) || !ParseValidator.CheckAngle(endAng))
                    {
                        error = ParseValidator.ValueError(raw, $"{startAng},{endAng}");
                        return false;
                    }

                    arc = new ArcData(new Point3d(cx, cy, 0), r,
                        startAng * Math.PI / 180.0,
                        endAng * Math.PI / 180.0);
                    return true;
                }
                catch
                {
                    error = ParseValidator.FormatError(raw);
                    return false;
                }
            }

            error = ParseValidator.FormatError(raw);
            return false;
        }

        /// <summary>
        /// Tính đường tròn từ 3 điểm và góc start/end
        /// </summary>
        private static (Point2d Center, double Radius) CircleFrom3Points(Point2d p1, Point2d p2, Point2d p3,
            out double startAng, out double endAng)
        {
            // Công thức giao của đường trung trực
            double ma = (p2.Y - p1.Y) / (p2.X - p1.X);
            double mb = (p3.Y - p2.Y) / (p3.X - p2.X);

            double cx = (ma * mb * (p1.Y - p3.Y) + mb * (p1.X + p2.X) - ma * (p2.X + p3.X)) /
                        (2 * (mb - ma));
            double cy = -1 / ma * (cx - (p1.X + p2.X) / 2) + (p1.Y + p2.Y) / 2;

            var center = new Point2d(cx, cy);
            double r = center.GetDistanceTo(p1);

            startAng = Math.Atan2(p1.Y - cy, p1.X - cx);
            endAng = Math.Atan2(p3.Y - cy, p3.X - cx);

            return (center, r);
        }
    }
}
