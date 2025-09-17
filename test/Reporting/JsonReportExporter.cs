using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using test.Data;

namespace test.Reporting
{
    public static class JsonReportExporter
    {
        public static void Export(List<SelectedEntityData> entities, string filePath)
        {
            var json = JsonConvert.SerializeObject(entities, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
    }
}
