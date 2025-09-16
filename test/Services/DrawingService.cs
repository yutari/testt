using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using test.Parsers.Base;

namespace test.Services
{
    public static class DrawingService
    {
        /// <summary>
        /// Generic draw: nhận data + factory tạo entity
        /// </summary>
        public static void Draw<T>(IEnumerable<T> datas, Database db, Func<T, IEnumerable<Entity>> factory)
        {
            if (datas == null) return;

            var entities = new List<Entity>();
            foreach (var d in datas)
            {
                var created = factory(d);
                if (created != null)
                    entities.AddRange(created);
            }

            DrawingHelper.AddEntities(db, entities);
        }

        public static void DrawLines(IEnumerable<LineData> lines, Database db) =>
            Draw(lines, db, d => new[] { new Line(d.Start, d.End) });

        public static void DrawCircles(IEnumerable<CircleData> circles, Database db) =>
            Draw(circles, db, d => new[] { new Circle(d.Center, Vector3d.ZAxis, d.Radius) });

        public static void DrawPolylines(IEnumerable<PolylineData> polylines, Database db) =>
            Draw(polylines, db, d =>
            {
                var pl = new Polyline();
                for (int i = 0; i < d.Vertices.Count; i++)
                {
                    var v = d.Vertices[i];
                    pl.AddVertexAt(i, v.Point, v.Bulge, 0, 0);
                }
                pl.Closed = d.IsClosed;
                return new[] { pl };
            });

        public static void DrawArcs(IEnumerable<ArcData> arcs, Database db) =>
            Draw(arcs, db, d => new[] { new Arc(d.Center, d.Radius, d.StartAngle, d.EndAngle) });

        public static void DrawLeaders(IEnumerable<LeaderData> leaders, Database db) =>
            Draw(leaders, db, d =>
            {
                if (d.Points.Count < 2) return null;

                var leader = new Leader { HasArrowHead = true };
                foreach (var pt in d.Points)
                    leader.AppendVertex(pt);

                // Nếu text có nội dung thì mới tạo MText
                if (!string.IsNullOrEmpty(d.Text))
                {
                    var lastPt = d.Points[d.Points.Count - 1];
                    var mtext = new MText
                    {
                        Location = lastPt,
                        Contents = d.Text,
                        TextHeight = 2.5
                    };
                    return new Entity[] { leader, mtext };
                }

                return new Entity[] { leader };
            }
            );
        public static void DrawTexts(IEnumerable<TextData> texts, Database db) =>
            Draw(texts, db, d =>
            {
                if (d.Type == TextType.DBText)
                {
                    var dbt = new DBText
                    {
                        Position = d.Position,
                        Rotation = d.Rotation,
                        TextString = d.Content,
                        Height = 2.5 // mặc định, có thể config
                    };
                    return new Entity[] { dbt };
                }
                else
                {
                    var mt = new MText
                    {
                        Location = d.Position,
                        Contents = d.Content,
                        TextHeight = 2.5,
                        Rotation = d.Rotation,
                        Width = d.Width // để text tự xuống dòng khi vượt quá
                    };
                    return new Entity[] { mt };
                }
            });
    }
}
