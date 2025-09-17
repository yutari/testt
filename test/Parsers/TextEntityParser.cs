using System;
using Autodesk.AutoCAD.Geometry;
using test.Parsers.Base;

namespace test.Parsers
{
    public static class TextEntityParser
    {
        // Input hợp lệ:
        // - (x1;y1),(x2;y2),"text"D|M
        // - (x;y),rot,len,"text"D|M

        public static ParseResult<TextData> ParseAll(string input)
        {
            var result = new ParseResult<TextData>();
            foreach (var raw in BaseParserHelper.SplitByDelimiter(input, '-'))
            {
                if (TryParseOne(raw, out TextData data, out string error))
                    result.AddValid(data);
                else
                    result.AddError(error);
            }
            return result;
        }

        private static bool TryParseOne(string raw, out TextData textEntity, out string error)
        {
            textEntity = null;
            error = null;

            // mode = D (DBText) hoặc M (MText)
            char mode = char.ToUpper(raw[raw.Length - 1]);
            if (mode != 'D' && mode != 'M')
            {
                error = ParseValidator.FormatError(raw);
                return false;
            }
            string core = raw.Substring(0, raw.Length - 1);

            var parts = core.Split(',');
            if (parts.Length < 3)
            {
                error = ParseValidator.FormatError(raw);
                return false;
            }

            var textType = (mode == 'M' ? TextType.MText : TextType.DBText);

            // ===== Trường hợp 1: 2 điểm + text =====
            if (parts.Length == 3)
            {
                if (!PrimitiveParser.TryParsePoint(parts[0], out var _, out var p1, true) ||
                    !PrimitiveParser.TryParsePoint(parts[1], out var _, out var p2, true))
                {
                    error = ParseValidator.FormatError(raw);
                    return false;
                }

                if (!PrimitiveParser.TryParseQuotedText(parts[2], out string text, out string textError))
                {
                    error = textError ?? ParseValidator.FormatError(parts[2]);
                    return false;
                }

                double dx = p2.X - p1.X;
                double dy = p2.Y - p1.Y;
                double rot = Math.Atan2(dy, dx);              // radians
                double width = p1.DistanceTo(p2);             // độ dài đoạn

                textEntity = new TextData(p1, rot, width, text, textType);
                return true;
            }

            // ===== Trường hợp 2: 1 điểm + rot + len + text =====
            if (parts.Length == 4)
            {
                if (!PrimitiveParser.TryParsePoint(parts[0], out var _, out var p, true))
                {
                    error = ParseValidator.FormatError(parts[0]);
                    return false;
                }

                if (!PrimitiveParser.TryParseDouble(parts[1], out double rotDeg) ||
                    !PrimitiveParser.TryParseDouble(parts[2], out double len))
                {
                    error = ParseValidator.FormatError(raw);
                    return false;
                }

                if (!ParseValidator.CheckAngle(rotDeg))
                {
                    error = ParseValidator.ValueError(raw, parts[1]);
                    return false;
                }
                if (!ParseValidator.CheckPositive(len))
                {
                    error = ParseValidator.ValueError(raw, parts[2]);
                    return false;
                }

                if (!PrimitiveParser.TryParseQuotedText(parts[3], out string text, out string textError))
                {
                    error = textError ?? ParseValidator.FormatError(parts[3]);
                    return false;
                }

                double rotRad = rotDeg * Math.PI / 180.0;    // đổi sang radians

                textEntity = new TextData(p, rotRad, len, text, textType);
                return true;
            }

            error = ParseValidator.FormatError(raw);
            return false;
        }
    }
}
