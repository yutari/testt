using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using netDxf;
using test.Data;

namespace test.Services
{
    public static class SelectionService
    {
        private struct Rgb
        {
            public byte R, G, B;
            public Rgb(byte r, byte g, byte b) { R = r; G = g; B = b; }
        }

        public static SelectionSet RunSingle(Editor ed, Database db, SelectionItem item, bool isInteractive = false)
        {
            if (item == null) return null;

            // 1. Build filter
            var filterList = new List<TypedValue>();

            if (item.Filters?.EntityTypes != null && item.Filters.EntityTypes.Count > 0)
                filterList.Add(new TypedValue((int)DxfCode.Start, string.Join(",", item.Filters.EntityTypes).ToUpper()));

            if (item.Filters?.Layers != null && item.Filters.Layers.Count > 0)
                filterList.Add(new TypedValue((int)DxfCode.LayerName, string.Join(",", item.Filters.Layers)));

            if (item.Filters?.Blocks != null && item.Filters.Blocks.Count > 0)
                filterList.Add(new TypedValue((int)DxfCode.BlockName, string.Join(",", item.Filters.Blocks)));

            var filter = filterList.Count > 0 ? new SelectionFilter(filterList.ToArray()) : null;

            // 2. Select sơ bộ
            PromptSelectionResult res;
            string scope = (item.Scope ?? "all").ToLowerInvariant();
            if (scope == "window" && item.Window != null)
            {
                var p1 = new Point3d(item.Window.P1X, item.Window.P1Y, 0);
                var p2 = new Point3d(item.Window.P2X, item.Window.P2Y, 0);
                res = isInteractive ? ed.GetSelection(filter) : ed.SelectWindow(p1, p2, filter);
            }
            else
            {
                res = isInteractive ? ed.GetSelection(filter) : ed.SelectAll(filter);
            }

            if (res.Status != PromptStatus.OK) return null;

            // 3. Post-filter theo màu
            var resultIds = new List<ObjectId>();
            using (var tr = db.TransactionManager.StartTransaction())
            {
                foreach (ObjectId id in res.Value.GetObjectIds())
                {
                    var ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                    if (ent == null) continue;

                    if (item.Filters?.Colors == null || item.Filters.Colors.Count == 0)
                    {
                        resultIds.Add(id);
                        continue;
                    }

                    var effColor = ResolveEntityRgb(ent, tr);
                    if (MatchWithTargetColors(effColor, item.Filters.Colors))
                        resultIds.Add(id);
                }
                tr.Commit();
            }

            return resultIds.Count > 0 ? SelectionSet.FromObjectIds(resultIds.ToArray()) : null;
        }

        private static Rgb ResolveEntityRgb(Entity ent, Transaction tr)
        {
            var c = ent.Color;

            if (c.IsByLayer)
            {
                var layer = (LayerTableRecord)tr.GetObject(ent.LayerId, OpenMode.ForRead);
                return ConvertAcColor(layer.Color);
            }

            if (c.IsByBlock && ent is BlockReference br)
                return ConvertAcColor(br.Color);

            return ConvertAcColor(c);
        }

        private static Rgb ConvertAcColor(Autodesk.AutoCAD.Colors.Color acCol)
        {
            switch (acCol.ColorMethod)
            {
                case Autodesk.AutoCAD.Colors.ColorMethod.ByColor:
                    return new Rgb(acCol.Red, acCol.Green, acCol.Blue);
                case Autodesk.AutoCAD.Colors.ColorMethod.ByAci:
                    var aci = AciColor.FromCadIndex(acCol.ColorIndex);
                    var sys = aci.ToColor();
                    return new Rgb(sys.R, sys.G, sys.B);
                default:
                    return new Rgb(0, 0, 0);
            }
        }

        private static bool MatchWithTargetColors(Rgb actual, List<string> targets)
        {
            foreach (var s in targets)
            {
                var inner = s.Substring(4, s.Length - 5);
                var parts = inner.Split(',');
                byte r = byte.Parse(parts[0].Trim());
                byte g = byte.Parse(parts[1].Trim());
                byte b = byte.Parse(parts[2].Trim());

                if (actual.R == r && actual.G == g && actual.B == b)
                    return true;
            }
            return false;
        }
    }
}

