using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using System.Collections.Generic;
using System.IO;
using System.Text;
using test.Converters;
using test.Data;
using test.Facade;
using test.IO;

namespace test.Commands
{
    public class EntityCommands
    {
        [CommandMethod("DRAW_FROM_JSON")]
        public void DrawFromJson()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            // 🔹 Chọn file input JSON
            var openOpts = new PromptOpenFileOptions("\nChọn file JSON để vẽ:");
            openOpts.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
            var openRes = ed.GetFileNameForOpen(openOpts);
            if (openRes.Status != PromptStatus.OK) return;

            var inPath = openRes.StringResult;
            if (!File.Exists(inPath))
            {
                ed.WriteMessage("\n❌ File không tồn tại.");
                return;
            }

            try
            {
                var batch = JsonFile.Read<DrawingBatch>(inPath);

                var ids = DrawingFacade.DrawAll(db, batch);

                var handleInfos = new List<object>();
                using (var tr = db.TransactionManager.StartTransaction())
                {
                    foreach (var id in ids)
                    {
                        var ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                        if (ent != null)
                        {
                            handleInfos.Add(new
                            {
                                Handle = ent.Handle.ToString(),
                                Type = ent.GetType().Name
                            });
                        }
                    }
                    tr.Commit();
                }

                var saveOpts = new PromptSaveFileOptions("\nChọn nơi lưu file handle JSON:");
                saveOpts.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                var saveRes = ed.GetFileNameForSave(saveOpts);
                if (saveRes.Status == PromptStatus.OK)
                {
                    var outPath = saveRes.StringResult;
                    DataExporter.Export(handleInfos, outPath, ExportFormat.Json);
                    ed.WriteMessage($"\n✔ Đã vẽ xong và lưu danh sách handle ra file: {outPath}");
                }
                else
                {
                    ed.WriteMessage("\n⚠ Vẽ xong nhưng bạn không lưu file handle.");
                }
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage("\nLỗi: " + ex.Message);
            }
        }

        [CommandMethod("SELECT_FROM_JSON")]
        public void SelectFromJson()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            var inOpt = new PromptOpenFileOptions("\nChọn file JSON yêu cầu chọn:");
            inOpt.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
            var inRes = ed.GetFileNameForOpen(inOpt);
            if (inRes.Status != PromptStatus.OK) return;

            var outOpt = new PromptSaveFileOptions("\nChọn nơi lưu file output (JSON):");
            outOpt.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
            var outRes = ed.GetFileNameForSave(outOpt);
            if (outRes.Status != PromptStatus.OK) return;

            var inPath = inRes.StringResult;
            var outPath = outRes.StringResult;

            if (!File.Exists(inPath))
            {
                ed.WriteMessage("\nFile yêu cầu chọn không tồn tại.");
                return;
            }

            try
            {
                var batch = JsonFile.Read<SelectionBatch>(inPath);
                var ssets = SelectionFacade.Run(ed, db, batch, false);

                if (ssets == null || ssets.Count == 0)
                {
                    ed.WriteMessage("\nKhông chọn được đối tượng nào.");
                    return;
                }

                var results = new List<List<SchemaEntity>>();
                foreach (var sset in ssets)
                {
                    var entities = new List<Entity>();
                    using (var tr = db.TransactionManager.StartTransaction())
                    {
                        foreach (var id in sset.GetObjectIds())
                        {
                            var e = tr.GetObject(id, OpenMode.ForRead) as Entity;
                            if (e != null) entities.Add(e);
                        }
                        tr.Commit();
                    }

                    var schemaList = EntityConverter.Convert(entities);
                    results.Add(schemaList);
                }
                DataExporter.Export(results, outPath, ExportFormat.Json);

                ed.WriteMessage("\n✔ Đã xuất kết quả chọn ra file.");
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage("\nLỗi: " + ex.Message);
            }
        }

        [CommandMethod("UPDATE_FROM_JSON")]
        public void UpdateFromJson()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            var fileOpts = new PromptOpenFileOptions("\nChọn file JSON update:");
            fileOpts.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
            var res = ed.GetFileNameForOpen(fileOpts);
            if (res.Status != PromptStatus.OK) return;

            var path = res.StringResult;
            if (!File.Exists(path))
            {
                ed.WriteMessage("\nFile không tồn tại.");
                return;
            }

            try
            {
                var batch = JsonFile.Read<UpdateBatch>(path);
                UpdateFacade.Run(db, batch);

                ed.WriteMessage("\n✔ Đã cập nhật entity từ JSON.");
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage("\nLỗi: " + ex.Message);
            }
        }
    }
}
