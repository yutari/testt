using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using System.Linq;
using test.Data;

namespace test.Converters
{
    public static class EntityConverter
    {
        public static List<SchemaEntity> Convert(List<Entity> entities)
        {
            return entities.Select(ConvertOne).ToList();
        }

        private static SchemaEntity ConvertOne(Entity ent)
        {
            var schema = new SchemaEntity
            {
                Handle = ent.Handle.ToString(),
                Type = ent.GetType().Name,
                Layer = ent.Layer,
                Color = ent.Color?.ColorNameForDisplay,
                Linetype = ent.Linetype,
                Lineweight = ent.LineWeight.ToString(),
                Geometry = new GeometrySchema(),
                XData = XDataConverter.ToDto(ent)
            };

            var g = schema.Geometry;

            // Chi tiết theo loại
            switch (ent)
            {
                case Line line:
                    g.Start = new PointSchema { X = line.StartPoint.X, Y = line.StartPoint.Y, Z = line.StartPoint.Z };
                    g.End = new PointSchema { X = line.EndPoint.X, Y = line.EndPoint.Y, Z = line.EndPoint.Z };
                    break;

                case Circle circle:
                    g.Center = new PointSchema { X = circle.Center.X, Y = circle.Center.Y, Z = circle.Center.Z };
                    g.Radius = circle.Radius;
                    break;

                case Arc arc:
                    g.Center = new PointSchema { X = arc.Center.X, Y = arc.Center.Y, Z = arc.Center.Z };
                    g.Radius = arc.Radius;
                    g.StartAngle = arc.StartAngle;
                    g.EndAngle = arc.EndAngle;
                    break;

                case Polyline pl:
                    g.Vertices = new List<PolylineVertexSchema>();
                    for (int i = 0; i < pl.NumberOfVertices; i++)
                    {
                        var pt = pl.GetPoint2dAt(i);
                        g.Vertices.Add(new PolylineVertexSchema { X = pt.X, Y = pt.Y, Bulge = pl.GetBulgeAt(i) });
                    }
                    g.Closed = pl.Closed;
                    break;

                case DBText dbt:
                    g.Position = new PointSchema { X = dbt.Position.X, Y = dbt.Position.Y, Z = dbt.Position.Z };
                    g.TextString = dbt.TextString;
                    g.Height = dbt.Height;
                    g.Rotation = dbt.Rotation;
                    g.StyleName = dbt.TextStyleName;
                    break;

                case MText mt:
                    g.Location = new PointSchema { X = mt.Location.X, Y = mt.Location.Y, Z = mt.Location.Z };
                    g.Contents = mt.Contents;
                    g.Height = mt.TextHeight;
                    g.Rotation = mt.Rotation;
                    g.Width = mt.Width;
                    g.StyleName = mt.TextStyleName;
                    g.LineSpacingFactor = mt.LineSpacingFactor;
                    g.LineSpacingStyle = mt.LineSpacingStyle.ToString();
                    break;

                case Leader leader:
                    g.Points = new List<PointSchema>();
                    for (int i = 0; i < leader.NumVertices; i++)
                    {
                        var pt = leader.VertexAt(i);
                        g.Points.Add(new PointSchema { X = pt.X, Y = pt.Y, Z = pt.Z });
                    }
                    break;

                default:
                    // các loại khác hiện chưa xử lý
                    break;
            }

            return schema;
        }
    }

    public static class XDataConverter
    {
        public static List<(DxfCode type, object value)> ConvertValues(XDataDto dto)
        {
            var result = new List<(DxfCode, object)>();
            if (dto == null || dto.Values == null) return result;

            foreach (var v in dto.Values)
            {
                switch (v.Type?.ToLowerInvariant())
                {
                    case "string":
                        result.Add((DxfCode.ExtendedDataAsciiString, v.Value?.ToString()));
                        break;
                    case "int":
                        result.Add((DxfCode.ExtendedDataInteger32, System.Convert.ToInt32(v.Value)));
                        break;
                    case "double":
                        result.Add((DxfCode.ExtendedDataReal, System.Convert.ToDouble(v.Value)));
                        break;
                }
            }
            return result;
        }

        public static XDataDto ToDto(Entity ent)
        {
            if (ent.XData == null) return null;

            var rb = ent.XData;
            var values = new List<XDataValue>();
            string appName = null;

            foreach (var tv in rb.AsArray())
            {
                if (tv.TypeCode == (int)DxfCode.ExtendedDataRegAppName)
                {
                    appName = tv.Value?.ToString();
                }
                else if (tv.TypeCode == (int)DxfCode.ExtendedDataAsciiString)
                {
                    values.Add(new XDataValue { Type = "string", Value = tv.Value });
                }
                else if (tv.TypeCode == (int)DxfCode.ExtendedDataInteger32)
                {
                    values.Add(new XDataValue { Type = "int", Value = tv.Value });
                }
                else if (tv.TypeCode == (int)DxfCode.ExtendedDataReal)
                {
                    values.Add(new XDataValue { Type = "double", Value = tv.Value });
                }
            }

            return new XDataDto { AppName = appName, Values = values };
        }
    }
}
