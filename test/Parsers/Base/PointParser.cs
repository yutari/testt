using System;
using System.Globalization;
using Autodesk.AutoCAD.Geometry;

namespace test.Parsers.Base
{
    public static class PointParser
    {
        /// <summary>
        /// Parse (x;y) → Point2d hoặc Point3d
        /// </summary>
        /// <param name="s">chuỗi input dạng (x;y)</param>
        /// <param name="point2d">output Point2d (nếu mode = 2D)</param>
        /// <param name="point3d">output Point3d (nếu mode = 3D)</param>
        /// <param name="is3D">true = trả về Point3d, false = Point2d</param>
        public static bool TryParse(string s, out Point2d point2d, out Point3d point3d, bool is3D)
        {
            point2d = default;
            point3d = default;

            try
            {
                s = s.Trim();
                if (!s.StartsWith("(") || !s.EndsWith(")"))
                    return false;

                var inner = s.Trim('(', ')');
                var xy = inner.Split(';');
                if (xy.Length != 2) return false;

                if (double.TryParse(xy[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x) &&
                    double.TryParse(xy[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
                {
                    if (is3D)
                    {
                        point3d = new Point3d(x, y, 0);
                        return true;
                    }
                    else
                    {
                        point2d = new Point2d(x, y);
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}

