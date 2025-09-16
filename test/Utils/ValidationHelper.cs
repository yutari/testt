using System;

namespace test.Utils
{
    public static class ValidationHelper
    {
        // ====== Check logic ======
        public static bool CheckAngle(double angle) => angle >= 0 && angle < 360;

        public static bool CheckMinVertices(int count, int min = 2) => count >= min;

        // ====== Chuẩn hóa báo lỗi ======
        /// <summary>
        /// Báo lỗi định dạng: gạch cả chuỗi entity.
        /// </summary>
        public static string FormatError(string raw)
        {
            return $"_{raw}_";
        }

        /// <summary>
        /// Báo lỗi giá trị: chỉ gạch giá trị sai trong entity.
        /// </summary>
        public static string ValueError(string raw, string badValue)
        {
            if (string.IsNullOrWhiteSpace(badValue))
                return FormatError(raw);

            int index = raw.IndexOf(badValue, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
                return FormatError(raw);

            return raw.Substring(0, index)
                 + "_" + badValue + "_"
                 + raw.Substring(index + badValue.Length);
        }
    }
}

