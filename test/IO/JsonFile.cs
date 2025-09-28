using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace test.IO
{
    public static class JsonFile
    {
        public static T Read<T>(string path)
        {
            using (var reader = new StreamReader(path, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
            {
                var json = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<T>(json);
            }
        }

        public static void Write<T>(string path, T data)
        {
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            using (var writer = new StreamWriter(path, false, Encoding.UTF8))
            {
                writer.Write(json);
            }
        }
    }
}

