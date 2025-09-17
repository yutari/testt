using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using test.Utils;

namespace test.Commands
{
    public class SelectionCommands
    {
        [CommandMethod("SELECTENTITY")]
        public void SelectEntity()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            SelectionRunner.Run(ed, db);
        }
    }
}
