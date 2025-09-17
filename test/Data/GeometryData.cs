using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;

namespace test.Parsers.Base
{
    public sealed class LineData
    {
        public Point3d Start { get; }
        public Point3d End { get; }

        public LineData(Point3d start, Point3d end)
        {
            Start = start;
            End = end;
        }

    }
    public sealed class CircleData
    {
        public Point3d Center { get; }
        public double Radius { get; }

        public CircleData(Point3d center, double radius)
        {
            Center = center;
            Radius = radius;
        }
    }
    public sealed class PolylineVertex
    {
        public Point2d Point { get; }
        public double Bulge { get; }  // 0 = đoạn thẳng, !=0 = cung (tan(theta/4))

        public PolylineVertex(Point2d point, double bulge = 0)
        {
            Point = point;
            Bulge = bulge;
        }
    }

    public sealed class PolylineData
    {
        public List<PolylineVertex> Vertices { get; }
        public bool IsClosed { get; }

        public PolylineData(List<PolylineVertex> vertices, bool isClosed)
        {
            Vertices = vertices;
            IsClosed = isClosed;
        }
    }
    public sealed class ArcData
    {
        public Point3d Center { get; }
        public double Radius { get; }
        public double StartAngle { get; }
        public double EndAngle { get; }

        public ArcData(Point3d center, double radius, double startAngle, double endAngle)
        {
            Center = center;
            Radius = radius;
            StartAngle = startAngle;
            EndAngle = endAngle;
        }
    }
    public sealed class LeaderData
    {
        public List<Point3d> Points { get; }
        public string Text { get; }

        public LeaderData(List<Point3d> points, string text)
        {
            Points = points;
            Text = text;
        }
    }
    public enum TextType
    {
        DBText,
        MText
    }

    public sealed class TextData
    {
        public Point3d Position { get; }
        public double Rotation { get; }   // radians
        public double Width { get; }      // chiều dài khung (0 nếu không có)
        public string Content { get; }
        public TextType Type { get; }


        public TextData(Point3d position, double rotation, double width, string content, TextType type)
        {
            Position = position;
            Rotation = rotation;
            Width = width;
            Content = content;
            Type = type;
        }
    }
}
