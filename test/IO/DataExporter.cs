using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace test.IO
{
    public enum ExportFormat { Json /* tương lai: Csv, Xml... */ }

    /// <summary>Xuất dữ liệu ra file theo định dạng (hiện JSON).</summary>
    public static class DataExporter
    {
        public static void Export<T>(T data, string filePath, ExportFormat format)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("filePath is required.", "filePath");

            string dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string content = ToText(data, format);
            File.WriteAllText(filePath, content, Encoding.UTF8);
        }

        public static string ToText<T>(T data, ExportFormat format)
        {
            switch (format)
            {
                case ExportFormat.Json:
                default:
                    var settings = new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        NullValueHandling = NullValueHandling.Include
                    };
                    return JsonConvert.SerializeObject(data, settings);
            }
        }
    }
}

