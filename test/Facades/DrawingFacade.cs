using Autodesk.AutoCAD.DatabaseServices;
using test.Data;
using test.Services;

namespace test.Facade
{
    /// <summary>Nhận DrawingBatch → gọi DrawingService. Viết rõ ràng, dễ đọc.</summary>
    public static class DrawingFacade
    {
        public static void DrawAll(Database db, DrawingBatch batch)
        {
            if (db == null || batch == null) return;

            if (batch.Lines != null && batch.Lines.Count > 0)
                DrawingService.DrawLines(batch.Lines, db);

            if (batch.Circles != null && batch.Circles.Count > 0)
                DrawingService.DrawCircles(batch.Circles, db);

            if (batch.Arcs != null && batch.Arcs.Count > 0)
                DrawingService.DrawArcs(batch.Arcs, db);

            if (batch.Polylines != null && batch.Polylines.Count > 0)
                DrawingService.DrawPolylines(batch.Polylines, db);

            if (batch.Leaders != null && batch.Leaders.Count > 0)
                DrawingService.DrawLeaders(batch.Leaders, db);

            if (batch.Texts != null && batch.Texts.Count > 0)
                DrawingService.DrawTexts(batch.Texts, db);
        }
    }
}

