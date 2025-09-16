using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;

namespace test.Services
{
    /// <summary>
    /// Helper dùng chung để thêm entity vào ModelSpace.
    /// </summary>
    public static class DrawingHelper
    {
        /// <summary>
        /// Thêm một loạt entity vào ModelSpace và commit transaction.
        /// </summary>
        public static void AddEntities(Database db, IEnumerable<Entity> entities)
        {
            if (entities == null) return;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                foreach (var e in entities)
                {
                    ms.AppendEntity(e);
                    tr.AddNewlyCreatedDBObject(e, true);
                }

                tr.Commit();
            }
        }
    }
}
