using System.Collections.Generic;
using Newtonsoft.Json;

namespace test.Data
{
    /// <summary>Nhóm filter; key nào không dùng thì để trống.</summary>
    public class SelectionFilters
    {
        public List<string> EntityTypes { get; set; }
        public List<string> Layers { get; set; }
        public List<string> Colors { get; set; }
        public List<string> Blocks { get; set; }

        public SelectionFilters()
        {
            EntityTypes = new List<string>();
            Layers = new List<string>();
            Colors = new List<string>();
            Blocks = new List<string>();
        }
    }

    public class WindowRect
    {
        public double P1X { get; set; }
        public double P1Y { get; set; }
        public double P2X { get; set; }
        public double P2Y { get; set; }
    }

    /// <summary>Yêu cầu chọn đối tượng theo JSON.</summary>
    public class SelectionRequest
    {
        /// <summary>all | window | block</summary>
        public string Scope { get; set; }
        public WindowRect Window { get; set; }
        public string BlockName { get; set; }
        public SelectionFilters Filters { get; set; }

        public SelectionRequest()
        {
            Scope = "all";
            Filters = new SelectionFilters();
        }

        public static SelectionRequest FromJson(string json)
        {
            return JsonConvert.DeserializeObject<SelectionRequest>(json);
        }
    }
}


