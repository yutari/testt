using System.Collections.Generic;
using System.IO;
using System;

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using test.Data;

namespace test.Services
{
    public static class BlockService
    {
        /// <summary>
        /// Chèn một block hoặc toàn bộ ModelSpace từ file DWG ngoài.
        /// </summary>
        public static ObjectId InsertBlock(Database targetDb, Transaction tr, BlockInsertItem item)
        {
            // Code để debug:
            //if (item == null)
            //    throw new ArgumentNullException(nameof(item));
            //if (string.IsNullOrWhiteSpace(item.FilePath))
            //    throw new Exception("❌ FilePath null hoặc rỗng.");
            //if (string.IsNullOrWhiteSpace(item.BlockName))
            //    throw new Exception("❌ BlockName null hoặc rỗng.");
            //if (!File.Exists(item.FilePath))
            //    throw new FileNotFoundException($"❌ Không tìm thấy file: {item.FilePath}");

            ObjectId brId = ObjectId.Null;

            // 1. Mở sourceDb từ file ngoài
            using (var sourceDb = new Database(false, true))
            {
                sourceDb.ReadDwgFile(item.FilePath, FileShare.ReadWrite, true, "");

                using (var trSource = sourceDb.TransactionManager.StartTransaction())
                {
                    var sourceBt = (BlockTable)trSource.GetObject(sourceDb.BlockTableId, OpenMode.ForRead);

                    if (!sourceBt.Has(item.BlockName))
                        throw new Exception($"❌ Block '{item.BlockName}' không tồn tại trong file {item.FilePath}");

                    var sourceBtrId = sourceBt[item.BlockName];

                    // 2. Clone block định nghĩa sang targetDb nếu chưa có
                    var targetBt = (BlockTable)tr.GetObject(targetDb.BlockTableId, OpenMode.ForRead);
                    if (!targetBt.Has(item.BlockName))
                    {
                        var ids = new ObjectIdCollection { sourceBtrId };
                        var mapping = new IdMapping();
                        targetDb.WblockCloneObjects(ids, targetBt.ObjectId, mapping, DuplicateRecordCloning.Replace, false);
                    }

                    trSource.Commit();
                }
            }

            // 3. Tạo BlockReference trong targetDb
            var targetBt2 = (BlockTable)tr.GetObject(targetDb.BlockTableId, OpenMode.ForRead);
            var ms = (BlockTableRecord)tr.GetObject(targetBt2[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            var pos = new Point3d(item.Position.X, item.Position.Y, item.Position.Z);
            var br = new BlockReference(pos, targetBt2[item.BlockName])
            {
                ScaleFactors = new Scale3d(item.Scale),
                Rotation = item.Rotation
            };

            ms.AppendEntity(br);
            tr.AddNewlyCreatedDBObject(br, true);

            brId = br.ObjectId;

            return brId;
        }

        /// <summary>
        /// Có thể update: Clone toàn bộ nội dung ModelSpace từ sourceDb sang targetDb.
        /// </summary>
        private static void InsertWholeDrawing(Database targetDb, Transaction tr, Database sourceDb, BlockInsertItem item)
        {
            var sourceBt = (BlockTable)sourceDb.BlockTableId.GetObject(OpenMode.ForRead);
            var sourceMs = (BlockTableRecord)sourceBt[BlockTableRecord.ModelSpace].GetObject(OpenMode.ForRead);

            var idsToClone = new ObjectIdCollection();
            foreach (ObjectId id in sourceMs)
                idsToClone.Add(id);

            var mapping = new IdMapping();

            var targetBt = (BlockTable)tr.GetObject(targetDb.BlockTableId, OpenMode.ForRead);
            var targetMs = (BlockTableRecord)tr.GetObject(targetBt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            targetDb.WblockCloneObjects(idsToClone, targetMs.ObjectId, mapping, DuplicateRecordCloning.Replace, false);

            var disp = Matrix3d.Displacement(new Vector3d(item.Position.X, item.Position.Y, item.Position.Z));
            var scale = Matrix3d.Scaling(item.Scale, new Point3d(0, 0, 0));
            var rot = Matrix3d.Rotation(item.Rotation, Vector3d.ZAxis, new Point3d(0, 0, 0));

            var transform = scale * rot * disp;

            foreach (IdPair pair in mapping)
            {
                if (pair.IsCloned)
                {
                    var ent = tr.GetObject(pair.Value, OpenMode.ForWrite) as Entity;
                    if (ent != null)
                        ent.TransformBy(transform);
                }
            }
        }

        /// <summary>
        /// Chèn nhiều block theo batch.
        /// </summary>
        public static List<ObjectId> InsertBlocks(Database db, BlockBatch batch)
        {
            var ids = new List<ObjectId>();
            if (batch == null || batch.Blocks.Count == 0) return ids;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                foreach (var item in batch.Blocks)
                {
                    var id = InsertBlock(db, tr, item);
                    if (!id.IsNull) ids.Add(id);
                }
                tr.Commit();
            }

            return ids;
        }
    }
}
