using Xunit;
using Autodesk.AutoCAD.DatabaseServices;
using test.Data;
using test.Parsers.Base;
using test.Services;
using test.Facade;
using test.Tests.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace test.Tests.IntegrationTests
{
    public class TestGeometryUpdate : GeometryTestBase
    {
        [Fact]
        public void Line_Update()
        {
            ClearModelSpace();

            var dto = new LineData
            {
                Start = new PointSchema { X = 0, Y = 0 },
                End = new PointSchema { X = 100, Y = 0 },
                Layer = "InitLineLayer",
                Color = "rgb(0,0,255)",
                XData = new XDataDto
                {
                    AppName = "APP_INIT",
                    Values = new List<XDataValue>
                    {
                        new XDataValue { Type = "string", Value = "hello" }
                    }
                }
            };
            DrawingService.DrawLines(new List<LineData> { dto }, _db);

            var updates = CollectUpdates<Line>(ln => new SchemaEntity
            {
                Type = "Line",
                Layer = "UpdatedLayer",
                Color = null, // giữ nguyên
                XData = new XDataDto
                {
                    AppName = "APP_UPDATED",
                    Values = new List<XDataValue>
                    {
                        new XDataValue { Type = "string", Value = "updated" }
                    }
                },
                Geometry = new GeometrySchema
                {
                    Start = new PointSchema { X = 0, Y = 10 },
                    End = new PointSchema { X = 200, Y = 10 }
                }
            });

            UpdateFacade.Run(_db, UpdateBatch.FromItems(updates));

            Assert.True(ExistsInModelSpace<Line>(l =>
                l.EndPoint.X == 200 &&
                l.Layer == "UpdatedLayer" &&
                l.XData.AsArray().Any(tv => tv.Value?.ToString() == "updated")));
        }

        [Fact]
        public void Circle_Update()
        {
            ClearModelSpace();

            var dto = new CircleData
            {
                Center = new PointSchema { X = 50, Y = 50 },
                Radius = 25,
                Layer = "InitCircleLayer"
            };
            DrawingService.DrawCircles(new List<CircleData> { dto }, _db);

            var updates = CollectUpdates<Circle>(c => new SchemaEntity
            {
                Type = "Circle",
                Layer = "UpdatedLayer",
                Color = "rgb(255,0,0)",
                XData = null,
                Geometry = new GeometrySchema
                {
                    Center = new PointSchema { X = 60, Y = 60 },
                    Radius = 50
                }
            });

            UpdateFacade.Run(_db, UpdateBatch.FromItems(updates));

            Assert.True(ExistsInModelSpace<Circle>(c =>
                c.Center.X == 60 &&
                c.Radius == 50 &&
                c.Layer == "UpdatedLayer" &&
                c.Color.Red == 255));
        }

        [Fact]
        public void Arc_Update()
        {
            ClearModelSpace();

            var dto = new ArcData
            {
                Center = new PointSchema { X = 0, Y = 0 },
                Radius = 10,
                StartAngle = 0,
                EndAngle = System.Math.PI / 2,
                Layer = "InitArcLayer",
                XData = new XDataDto
                {
                    AppName = "APP_ARC",
                    Values = new List<XDataValue>
                    {
                        new XDataValue { Type = "int", Value = 123 }
                    }
                }
            };
            DrawingService.DrawArcs(new List<ArcData> { dto }, _db);

            var updates = CollectUpdates<Arc>(a => new SchemaEntity
            {
                Type = "Arc",
                Layer = "UpdatedLayer",
                Color = null,
                XData = new XDataDto
                {
                    AppName = "APP_ARC",
                    Values = new List<XDataValue>
                    {
                        new XDataValue { Type = "int", Value = 456 }
                    }
                },
                Geometry = new GeometrySchema
                {
                    Center = new PointSchema { X = 5, Y = 5 },
                    Radius = 20,
                    StartAngle = 0.5,
                    EndAngle = 1.0
                }
            });

            UpdateFacade.Run(_db, UpdateBatch.FromItems(updates));

            Assert.True(ExistsInModelSpace<Arc>(a =>
                a.Center.X == 5 &&
                a.Radius == 20 &&
                a.Layer == "UpdatedLayer" &&
                a.XData.AsArray().Any(tv => tv.Value?.ToString() == "456")));
        }

        [Fact]
        public void Polyline_Update_OnlyProps()
        {
            ClearModelSpace();

            var dto = new PolylineData
            {
                Vertices = new List<PolylineVertex>
                {
                    new PolylineVertex { Point = new PointSchema { X = 0, Y = 0 } },
                    new PolylineVertex { Point = new PointSchema { X = 100, Y = 0 } }
                },
                IsClosed = false,
                Layer = "InitPolyLayer",
                Color = "rgb(0,255,255)"
            };
            DrawingService.DrawPolylines(new List<PolylineData> { dto }, _db);

            var updates = CollectUpdates<Polyline>(pl => new SchemaEntity
            {
                Type = "Polyline",
                Layer = "UpdatedLayer",
                Color = "rgb(0,255,0)",
                XData = new XDataDto
                {
                    AppName = "APP_POLY",
                    Values = new List<XDataValue>
                    {
                        new XDataValue { Type = "string", Value = "poly-updated" }
                    }
                },
                Geometry = null // không đổi điểm
            });

            UpdateFacade.Run(_db, UpdateBatch.FromItems(updates));

            Assert.True(ExistsInModelSpace<Polyline>(pl =>
                pl.Layer == "UpdatedLayer" &&
                pl.Color.Green == 255 &&
                pl.XData.AsArray().Any(tv => tv.Value?.ToString() == "poly-updated")));
        }

        [Fact]
        public void Leader_Update_OnlyProps()
        {
            ClearModelSpace();

            var dto = new LeaderData
            {
                Points = new List<PointSchema>
                {
                    new PointSchema { X = 0, Y = 0 },
                    new PointSchema { X = 50, Y = 50 }
                },
                Layer = "InitLeaderLayer"
            };
            DrawingService.DrawLeaders(new List<LeaderData> { dto }, _db);

            var updates = CollectUpdates<Leader>(ld => new SchemaEntity
            {
                Type = "Leader",
                Layer = "UpdatedLayer",
                Color = "rgb(255,0,0)",
                XData = new XDataDto
                {
                    AppName = "APP_LEADER",
                    Values = new List<XDataValue>
                    {
                        new XDataValue { Type = "string", Value = "leader-updated" }
                    }
                },
                Geometry = null
            });

            UpdateFacade.Run(_db, UpdateBatch.FromItems(updates));

            Assert.True(ExistsInModelSpace<Leader>(ld =>
                ld.Layer == "UpdatedLayer" &&
                ld.Color.Red == 255 &&
                ld.XData.AsArray().Any(tv => tv.Value?.ToString() == "leader-updated")));
        }

        [Fact]
        public void DbText_Update()
        {
            ClearModelSpace();

            var dto = new TextData
            {
                Position = new PointSchema { X = 10, Y = 20 },
                Content = "Hello",
                Height = 2.5,
                Type = TextType.DBText,
                Color = "rgb(255,0,255)"
            };
            DrawingService.DrawTexts(new List<TextData> { dto }, _db);

            var updates = CollectUpdates<DBText>(dbt => new SchemaEntity
            {
                Type = "DBText",
                Layer = "UpdatedLayer",
                Color = null,
                XData = new XDataDto
                {
                    AppName = "APP_TEXT",
                    Values = new List<XDataValue>
                    {
                        new XDataValue { Type = "string", Value = "updated-text" }
                    }
                },
                Geometry = new GeometrySchema
                {
                    Position = new PointSchema { X = 15, Y = 25 },
                    TextString = "Updated",
                    Height = 5,
                    Rotation = 0.5,
                    StyleName = "Standard"
                }
            });

            UpdateFacade.Run(_db, UpdateBatch.FromItems(updates));

            Assert.True(ExistsInModelSpace<DBText>(dbt =>
                dbt.TextString == "Updated" &&
                dbt.Height == 5 &&
                dbt.Layer == "UpdatedLayer" &&
                dbt.XData.AsArray().Any(tv => tv.Value?.ToString() == "updated-text")));
        }

        [Fact]
        public void MText_Update()
        {
            ClearModelSpace();

            var dto = new TextData
            {
                Position = new PointSchema { X = 10, Y = 20 },
                Content = "Multi-line",
                Height = 2.5,
                Type = TextType.MText,
                Width = 50,
                Layer = "InitMTextLayer"
            };
            DrawingService.DrawTexts(new List<TextData> { dto }, _db);

            var updates = CollectUpdates<MText>(mt => new SchemaEntity
            {
                Type = "MText",
                Layer = "UpdatedLayer",
                Color = "rgb(128,0,128)",
                XData = null,
                Geometry = new GeometrySchema
                {
                    Location = new PointSchema { X = 20, Y = 30 },
                    Contents = "Updated Text",
                    Height = 4,
                    Rotation = 0.25,
                    Width = 60,
                    LineSpacingFactor = 1.5,
                    LineSpacingStyle = "Exactly",
                    StyleName = "Standard"
                }
            });

            UpdateFacade.Run(_db, UpdateBatch.FromItems(updates));

            Assert.True(ExistsInModelSpace<MText>(mt =>
                mt.Contents.Contains("Updated") &&
                mt.Width == 60 &&
                mt.Layer == "UpdatedLayer"));
        }
    }
}
