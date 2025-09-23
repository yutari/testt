using System.Collections.Generic;
using test.Parsers.Base; // LineData, CircleData, ArcData, PolylineData, LeaderData, TextData

namespace test.Data
{
    /// <summary>
    /// Object trung gian “A” cho chức năng vẽ: gom tất cả entity cần vẽ.
    /// Mọi nguồn input (User/JSON/DB) đều build ra object này rồi đưa vào logic.
    /// </summary>
    public class DrawingBatch
    {
        public List<LineData> Lines { get; set; }
        public List<CircleData> Circles { get; set; }
        public List<ArcData> Arcs { get; set; }
        public List<PolylineData> Polylines { get; set; }
        public List<LeaderData> Leaders { get; set; }
        public List<TextData> Texts { get; set; }

        public DrawingBatch()
        {
            Lines = new List<LineData>();
            Circles = new List<CircleData>();
            Arcs = new List<ArcData>();
            Polylines = new List<PolylineData>();
            Leaders = new List<LeaderData>();
            Texts = new List<TextData>();
        }
        public static DrawingBatch FromJson(string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DrawingBatch>(json);
        }
    }
}


