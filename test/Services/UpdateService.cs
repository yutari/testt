using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using test.Converters;
using test.Data;

namespace test.Services
{
    /// <summary>
    /// Service cập nhật Entity từ SchemaEntity.
    /// Chỉ xử lý thao tác với AutoCAD Entity, không lo default (do Facade/Validator lo).
    /// </summary>
    public static class UpdateService
    {
        public static void Apply(Entity ent, SchemaEntity schema, Database db, Transaction tr)
        {
            if (ent == null || schema == null) return;

            // -------- Thuộc tính chung --------
            if (!string.IsNullOrWhiteSpace(schema.Layer))
                DrawingHelper.ApplyLayer(ent, schema.Layer, db, tr);

            if (!string.IsNullOrWhiteSpace(schema.Color))
            {
                if (string.Equals(schema.Color, "ByLayer", StringComparison.OrdinalIgnoreCase))
                {
                    ent.Color = Color.FromColorIndex(ColorMethod.ByLayer, 256);
                }
                else
                {
                    DrawingHelper.ApplyColor(ent, schema.Color);
                }
            }


            if (!string.IsNullOrWhiteSpace(schema.Linetype))
                ent.Linetype = schema.Linetype;

            if (!string.IsNullOrWhiteSpace(schema.Lineweight) &&
                System.Enum.TryParse<LineWeight>(schema.Lineweight, out var lw))
                ent.LineWeight = lw;

            if (schema.XData != null)
                DrawingHelper.ApplyXData(ent, schema.XData.AppName,
                    XDataConverter.ConvertValues(schema.XData), tr, db);

            // -------- Hình học theo loại --------
            if (schema.Geometry != null)
                UpdateGeometry(ent, schema.Geometry, db);
        }

        private static void UpdateGeometry(Entity ent, GeometrySchema g, Database db)
        {
            switch (ent)
            {
                case Line line:
                    if (g.Start != null)
                        line.StartPoint = new Point3d(g.Start.X, g.Start.Y, g.Start.Z);
                    if (g.End != null)
                        line.EndPoint = new Point3d(g.End.X, g.End.Y, g.End.Z);
                    break;

                case Circle circle:
                    if (g.Center != null)
                        circle.Center = new Point3d(g.Center.X, g.Center.Y, g.Center.Z);
                    if (g.Radius.HasValue)
                        circle.Radius = g.Radius.Value;
                    break;

                case Arc arc:
                    if (g.Center != null)
                        arc.Center = new Point3d(g.Center.X, g.Center.Y, g.Center.Z);
                    if (g.Radius.HasValue)
                        arc.Radius = g.Radius.Value;
                    if (g.StartAngle.HasValue)
                        arc.StartAngle = g.StartAngle.Value;
                    if (g.EndAngle.HasValue)
                        arc.EndAngle = g.EndAngle.Value;
                    break;

                case Polyline pl:
                    if (g.Vertices != null && g.Vertices.Count > 0)
                    {
                        pl.UpgradeOpen();

                        for (int i = pl.NumberOfVertices - 1; i >= 0; i--)
                            pl.RemoveVertexAt(i);

                        int idx = 0;
                        foreach (var v in g.Vertices)
                            pl.AddVertexAt(idx++, new Point2d(v.X, v.Y), v.Bulge, 0, 0);

                        if (g.Closed.HasValue)
                            pl.Closed = g.Closed.Value;
                    }
                    break;

                case DBText dbt:
                    if (g.Position != null)
                        dbt.Position = new Point3d(g.Position.X, g.Position.Y, g.Position.Z);
                    if (!string.IsNullOrEmpty(g.TextString))
                        dbt.TextString = g.TextString;
                    if (g.Height.HasValue)
                        dbt.Height = g.Height.Value;
                    if (g.Rotation.HasValue)
                        dbt.Rotation = g.Rotation.Value;
                    if (!string.IsNullOrEmpty(g.StyleName))
                        dbt.TextStyleId = DrawingHelper.EnsureTextStyle(db, g.StyleName);
                    break;

                case MText mt:
                    if (g.Location != null)
                        mt.Location = new Point3d(g.Location.X, g.Location.Y, g.Location.Z);
                    if (!string.IsNullOrEmpty(g.Contents))
                        mt.Contents = g.Contents;
                    if (g.Height.HasValue)
                        mt.TextHeight = g.Height.Value;
                    if (g.Rotation.HasValue)
                        mt.Rotation = g.Rotation.Value;
                    if (g.Width.HasValue)
                        mt.Width = g.Width.Value;
                    if (g.LineSpacingFactor.HasValue)
                        mt.LineSpacingFactor = g.LineSpacingFactor.Value;
                    if (!string.IsNullOrEmpty(g.LineSpacingStyle) &&
                        System.Enum.TryParse<LineSpacingStyle>(g.LineSpacingStyle, out var ls))
                        mt.LineSpacingStyle = ls;
                    if (!string.IsNullOrEmpty(g.StyleName))
                        mt.TextStyleId = DrawingHelper.EnsureTextStyle(db, g.StyleName);
                    break;

                case Leader leader:
                    if (g.Points != null && g.Points.Count > 0)
                    {
                        leader.UpgradeOpen();

                        foreach (var pt in g.Points)
                            leader.AppendVertex(new Point3d(pt.X, pt.Y, pt.Z));
                    }
                    break;

                default:
                    // Chưa hỗ trợ loại entity khác
                    break;
            }
        }
    }
}
