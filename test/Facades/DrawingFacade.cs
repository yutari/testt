using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using test.Data;
using test.Services;

namespace test.Facade
{
    /// <summary>Nhận DrawingBatch → gọi DrawingService, gom ObjectId trả ra.</summary>
    public static class DrawingFacade
    {
        public static List<ObjectId> DrawAll(Database db, DrawingBatch batch)
        {
            var ids = new List<ObjectId>();
            if (db == null || batch == null) return ids;

            if (batch.Lines?.Count > 0) ids.AddRange(DrawingService.DrawLines(batch.Lines, db));
            if (batch.Circles?.Count > 0) ids.AddRange(DrawingService.DrawCircles(batch.Circles, db));
            if (batch.Arcs?.Count > 0) ids.AddRange(DrawingService.DrawArcs(batch.Arcs, db));
            if (batch.Polylines?.Count > 0) ids.AddRange(DrawingService.DrawPolylines(batch.Polylines, db));
            if (batch.Leaders?.Count > 0) ids.AddRange(DrawingService.DrawLeaders(batch.Leaders, db));
            if (batch.Texts?.Count > 0) ids.AddRange(DrawingService.DrawTexts(batch.Texts, db));

            return ids;
        }
    }
}

