using System.IO;
using Newtonsoft.Json;

namespace test.IO
{
    public static class JsonFile
    {
        public static T Read<T>(string path)
        {
            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static void Write<T>(string path, T data)
        {
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(path, json);
        }
    }
}

