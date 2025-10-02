using Xunit;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using test.Data;
using test.Services;
using test.Parsers.Base;
using test.Tests.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace test.Tests.IntegrationTests
{
    public class SelectionTests : GeometryTestBase
    {
        private Editor _ed => Application.DocumentManager.MdiActiveDocument.Editor;

        [Fact]
        public void SelectAll_NoFilter_ShouldReturnAllEntities()
        {
            ClearModelSpace();

            DrawingService.DrawLines(new[] {
                new LineData { Start = new PointSchema{X=0,Y=0}, End=new PointSchema{X=100,Y=0}, Layer="LayerA", Color="rgb(255,0,0)" }
            }, _db);

            DrawingService.DrawCircles(new[] {
                new CircleData { Center=new PointSchema{X=50,Y=50}, Radius=10, Layer="LayerB", Color="rgb(0,255,0)" }
            }, _db);

            var item = new SelectionItem { Scope = "all", Filters = new SelectionFilters() };
            var sset = SelectionService.RunSingle(_ed, _db, item);

            Assert.NotNull(sset);
            Assert.Equal(2, sset.GetObjectIds().Length);
        }

        [Fact]
        public void Select_FilterByType_ShouldReturnOnlyLines()
        {
            ClearModelSpace();

            DrawingService.DrawLines(new[] {
                new LineData { Start=new PointSchema{X=0,Y=0}, End=new PointSchema{X=100,Y=0}, Layer="LayerA", Color="rgb(255,0,0)" }
            }, _db);
            DrawingService.DrawCircles(new[] {
                new CircleData { Center=new PointSchema{X=50,Y=50}, Radius=10, Layer="LayerB", Color="rgb(0,255,0)" }
            }, _db);

            var item = new SelectionItem
            {
                Scope = "all",
                Filters = new SelectionFilters { EntityTypes = new List<string> { "LINE" } }
            };
            var sset = SelectionService.RunSingle(_ed, _db, item);

            Assert.NotNull(sset);
            var ids = sset.GetObjectIds();
            Assert.Single(ids); // chỉ còn 1 Line
        }

        [Fact]
        public void Select_FilterByLayer_ShouldReturnOnlyFromThatLayer()
        {
            ClearModelSpace();

            DrawingService.DrawLines(new[] {
                new LineData { Start=new PointSchema{X=0,Y=0}, End=new PointSchema{X=100,Y=0}, Layer="TargetLayer", Color="rgb(255,0,0)" }
            }, _db);
            DrawingService.DrawCircles(new[] {
                new CircleData { Center=new PointSchema{X=50,Y=50}, Radius=10, Layer="OtherLayer", Color="rgb(0,255,0)" }
            }, _db);

            var item = new SelectionItem
            {
                Scope = "all",
                Filters = new SelectionFilters { Layers = new List<string> { "TargetLayer" } }
            };
            var sset = SelectionService.RunSingle(_ed, _db, item);

            Assert.NotNull(sset);
            var ids = sset.GetObjectIds();
            Assert.Single(ids); // chỉ còn entity trong TargetLayer
        }

        [Fact]
        public void Select_FilterByColor_ShouldReturnOnlyMatchingColor()
        {
            ClearModelSpace();

            DrawingService.DrawLines(new[] {
                new LineData { Start=new PointSchema{X=0,Y=0}, End=new PointSchema{X=100,Y=0}, Layer="LayerA", Color="rgb(255,0,0)" } // đỏ
            }, _db);
            DrawingService.DrawCircles(new[] {
                new CircleData { Center=new PointSchema{X=50,Y=50}, Radius=10, Layer="LayerA", Color="rgb(0,255,0)" } // xanh
            }, _db);

            var item = new SelectionItem
            {
                Scope = "all",
                Filters = new SelectionFilters { Colors = new List<string> { "rgb(0,255,0)" } }
            };
            var sset = SelectionService.RunSingle(_ed, _db, item);

            Assert.NotNull(sset);
            var ids = sset.GetObjectIds();
            Assert.Single(ids); // chỉ Circle xanh
        }

        [Fact]
        public void Select_FilterByTypeLayerAndColor_ShouldReturnOnlyValidEntities()
        {
            ClearModelSpace();

            // hợp lệ: Line đỏ trong TargetLayer
            DrawingService.DrawLines(new[] {
                new LineData { Start=new PointSchema{X=0,Y=0}, End=new PointSchema{X=100,Y=0}, Layer="TargetLayer", Color="rgb(255,0,0)" }
            }, _db);

            // hợp lệ: Circle xanh trong TargetLayer
            DrawingService.DrawCircles(new[] {
                new CircleData { Center=new PointSchema{X=50,Y=50}, Radius=10, Layer="TargetLayer", Color="rgb(0,255,0)" }
            }, _db);

            // không hợp lệ: Circle xanh dương
            DrawingService.DrawCircles(new[] {
                new CircleData { Center=new PointSchema{X=80,Y=80}, Radius=5, Layer="TargetLayer", Color="rgb(0,0,255)" }
            }, _db);

            // không hợp lệ: Line đỏ nhưng sai Layer
            DrawingService.DrawLines(new[] {
                new LineData { Start=new PointSchema{X=10,Y=10}, End=new PointSchema{X=20,Y=20}, Layer="OtherLayer", Color="rgb(255,0,0)" }
            }, _db);

            var item = new SelectionItem
            {
                Scope = "all",
                Filters = new SelectionFilters
                {
                    EntityTypes = new List<string> { "LINE", "CIRCLE" },
                    Layers = new List<string> { "TargetLayer" },
                    Colors = new List<string> { "rgb(255,0,0)", "rgb(0,255,0)" }
                }
            };

            var sset = SelectionService.RunSingle(_ed, _db, item);

            Assert.NotNull(sset);
            var ids = sset.GetObjectIds();
            Assert.Equal(2, ids.Length); // chỉ còn Line đỏ + Circle xanh
        }

        [Fact]
        public void SelectWindow_ShouldReturnEntitiesInsideWindow()
        {
            ClearModelSpace();

            // entity trong cửa sổ
            DrawingService.DrawCircles(new[] {
                new CircleData { Center=new PointSchema{X=5,Y=5}, Radius=2, Layer="LayerWin", Color="rgb(255,0,0)" }
            }, _db);

            // entity ngoài cửa sổ
            DrawingService.DrawCircles(new[] {
                new CircleData { Center=new PointSchema{X=100,Y=100}, Radius=2, Layer="LayerWin", Color="rgb(0,255,0)" }
            }, _db);

            var item = new SelectionItem
            {
                Scope = "window",
                Window = new WindowRect
                {
                    P1X = 0,
                    P1Y = 0,
                    P2X = 20,
                    P2Y = 20
                },
                Filters = new SelectionFilters
                {
                    EntityTypes = new List<string> { "CIRCLE" }
                }
            };

            var sset = SelectionService.RunSingle(_ed, _db, item);

            Assert.NotNull(sset);
            var ids = sset.GetObjectIds();
            Assert.Single(ids); // chỉ Circle trong cửa sổ
        }
    }
}
