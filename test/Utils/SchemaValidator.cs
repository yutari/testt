using test.Data;
using test.Parsers.Base;

namespace test.Utils
{
    /// <summary>
    /// Validate dữ liệu JSON theo Schema.
    /// Nếu thiếu field → điền default.
    /// Nếu có field sai → bỏ qua (hoặc có thể báo lỗi tuỳ chính sách).
    /// </summary>
    public static class SchemaValidator
    {
        public static void ValidateAndFill(SchemaEntity schema)
        {
            if (schema == null) return;

            // -------- Common fields --------
            if (string.IsNullOrWhiteSpace(schema.Layer))
                schema.Layer = "0"; // default layer

            if (string.IsNullOrWhiteSpace(schema.Color))
                schema.Color = null; // ByLayer

            if (string.IsNullOrWhiteSpace(schema.Linetype))
                schema.Linetype = "ByLayer";

            if (string.IsNullOrWhiteSpace(schema.Lineweight))
                schema.Lineweight = "ByLayer";

            // -------- Geometry --------
            if (schema.Geometry == null)
                schema.Geometry = new GeometrySchema(); // để trống, không thay đổi gì

            // -------- Text specific --------
            if (schema.Type == "DBText" || schema.Type == "MText")
            {
                if (schema.Geometry.Height == null)
                    schema.Geometry.Height = 2.5; // default text height

                if (string.IsNullOrWhiteSpace(schema.Geometry.StyleName))
                    schema.Geometry.StyleName = "Standard";

                if (schema.Geometry.LineSpacingFactor == null)
                    schema.Geometry.LineSpacingFactor = 1.0;

                if (schema.Geometry.LineSpacingStyle == null)
                    schema.Geometry.LineSpacingStyle = "AtLeast";
            }

            // -------- XData --------
            if (schema.XData == null)
                schema.XData = new XDataDto(); // empty
        }
    }
}
