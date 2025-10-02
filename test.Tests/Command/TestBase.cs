using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using test.Data;

namespace test.Tests.Helpers
{
    public abstract class GeometryTestBase
    {
        protected readonly Database _db;

        protected GeometryTestBase()
        {
            _db = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;
        }

        protected IEnumerable<T> GetEntitiesInModelSpace<T>(Transaction tr) where T : Entity
        {
            var bt = (BlockTable)tr.GetObject(_db.BlockTableId, OpenMode.ForRead);
            var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

            foreach (ObjectId id in ms)
            {
                if (tr.GetObject(id, OpenMode.ForRead) is T ent)
                    yield return ent;
            }
        }

        protected bool ExistsInModelSpace<T>(System.Func<T, bool> predicate) where T : Entity
        {
            using var tr = _db.TransactionManager.StartTransaction();
            foreach (var ent in GetEntitiesInModelSpace<T>(tr))
            {
                if (predicate(ent)) return true;
            }
            tr.Abort();
            return false;
        }

        protected List<SchemaEntityUpdate> CollectUpdates<T>(System.Func<T, SchemaEntity> makeUpdate) where T : Entity
        {
            var updates = new List<SchemaEntityUpdate>();
            using var tr = _db.TransactionManager.StartTransaction();
            foreach (var ent in GetEntitiesInModelSpace<T>(tr))
            {
                updates.Add(new SchemaEntityUpdate
                {
                    Handle = ent.Handle.ToString(),
                    Data = makeUpdate(ent)
                });
            }
            tr.Abort();
            return updates;
        }

        protected void ClearModelSpace()
        {
            using var tr = _db.TransactionManager.StartTransaction();
            var bt = (BlockTable)tr.GetObject(_db.BlockTableId, OpenMode.ForRead);
            var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            var ids = new List<ObjectId>();
            foreach (ObjectId id in ms) ids.Add(id);

            foreach (var id in ids)
            {
                var ent = (Entity)tr.GetObject(id, OpenMode.ForWrite);
                ent.Erase();
            }
            tr.Commit();
        }
    }
}
