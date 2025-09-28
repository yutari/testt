using System.Collections.Generic;
using Newtonsoft.Json;
using test.Parsers.Base; // LineData, CircleData, ArcData, PolylineData, LeaderData, TextData

namespace test.Data
{
    public class SchemaEntityUpdate
    {
        public string Handle { get; set; }
        public SchemaEntity Data { get; set; }
    }
    /// <summary>
    /// Batch cho chức năng vẽ: gom tất cả entity cần vẽ.
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
            return JsonConvert.DeserializeObject<DrawingBatch>(json);
        }
    }

    /// <summary>
    /// Batch cho chức năng chọn: gom nhiều request chọn trong 1 lần.
    /// </summary>
    /// <summary>
    /// Batch cho chức năng chọn: gom nhiều SelectionRequest trong 1 lần.
    /// </summary>
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

    public class SelectionItem
    {
        public string Scope { get; set; }   // all | window | block
        public WindowRect Window { get; set; }
        public string BlockName { get; set; }
        public SelectionFilters Filters { get; set; }

        public SelectionItem()
        {
            Scope = "all";
            Filters = new SelectionFilters();
        }
    }

    public class SelectionBatch
    {
        public List<SelectionItem> Requests { get; set; }

        public SelectionBatch()
        {
            Requests = new List<SelectionItem>();
        }

        public static SelectionBatch FromJson(string json)
        {
            return JsonConvert.DeserializeObject<SelectionBatch>(json);
        }

        public static SelectionBatch FromItems(IEnumerable<SelectionItem> items)
        {
            return new SelectionBatch { Requests = new List<SelectionItem>(items) };
        }
    }

    /// <summary>
    /// Batch cho chức năng update: gom nhiều request update trong 1 lần.
    /// </summary>
    public class UpdateBatch
    {
        public List<SchemaEntityUpdate> Requests { get; set; }

        public UpdateBatch()
        {
            Requests = new List<SchemaEntityUpdate>();
        }

        public static UpdateBatch FromJson(string json)
        {
            return JsonConvert.DeserializeObject<UpdateBatch>(json);
        }

        public static UpdateBatch FromItems(IEnumerable<SchemaEntityUpdate> items)
        {
            return new UpdateBatch { Requests = new List<SchemaEntityUpdate>(items) };
        }
    }

    // ==========================
    // Block batch
    // ==========================
    public class BlockInsertItem
    {
        public string FilePath { get; set; }      // đường dẫn file DWG ngoài
        public string BlockName { get; set; }     // tên block trong file
        public PointSchema Position { get; set; } // vị trí chèn
        public double Scale { get; set; }
        public double Rotation { get; set; }
        public Dictionary<string, object> DynamicProps { get; set; }

        public BlockInsertItem()
        {
            Position = new PointSchema();
            Scale = 1.0;
            Rotation = 0.0;
            DynamicProps = new Dictionary<string, object>();
        }
    }

    public class BlockBatch
    {
        public List<BlockInsertItem> Blocks { get; set; }

        public BlockBatch()
        {
            Blocks = new List<BlockInsertItem>();
        }

        public static BlockBatch FromJson(string json)
        {
            return JsonConvert.DeserializeObject<BlockBatch>(json);
        }
    }
}




