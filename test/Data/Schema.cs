using System.Collections.Generic;

namespace test.Data
{
    /// <summary>
    /// Entity chuẩn hóa để import/export.
    /// Đây là cấu trúc JSON chính thức của hệ thống.
    /// </summary>
    public class SchemaEntity
    {
        public string Handle { get; set; }      // để update
        public string Type { get; set; }        // Line, Circle, Arc, Polyline, DBText, MText, Leader...

        // Thuộc tính chung
        public string Layer { get; set; }
        public string Color { get; set; }       // "rgb(r,g,b)" hoặc null = ByLayer
        public string Linetype { get; set; }
        public string Lineweight { get; set; }

        // Hình học (tùy loại entity)
        public GeometrySchema Geometry { get; set; }

        // XData kèm theo
        public XDataDto XData { get; set; }
    }

    /// <summary>
    /// Geometry tùy loại entity (Line, Circle, Arc, Polyline, Text, Leader).
    /// </summary>
    public class GeometrySchema
    {
        // -------- Line --------
        public PointSchema Start { get; set; }
        public PointSchema End { get; set; }

        // -------- Circle & Arc --------
        public PointSchema Center { get; set; }
        public double? Radius { get; set; }
        public double? StartAngle { get; set; }
        public double? EndAngle { get; set; }

        // -------- Polyline --------
        public List<PolylineVertexSchema> Vertices { get; set; }
        public bool? Closed { get; set; }

        // -------- Text --------
        public PointSchema Position { get; set; }    // DBText
        public PointSchema Location { get; set; }    // MText
        public string TextString { get; set; }
        public string Contents { get; set; }
        public double? Height { get; set; }
        public double? Rotation { get; set; }
        public double? Width { get; set; }
        public string StyleName { get; set; }
        public double? LineSpacingFactor { get; set; }
        public string LineSpacingStyle { get; set; }

        // -------- Leader --------
        public List<PointSchema> Points { get; set; }

        public GeometrySchema()
        {
            Vertices = new List<PolylineVertexSchema>();
            Points = new List<PointSchema>();
        }
    }

    /// <summary>
    /// Điểm 2D/3D trong JSON.
    /// </summary>
    public class PointSchema
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; } = 0;
    }

    /// <summary>
    /// Vertex của polyline.
    /// </summary>
    public class PolylineVertexSchema
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Bulge { get; set; } = 0;
    }

    /// <summary>
    /// XData kèm theo entity.
    /// </summary>
    public class XDataDto
    {
        public string AppName { get; set; }
        public List<XDataValue> Values { get; set; } = new List<XDataValue>();
    }

    public class XDataValue
    {
        public string Type { get; set; }   // string, int, double...
        public object Value { get; set; }
    }
}
