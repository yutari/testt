using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using test.Data;
using test.Services;

namespace test.Facade
{
    /// <summary>
    /// Facade cho chức năng chèn block từ file ngoài.
    /// Nhận BlockBatch (từ JSON), validate và gọi xuống BlockService.
    /// </summary>
    public static class BlockFacade
    {
        /// <summary>
        /// Chèn toàn bộ block trong batch.
        /// </summary>
        public static List<ObjectId> Run(Database db, BlockBatch batch)
        {
            if (db == null || batch == null || batch.Blocks.Count == 0)
                return new List<ObjectId>();

            return BlockService.InsertBlocks(db, batch);
        }
    }
}

