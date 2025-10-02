using Xunit;
using Autodesk.AutoCAD.DatabaseServices;
using test.Data;
using test.Parsers.Base;
using test.Services;
using test.Tests.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace test.Tests.IntegrationTests
{
    public class TestGeometryCreate : GeometryTestBase
    {
        [Fact]
        public void Line_Create_WithColorAndXData()
        {
            ClearModelSpace();
            var dto = new LineData
            {
                Start = new PointSchema { X = 0, Y = 0 },
                End = new PointSchema { X = 100, Y = 0 },
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
            Assert.True(ExistsInModelSpace<Line>(l => l.Color.Blue == 255));
        }

        [Fact]
        public void Circle_Create_WithLayerOnly()
        {
            ClearModelSpace();
            var dto = new CircleData
            {
                Center = new PointSchema { X = 50, Y = 50 },
                Radius = 25,
                Layer = "LayerRed"
            };
            DrawingService.DrawCircles(new List<CircleData> { dto }, _db);
            Assert.True(ExistsInModelSpace<Circle>(c => c.Layer == "LayerRed" && c.Color.IsByLayer));
        }

        [Fact]
        public void Arc_Create_WithLayerAndXData()
        {
            ClearModelSpace();
            var dto = new ArcData
            {
                Center = new PointSchema { X = 0, Y = 0 },
                Radius = 10,
                StartAngle = 0,
                EndAngle = System.Math.PI / 2,
                Layer = "LayerGreen",
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
            Assert.True(ExistsInModelSpace<Arc>(a => a.Layer == "LayerGreen"));
        }

        [Fact]
        public void Polyline_Create_WithLayerAndColor()
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
                Layer = "LayerPoly",
                Color = "rgb(0,255,255)"
            };
            DrawingService.DrawPolylines(new List<PolylineData> { dto }, _db);
            Assert.True(ExistsInModelSpace<Polyline>(pl => pl.Layer == "LayerPoly"));
        }

        [Fact]
        public void Leader_Create_WithLayerAndXData()
        {
            ClearModelSpace();
            var dto = new LeaderData
            {
                Points = new List<PointSchema>
                {
                    new PointSchema { X = 0, Y = 0 },
                    new PointSchema { X = 50, Y = 50 }
                },
                Layer = "LayerGreen",
                XData = new XDataDto
                {
                    AppName = "APP_LEADER",
                    Values = new List<XDataValue>
                    {
                        new XDataValue { Type = "string", Value = "leader-data" }
                    }
                }
            };
            DrawingService.DrawLeaders(new List<LeaderData> { dto }, _db);
            Assert.True(ExistsInModelSpace<Leader>(ld => ld.Layer == "LayerGreen"));
        }

        [Fact]
        public void DbText_Create_WithColorAndXData()
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
            Assert.True(ExistsInModelSpace<DBText>(dbt => dbt.TextString == "Hello"));
        }

        [Fact]
        public void MText_Create_WithLayerOnly()
        {
            ClearModelSpace();
            var dto = new TextData
            {
                Position = new PointSchema { X = 10, Y = 20 },
                Content = "Multi-line",
                Height = 2.5,
                Type = TextType.MText,
                Width = 50,
                Layer = "LayerRed"
            };
            DrawingService.DrawTexts(new List<TextData> { dto }, _db);
            Assert.True(ExistsInModelSpace<MText>(mt => mt.Layer == "LayerRed"));
        }
    }
}
