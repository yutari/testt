using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using System.Collections.Generic;
using System.Linq;
using test.Services;
using test.Reporting;

namespace test.Commands
{
    public class ReportCommands
    {
        [CommandMethod("COUNT_ENTITIES_V1")]
        public void CountEntitiesTree()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var ed = doc.Editor;

            var totals = new Dictionary<string, int>();
            var root = new StatNode("ModelSpace");

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                ReadingService.CountModelSpace(ms, tr, root);
                root.Recalculate();
                tr.Commit();
            }

            using (var tr2 = db.TransactionManager.StartTransaction())
            {
                var bt = (BlockTable)tr2.GetObject(db.BlockTableId, OpenMode.ForRead);
                var ms = (BlockTableRecord)tr2.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                foreach (ObjectId id in ms)
                {
                    var ent = tr2.GetObject(id, OpenMode.ForRead) as Entity;
                    if (ent != null && !ent.IsErased)
                        ReadingService.AccumulateTotals(ent, tr2, totals);
                }
                tr2.Commit();
            }

            ed.WriteMessage("\n--------------------------------");
            ed.WriteMessage("\nBản vẽ có những thành phần sau:");
            EntityPrinter.Print(ed, root);
            ed.WriteMessage("\nTổng số các thành phần chi tiết:");
            foreach (var kv in totals.OrderBy(k => k.Key))
                ed.WriteMessage($"\n- {kv.Key}: {kv.Value}");
            ed.WriteMessage("\n--------------------------------\n");
        }
    }
}
