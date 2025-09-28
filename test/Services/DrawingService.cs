using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.ApplicationServices;
using System.Collections.Generic;
using test.Converters; // XDataConverter
using test.Parsers.Base;
using test.Services;   // DrawingHelper
using test.Data;

namespace test.Services
{
    public static class DrawingService
    {
        private static void ApplyCommonProps(Entity ent, dynamic d, Database db, Transaction tr)
        {
            DrawingHelper.ApplyLayer(ent, d.Layer, db, tr);
            DrawingHelper.ApplyColor(ent, d.Color);

            if (!string.IsNullOrWhiteSpace(d.Linetype))
                ent.Linetype = d.Linetype;

            if (d.Lineweight != null)
                ent.LineWeight = d.Lineweight.Value;

            if (d.XData != null)
                DrawingHelper.ApplyXData(ent, d.XData.AppName,
                    XDataConverter.ConvertValues(d.XData), tr, db);
        }

        private static List<ObjectId> Draw<T>(IEnumerable<T> datas, Database db, System.Func<T, IEnumerable<Entity>> factory)
        {
            var ids = new List<ObjectId>();
            if (datas == null) return ids;

            var entities = new List<Entity>();
            foreach (var d in datas)
            {
                var created = factory(d);
                if (created != null)
                    entities.AddRange(created);
            }

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                foreach (var e in entities)
                {
                    ms.AppendEntity(e);
                    tr.AddNewlyCreatedDBObject(e, true);
                    ids.Add(e.ObjectId);
                }

                tr.Commit();
            }

            return ids;
        }

        public static List<ObjectId> DrawLines(IEnumerable<LineData> lines, Database db) =>
            Draw(lines, db, d =>
            {
                var p1 = new Point3d(d.Start.X, d.Start.Y, d.Start.Z);
                var p2 = new Point3d(d.End.X, d.End.Y, d.End.Z);
                var line = new Line(p1, p2);

                using (var tr = db.TransactionManager.StartTransaction())
                {
                    ApplyCommonProps(line, d, db, tr);
                    tr.Commit();
                }
                return new[] { line };
            });

        public static List<ObjectId> DrawCircles(IEnumerable<CircleData> circles, Database db) =>
            Draw(circles, db, d =>
            {
                var center = new Point3d(d.Center.X, d.Center.Y, d.Center.Z);
                var circle = new Circle(center, Vector3d.ZAxis, d.Radius);

                using (var tr = db.TransactionManager.StartTransaction())
                {
                    ApplyCommonProps(circle, d, db, tr);
                    tr.Commit();
                }
                return new[] { circle };
            });

        public static List<ObjectId> DrawPolylines(IEnumerable<PolylineData> polylines, Database db) =>
            Draw(polylines, db, d =>
            {
                var pl = new Polyline();
                for (int i = 0; i < d.Vertices.Count; i++)
                {
                    var v = d.Vertices[i];
                    pl.AddVertexAt(i, new Point2d(v.Point.X, v.Point.Y), v.Bulge, 0, 0);
                }
                pl.Closed = d.IsClosed;

                using (var tr = db.TransactionManager.StartTransaction())
                {
                    ApplyCommonProps(pl, d, db, tr);
                    tr.Commit();
                }
                return new[] { pl };
            });

        public static List<ObjectId> DrawArcs(IEnumerable<ArcData> arcs, Database db) =>
            Draw(arcs, db, d =>
            {
                var center = new Point3d(d.Center.X, d.Center.Y, d.Center.Z);
                var arc = new Arc(center, d.Radius, d.StartAngle, d.EndAngle);

                using (var tr = db.TransactionManager.StartTransaction())
                {
                    ApplyCommonProps(arc, d, db, tr);
                    tr.Commit();
                }
                return new[] { arc };
            });

        public static List<ObjectId> DrawLeaders(IEnumerable<LeaderData> leaders, Database db) =>
            Draw(leaders, db, d =>
            {
                if (d.Points.Count < 2) return null;

                var leader = new Leader { HasArrowHead = true };
                foreach (var pt in d.Points)
                    leader.AppendVertex(new Point3d(pt.X, pt.Y, pt.Z));

                Entity textEntity = null;
                if (!string.IsNullOrEmpty(d.Text))
                {
                    var lastPt = d.Points[d.Points.Count - 1];
                    textEntity = new MText
                    {
                        Location = new Point3d(lastPt.X, lastPt.Y, lastPt.Z),
                        Contents = d.Text,
                        TextHeight = 2.5
                    };
                }

                using (var tr = db.TransactionManager.StartTransaction())
                {
                    ApplyCommonProps(leader, d, db, tr);
                    if (textEntity != null)
                        ApplyCommonProps(textEntity, d, db, tr);

                    tr.Commit();
                }

                return textEntity != null ? new Entity[] { leader, textEntity } : new Entity[] { leader };
            });

        public static List<ObjectId> DrawTexts(IEnumerable<TextData> texts, Database db) =>
            Draw(texts, db, d =>
            {
                Entity textEntity;

                if (d.Type == TextType.DBText)
                {
                    var dbt = new DBText
                    {
                        Position = new Point3d(d.Position.X, d.Position.Y, d.Position.Z),
                        Rotation = d.Rotation,
                        TextString = d.Content,
                        Height = d.Height
                    };
                    if (!string.IsNullOrEmpty(d.StyleName))
                        dbt.TextStyleId = DrawingHelper.EnsureTextStyle(db, d.StyleName);
                    textEntity = dbt;
                }
                else
                {
                    var mt = new MText
                    {
                        Location = new Point3d(d.Position.X, d.Position.Y, d.Position.Z),
                        Contents = d.Content,
                        TextHeight = d.Height,
                        Rotation = d.Rotation,
                        Width = d.Width,
                        LineSpacingFactor = d.LineSpacingFactor,
                        LineSpacingStyle = d.LineSpacingStyle
                    };
                    if (!string.IsNullOrEmpty(d.StyleName))
                        mt.TextStyleId = DrawingHelper.EnsureTextStyle(db, d.StyleName);
                    textEntity = mt;
                }

                using (var tr = db.TransactionManager.StartTransaction())
                {
                    ApplyCommonProps(textEntity, d, db, tr);
                    tr.Commit();
                }
                return new[] { textEntity };
            });
    }
}
