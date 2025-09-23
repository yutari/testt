using Autodesk.AutoCAD.EditorInput;
using Newtonsoft.Json;

namespace test.IO
{
    /// <summary>In “truyền gì in nấy” ra command line AutoCAD (mặc định JSON đẹp).</summary>
    public static class EditorPrinter
    {
        public static void Print(Editor ed, object data, string header)
        {
            if (ed == null) return;

            if (!string.IsNullOrWhiteSpace(header))
                ed.WriteMessage("\n" + header);

            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Include
            };

            var json = JsonConvert.SerializeObject(data, settings);
            WriteMultiline(ed, json);
        }

        public static void Print(Editor ed, object data)
        {
            Print(ed, data, null);
        }

        private static void WriteMultiline(Editor ed, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                ed.WriteMessage("\n(null)");
                return;
            }
            var lines = text.Replace("\r\n", "\n").Split('\n');
            for (int i = 0; i < lines.Length; i++)
                ed.WriteMessage("\n" + lines[i]);
        }
    }
}
