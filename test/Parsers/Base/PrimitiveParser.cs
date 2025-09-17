using System;
using System.Globalization;
using Autodesk.AutoCAD.Geometry;
using test.Utils;

namespace test.Parsers.Base
{
    /// <summary>
    /// Gom parser primitive: Number, Point, Text
    /// </summary>
    public static class PrimitiveParser
    {
        // ====== Number ======
        public static bool TryParsePositive(string s, out double value)
        {
            if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            {
                return value > 0;
            }
            return false;
        }

        public static bool TryParseDouble(string s, out double value)
        {
            return double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        // ====== Point ======
        /// <summary>
        /// Parse (x;y) → Point2d hoặc Point3d
        /// </summary>
        public static bool TryParsePoint(string s, out Point2d point2d, out Point3d point3d, bool is3D)
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

        // ====== Text ======
        /// <summary>
        /// Thử parse chuỗi text nằm trong dấu ngoặc kép "...".
        /// Hỗ trợ: /n = xuống dòng.
        /// Cho phép text rỗng.
        /// </summary>
        public static bool TryParseQuotedText(string raw, out string text, out string error)
        {
            text = null;
            error = null;

            if (string.IsNullOrWhiteSpace(raw))
            {
                error = ParseValidator.FormatError(raw);
                return false;
            }

            int firstQuote = raw.IndexOf('"');
            int lastQuote = raw.LastIndexOf('"');

            // Bắt buộc có cặp dấu "
            if (firstQuote < 0 || lastQuote <= firstQuote)
            {
                error = ParseValidator.FormatError(raw);
                return false;
            }

            // Sau dấu " cuối cùng chỉ được phép là whitespace
            for (int i = lastQuote + 1; i < raw.Length; i++)
            {
                if (!char.IsWhiteSpace(raw[i]))
                {
                    error = ParseValidator.FormatError(raw);
                    return false;
                }
            }

            // Lấy nội dung trong "" (có thể rỗng)
            string inner = raw.Substring(firstQuote + 1, lastQuote - firstQuote - 1);

            // Xử lý xuống dòng: /n -> newline
            text = inner.Replace("/n", Environment.NewLine);

            return true;
        }
    }
}

