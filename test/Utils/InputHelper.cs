using System.Collections.Generic;
using System.Text;

namespace test.Utils
{
    public static class InputHelper
    {
        // Ghép danh sách line lỗi thành block báo lỗi
        public static string JoinInvalidAsBlock(IEnumerable<string> invalids)
        {
            if (invalids == null) return "Không có line lỗi.";

            var sb = new StringBuilder();
            sb.Append("Những line sau không hợp lệ:");
            foreach (var s in invalids)
            {
                sb.Append(" - ");
                sb.Append(s);
            }
            return sb.ToString();
        }
    }
}

