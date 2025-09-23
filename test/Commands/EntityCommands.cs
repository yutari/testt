using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using System.IO;
using test.Data;
using test.Facade;
using test.IO;
using test.Converters;

namespace test.Commands
{
    /// <summary>
    /// Lệnh automation:
    /// - DRAW_FROM_JSON: vẽ từ file JSON (DrawingBatchDto)
    /// - SELECT_FROM_JSON: chọn theo JSON (SelectionRequest) và xuất kết quả ra JSON
    /// </summary>
    public class EntityCommands
    {
        [CommandMethod("DRAW_FROM_JSON")]
        public void DrawFromJson()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            var pr = new PromptStringOptions("\nNhập đường dẫn file JSON vẽ: ") { AllowSpaces = true };
            var res = ed.GetString(pr);
            if (res.Status != PromptStatus.OK) return;

            var path = res.StringResult.Trim('"');
            if (!File.Exists(path))
            {
                ed.WriteMessage("\nFile không tồn tại.");
                return;
            }

            try
            {
                string json = File.ReadAllText(path);
                var batch = DrawingBatch.FromJson(json);
                DrawingFacade.DrawAll(db, batch);
                ed.WriteMessage("\n✔ Đã vẽ xong từ JSON.");
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

            var inOpt = new PromptStringOptions("\nNhập đường dẫn file JSON yêu cầu chọn: ") { AllowSpaces = true };
            var inRes = ed.GetString(inOpt);
            if (inRes.Status != PromptStatus.OK) return;

            var outOpt = new PromptStringOptions("\nNhập đường dẫn file output (JSON): ") { AllowSpaces = true };
            var outRes = ed.GetString(outOpt);
            if (outRes.Status != PromptStatus.OK) return;

            var inPath = inRes.StringResult.Trim('"');
            var outPath = outRes.StringResult.Trim('"');

            if (!File.Exists(inPath))
            {
                ed.WriteMessage("\nFile yêu cầu chọn không tồn tại.");
                return;
            }

            try
            {
                var req = JsonFile.Read<SelectionRequest>(inPath);
                var sset = SelectionFacade.Run(ed, db, req, false);
                if (sset == null)
                {
                    ed.WriteMessage("\nKhông chọn được đối tượng nào.");
                    return;
                }

                var entities = sset.GetObjectIds();
                var list = new System.Collections.Generic.List<Entity>();
                using (var tr = db.TransactionManager.StartTransaction())
                {
                    for (int i = 0; i < entities.Length; i++)
                    {
                        var e = tr.GetObject(entities[i], OpenMode.ForRead) as Entity;
                        if (e != null) list.Add(e);
                    }
                    tr.Commit();
                }

                var selectedData = EntityConverter.Convert(list);
                DataExporter.Export(selectedData, outPath, ExportFormat.Json);

                ed.WriteMessage("\n✔ Đã xuất kết quả chọn ra file.");
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage("\nLỗi: " + ex.Message);
            }
        }

    }

}
