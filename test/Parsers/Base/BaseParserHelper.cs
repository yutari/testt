using System;
using System.Collections.Generic;

namespace test.Parsers.Base
{
    /// <summary>
    /// Helper dùng chung cho mọi parser (Line, Circle, Polyline, Arc, Text, Leader...)
    /// </summary>
    public static class BaseParserHelper
    {
        /// <summary>
        /// Cắt chuỗi input thành các block theo delimiter (thường là '-').
        /// Tự động Trim và bỏ empty entries.
        /// </summary>
        public static IEnumerable<string> SplitByDelimiter(string input, char delimiter = '-')
        {
            if (string.IsNullOrWhiteSpace(input)) yield break;

            var parts = input.Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var raw in parts)
            {
                var trimmed = raw.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                    yield return trimmed;
            }
        }
    }
}
