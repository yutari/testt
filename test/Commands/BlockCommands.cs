using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using test.Converters;
using test.Data;
using test.Facade;
using test.IO;

namespace test.Commands
{
    public class BlockCommands
    {
        [CommandMethod("INSERT_BLOCK_FROM_JSON")]
        public void InsertBlockFromJson()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            var pr = new PromptStringOptions("\nNhập đường dẫn file JSON insert block: ") { AllowSpaces = true };
            var res = ed.GetString(pr);
            if (res.Status != PromptStatus.OK) return;

            var path = res.StringResult.Trim('"');
            if (!System.IO.File.Exists(path))
            {
                ed.WriteMessage("\nFile không tồn tại.");
                return;
            }

            try
            {
                var batch = test.IO.JsonFile.Read<BlockBatch>(path);

                ed.WriteMessage($"\nBatch null? {(batch == null)}");
                ed.WriteMessage($"\nBlocks null? {(batch.Blocks == null)}");
                if (batch?.Blocks != null && batch.Blocks.Count > 0)
                {
                    var first = batch.Blocks[0];
                    ed.WriteMessage($"\nFirst block null? {(first == null)}");
                    ed.WriteMessage($"\nFilePath: {first?.FilePath}");
                    ed.WriteMessage($"\nBlockName: {first?.BlockName}");
                    ed.WriteMessage($"\nPosition null? {(first?.Position == null)}");
                }
                var ids = BlockFacade.Run(db, batch);

                ed.WriteMessage($"\n✔ Đã chèn {ids.Count} block từ JSON.");
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage("\nLỗi: " + ex.Message);
            }
        }
    }
}
