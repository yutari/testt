using System;
using test.Utils;

namespace test.Parsers.Base
{
    public static class TextParser
    {
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
                error = ValidationHelper.FormatError(raw);
                return false;
            }

            int firstQuote = raw.IndexOf('"');
            int lastQuote = raw.LastIndexOf('"');

            // Bắt buộc có cặp dấu "
            if (firstQuote < 0 || lastQuote <= firstQuote)
            {
                error = ValidationHelper.FormatError(raw);
                return false;
            }

            // Sau dấu " cuối cùng chỉ được phép là whitespace
            for (int i = lastQuote + 1; i < raw.Length; i++)
            {
                if (!char.IsWhiteSpace(raw[i]))
                {
                    error = ValidationHelper.FormatError(raw);
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
