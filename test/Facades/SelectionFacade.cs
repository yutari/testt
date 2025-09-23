using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using netDxf; // để dùng AciColor
using System.Collections.Generic;
using test.Data;

namespace test.Facade
{
    public static class SelectionFacade
    {
        // Struct RGB đơn giản
        private struct Rgb
        {
            public byte R, G, B;
            public Rgb(byte r, byte g, byte b) { R = r; G = g; B = b; }
            public override string ToString() => $"({R},{G},{B})";
        }

        /// <summary>
        /// Thực thi lựa chọn theo SelectionRequest (JSON).
        /// </summary>
        public static SelectionSet Run(Editor ed, Database db, SelectionRequest req, bool isInteractive = false)
        {
            if (ed == null || db == null || req == null) return null;

            // 1. Build SelectionFilter sơ bộ (EntityTypes, Layers, Blocks)
            var filterList = new List<TypedValue>();

            if (req.Filters?.EntityTypes != null && req.Filters.EntityTypes.Count > 0)
            {
                string finalTypes = string.Join(",", req.Filters.EntityTypes).ToUpper();
                filterList.Add(new TypedValue((int)DxfCode.Start, finalTypes));
            }

            if (req.Filters?.Layers != null && req.Filters.Layers.Count > 0)
            {
                string finalLayers = string.Join(",", req.Filters.Layers);
                filterList.Add(new TypedValue((int)DxfCode.LayerName, finalLayers));
            }

            if (req.Filters?.Blocks != null && req.Filters.Blocks.Count > 0)
            {
                string finalBlocks = string.Join(",", req.Filters.Blocks);
                filterList.Add(new TypedValue((int)DxfCode.BlockName, finalBlocks));
            }

            var filter = filterList.Count > 0 ? new SelectionFilter(filterList.ToArray()) : null;

            // 2. Thực hiện select sơ bộ
            PromptSelectionResult res;
            string scope = (req.Scope ?? "all").ToLowerInvariant();
            if (scope == "window" && req.Window != null)
            {
                var p1 = new Point3d(req.Window.P1X, req.Window.P1Y, 0);
                var p2 = new Point3d(req.Window.P2X, req.Window.P2Y, 0);
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

                    if (req.Filters?.Colors == null || req.Filters.Colors.Count == 0)
                    {
                        resultIds.Add(id);
                        continue;
                    }

                    var effColor = ResolveEntityRgb(ent, tr);
                    if (MatchWithTargetColors(effColor, req.Filters.Colors))
                        resultIds.Add(id);
                }
                tr.Commit();
            }

            return resultIds.Count > 0 ? SelectionSet.FromObjectIds(resultIds.ToArray()) : null;
        }

        /// <summary>
        /// Resolve màu thực sự hiển thị của entity (ByLayer, ByBlock, TrueColor, ACI).
        /// </summary>
        private static Rgb ResolveEntityRgb(Entity ent, Transaction tr)
        {
            var c = ent.Color;

            // ByLayer → lấy màu của Layer
            if (c.IsByLayer)
            {
                var layer = (LayerTableRecord)tr.GetObject(ent.LayerId, OpenMode.ForRead);
                return ConvertAcColor(layer.Color);
            }

            // ByBlock → lấy màu của BlockReference
            if (c.IsByBlock && ent is BlockReference br)
            {
                return ConvertAcColor(br.Color);
            }

            // TrueColor hoặc ACI trực tiếp
            return ConvertAcColor(c);
        }

        /// <summary>
        /// Convert Autodesk AutoCAD Color → RGB
        /// </summary>
        private static Rgb ConvertAcColor(Autodesk.AutoCAD.Colors.Color acCol)
        {
            switch (acCol.ColorMethod)
            {
                case Autodesk.AutoCAD.Colors.ColorMethod.ByColor:
                    return new Rgb(acCol.Red, acCol.Green, acCol.Blue);

                case Autodesk.AutoCAD.Colors.ColorMethod.ByAci:
                    var aci = AciColor.FromCadIndex(acCol.ColorIndex);
                    var sys = aci.ToColor(); // System.Drawing.Color
                    return new Rgb(sys.R, sys.G, sys.B);

                default:
                    return new Rgb(0, 0, 0);
            }
        }

        /// <summary>
        /// So sánh màu thực tế với danh sách màu từ JSON (chuỗi dạng "rgb(r,g,b)").
        /// </summary>
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
