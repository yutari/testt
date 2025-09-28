using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using test.Data;
using test.Services;

namespace test.Facade
{
    public static class SelectionFacade
    {
        public static List<SelectionSet> Run(Editor ed, Database db, SelectionBatch batch, bool isInteractive = false)
        {
            var results = new List<SelectionSet>();
            if (ed == null || db == null || batch == null || batch.Requests.Count == 0)
                return results;

            foreach (var item in batch.Requests)
            {
                var sset = SelectionService.RunSingle(ed, db, item, isInteractive);
                if (sset != null)
                    results.Add(sset);
            }

            return results;
        }
    }
}
