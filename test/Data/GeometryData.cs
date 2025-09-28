using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using test.Data;

namespace test.Parsers.Base
{
    // ==========================
    // Line
    // ==========================
    public sealed class LineData
    {
        public PointSchema Start { get; set; }
        public PointSchema End { get; set; }

        public string Layer { get; set; }
        public string Color { get; set; }
        public string Linetype { get; set; }
        public LineWeight? Lineweight { get; set; }
        public XDataDto XData { get; set; }
    }

    // ==========================
    // Circle
    // ==========================
    public sealed class CircleData
    {
        public PointSchema Center { get; set; }
        public double Radius { get; set; }

        public string Layer { get; set; }
        public string Color { get; set; }
        public string Linetype { get; set; }
        public LineWeight? Lineweight { get; set; }
        public XDataDto XData { get; set; }
    }

    // ==========================
    // Polyline
    // ==========================
    public sealed class PolylineVertex
    {
        public PointSchema Point { get; set; }
        public double Bulge { get; set; }
    }

    public sealed class PolylineData
    {
        public List<PolylineVertex> Vertices { get; set; }
        public bool IsClosed { get; set; }

        public string Layer { get; set; }
        public string Color { get; set; }
        public string Linetype { get; set; }
        public LineWeight? Lineweight { get; set; }
        public XDataDto XData { get; set; }

        public PolylineData()
        {
            Vertices = new List<PolylineVertex>();
        }
    }

    // ==========================
    // Arc
    // ==========================
    public sealed class ArcData
    {
        public PointSchema Center { get; set; }
        public double Radius { get; set; }
        public double StartAngle { get; set; }
        public double EndAngle { get; set; }

        public string Layer { get; set; }
        public string Color { get; set; }
        public string Linetype { get; set; }
        public LineWeight? Lineweight { get; set; }
        public XDataDto XData { get; set; }
    }

    // ==========================
    // Leader
    // ==========================
    public sealed class LeaderData
    {
        public List<PointSchema> Points { get; set; }
        public string Text { get; set; }

        public string Layer { get; set; }
        public string Color { get; set; }
        public string Linetype { get; set; }
        public LineWeight? Lineweight { get; set; }
        public XDataDto XData { get; set; }

        public LeaderData()
        {
            Points = new List<PointSchema>();
        }
    }

    // ==========================
    // Text
    // ==========================
    public enum TextType
    {
        DBText,
        MText
    }

    public sealed class TextData
    {
        public PointSchema Position { get; set; }
        public double Rotation { get; set; }
        public double Width { get; set; }
        public string Content { get; set; }
        public TextType Type { get; set; }

        public double Height { get; set; } = 2.5;
        public string StyleName { get; set; }
        public double LineSpacingFactor { get; set; } = 1.0;
        public LineSpacingStyle LineSpacingStyle { get; set; } = LineSpacingStyle.AtLeast;

        public string Layer { get; set; }
        public string Color { get; set; }
        public string Linetype { get; set; }
        public LineWeight? Lineweight { get; set; }
        public XDataDto XData { get; set; }
    }
}
