using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using test.Data;
using test.Utils;
using test.Services;

namespace test.Facade
{
    /// <summary>
    /// Facade xử lý yêu cầu UPDATE_FROM_JSON.
    /// </summary>
    public static class UpdateFacade
    {
        public static void Run(Database db, UpdateBatch batch)
        {
            if (db == null || batch == null || batch.Requests.Count == 0) return;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                foreach (var req in batch.Requests)
                {
                    if (req == null || string.IsNullOrEmpty(req.Handle) || req.Data == null)
                        continue;

                    // 1. Validate & fill default values
                    SchemaValidator.ValidateAndFill(req.Data);

                    try
                    {
                        // 2. Lấy ObjectId từ Handle
                        var handle = new Handle(System.Convert.ToInt64(req.Handle, 16));
                        var id = db.GetObjectId(false, handle, 0);

                        var ent = tr.GetObject(id, OpenMode.ForWrite) as Entity;
                        if (ent != null)
                        {
                            // 3. Gọi UpdateService để cập nhật
                            UpdateService.Apply(ent, req.Data, db, tr);
                        }
                    }
                    catch
                    {
                        // Nếu không tìm thấy handle hoặc lỗi khác → bỏ qua request đó
                        continue;
                    }
                }

                tr.Commit();
            }
        }
    }
}
