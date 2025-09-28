using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using System.Linq;

namespace test.Services
{
    /// <summary>
    /// Helper dùng chung để thêm entity vào ModelSpace.
    /// </summary>
    public static class DrawingHelper
    {
        /// <summary>
        /// Thêm một loạt entity vào ModelSpace và commit transaction.
        /// </summary>
        public static void AddEntities(Database db, IEnumerable<Entity> entities)
        {
            if (entities == null) return;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                foreach (var e in entities)
                {
                    ms.AppendEntity(e);
                    tr.AddNewlyCreatedDBObject(e, true);
                }

                tr.Commit();
            }
        }

        // ==========================
        // LAYER
        // ==========================
        public static void EnsureLayer(Database db, Transaction tr, string layerName, Color color = null)
        {
            if (string.IsNullOrWhiteSpace(layerName))
                layerName = "0";

            var lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
            if (!lt.Has(layerName))
            {
                lt.UpgradeOpen();
                var ltr = new LayerTableRecord { Name = layerName };
                if (color != null) ltr.Color = color;
                lt.Add(ltr);
                tr.AddNewlyCreatedDBObject(ltr, true);
            }
        }

        public static void ApplyLayer(Entity ent, string layerName, Database db, Transaction tr)
        {
            if (string.IsNullOrWhiteSpace(layerName))
                layerName = "0";

            EnsureLayer(db, tr, layerName);
            ent.Layer = layerName;
        }

        public static void UpdateLayer(ObjectId entId, string newLayer, Database db)
        {
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var ent = tr.GetObject(entId, OpenMode.ForWrite) as Entity;
                if (ent != null)
                {
                    ApplyLayer(ent, newLayer, db, tr);
                }
                tr.Commit();
            }
        }

        // ==========================
        // COLOR
        // ==========================
        public static Color ParseRgb(string rgb)
        {
            if (string.IsNullOrWhiteSpace(rgb)) return null;
            try
            {
                var inner = rgb.Substring(4, rgb.Length - 5);
                var parts = inner.Split(',');
                byte r = byte.Parse(parts[0].Trim());
                byte g = byte.Parse(parts[1].Trim());
                byte b = byte.Parse(parts[2].Trim());
                return Color.FromRgb(r, g, b);
            }
            catch { return null; }
        }

        public static void ApplyColor(Entity ent, string rgb)
        {
            var c = ParseRgb(rgb);
            if (c != null)
                ent.Color = c;
            else
                ent.Color = Color.FromColorIndex(ColorMethod.ByLayer, 256);
        }

        public static void UpdateColor(ObjectId entId, string rgb, Database db)
        {
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var ent = tr.GetObject(entId, OpenMode.ForWrite) as Entity;
                if (ent != null)
                    ApplyColor(ent, rgb);
                tr.Commit();
            }
        }

        // ==========================
        // XDATA
        // ==========================
        public static void EnsureRegApp(Transaction tr, Database db, string appName)
        {
            var regTable = (RegAppTable)tr.GetObject(db.RegAppTableId, OpenMode.ForRead);
            if (!regTable.Has(appName))
            {
                regTable.UpgradeOpen();
                var reg = new RegAppTableRecord { Name = appName };
                regTable.Add(reg);
                tr.AddNewlyCreatedDBObject(reg, true);
            }
        }

        public static void ApplyXData(Entity ent, string appName, List<(DxfCode type, object value)> values, Transaction tr, Database db)
        {
            if (string.IsNullOrWhiteSpace(appName) || values == null || values.Count == 0) return;

            EnsureRegApp(tr, db, appName);

            var tvs = new List<TypedValue> { new TypedValue((int)DxfCode.ExtendedDataRegAppName, appName) };
            tvs.AddRange(values.Select(v => new TypedValue((int)v.type, v.value)));
            ent.XData = new ResultBuffer(tvs.ToArray());
        }

        public static void UpdateXData(ObjectId entId, string appName, List<(DxfCode type, object value)> values, Database db)
        {
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var ent = tr.GetObject(entId, OpenMode.ForWrite) as Entity;
                if (ent != null)
                    ApplyXData(ent, appName, values, tr, db);
                tr.Commit();
            }
        }
        public static ObjectId EnsureTextStyle(Database db, string styleName)
        {
            if (string.IsNullOrWhiteSpace(styleName))
                styleName = "Standard";

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var tst = (TextStyleTable)tr.GetObject(db.TextStyleTableId, OpenMode.ForRead);
                if (tst.Has(styleName))
                    return tst[styleName];

                tst.UpgradeOpen();
                var tsr = new TextStyleTableRecord { Name = styleName };
                var id = tst.Add(tsr);
                tr.AddNewlyCreatedDBObject(tsr, true);
                tr.Commit();
                return id;
            }
        }

    }
}
