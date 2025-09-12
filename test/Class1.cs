using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CadStats
{
    // Node thống kê với chế độ cộng dồn từ con (SumChildren)
    public class StatNode
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public string ParentName { get; set; }
        public bool SumChildren { get; set; } // true: Count = sum(Children.Count)
        public List<StatNode> Children { get; } = new List<StatNode>();

        public StatNode(string name, bool sumChildren = false)
        {
            Name = name;
            SumChildren = sumChildren;
        }

        public StatNode GetOrAddChild(string name, bool sumChildren = false)
        {
            var c = Children.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (c == null)
            {
                c = new StatNode(name, sumChildren);
                Children.Add(c);
            }
            return c;
        }

        public void Recalculate()
        {
            foreach (var c in Children) c.Recalculate();
            if (SumChildren)
                Count = Children.Sum(c => c.Count);
        }
    }

    public class EntityCounterTreeFormatted
    {
        [CommandMethod("COUNT_ENTITIES_V1")]
        public void CountEntitiesTree()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var ed = doc.Editor;

            var totals = new Dictionary<string, int>();
            // Root cho in ra: ModelSpace
            var root = new StatNode("ModelSpace", sumChildren: false);

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                // Đếm thực thể trực tiếp trong ModelSpace
                CountModelSpace(ms, tr, root);

                // Tính lại tổng cho các node cha cần cộng dồn (Text, Dimension, BlockReference nhóm)
                root.Recalculate();

                tr.Commit();
            }

            //Tính tổng thực thể (có nhân số lần block) để đối chiếu
            using (var tr2 = db.TransactionManager.StartTransaction())
            {
                var bt = (BlockTable)tr2.GetObject(db.BlockTableId, OpenMode.ForRead);
                var ms = (BlockTableRecord)tr2.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                foreach (ObjectId id in ms)
                {
                    var ent = tr2.GetObject(id, OpenMode.ForRead) as Entity;
                    if (ent != null && !ent.IsErased)
                        AccumulateTotals(ent, tr2, totals);
                }
                tr2.Commit();
            }
            // In theo đúng format yêu cầu
            ed.WriteMessage("\n--------------------------------");
            ed.WriteMessage("\nBản vẽ có những thành phần sau:");
            PrintModelSpace(ed, root);
            ed.WriteMessage("\nTổng số các thành phần chi tiết:");
            foreach (var kv in totals.OrderBy(k => k.Key))
                ed.WriteMessage($"\n- {kv.Key}: {kv.Value}");
            ed.WriteMessage("\n--------------------------------\n");
            ed.WriteMessage("\n--------------------------------\n");
        }

        // Đếm ở ModelSpace: gom nhóm Text/Dimension theo con chi tiết, BlockReference theo tên block (đếm số lần chèn)
        private void CountModelSpace(BlockTableRecord ms, Transaction tr, StatNode root)
        {
            // Các nhóm cha cần cộng dồn từ con
            var textGroup = root.GetOrAddChild("Text", sumChildren: true);
            var dimGroup = root.GetOrAddChild("Dimension", sumChildren: true);
            var brGroup = root.GetOrAddChild("BlockReference", sumChildren: true);

            foreach (ObjectId id in ms)
            {
                var ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                if (ent == null || ent.IsErased) continue;

                // ---- Text
                if (ent is DBText)
                    textGroup.GetOrAddChild("DBText").Count++;
                else if (ent is MText)
                    textGroup.GetOrAddChild("MText").Count++;

                // ---- Dimension (chi tiết)
                else if (ent is Dimension dim)
                {
                    var sub = GetDimSubtype(dim);
                    dimGroup.GetOrAddChild(sub).Count++;
                }

                // ---- BlockReference (chỉ đếm instance theo tên)
                else if (ent is BlockReference br)
                {
                    var name = GetBlockName(br, tr);
                    var blockNode = brGroup.GetOrAddChild(name, sumChildren: false);
                    blockNode.Count++; // số lần chèn

                    // Xây "thành phần block" theo định nghĩa (per definition), làm 1 lần/định nghĩa
                    if (blockNode.Children.Count == 0)
                    {
                        var defId = GetEffectiveBlockDefId(br);
                        var visited = new HashSet<ObjectId>(); // chống đệ quy vòng
                        BuildBlockComposition(defId, tr, blockNode, visited);
                    }
                }

                // ---- Các loại entity phẳng khác (nằm trực tiếp trong ModelSpace)
                else if (ent is Polyline || ent is Polyline2d || ent is Polyline3d)
                    root.GetOrAddChild("Polyline").Count++;
                else if (ent is Circle)
                    root.GetOrAddChild("Circle").Count++;
                else if (ent is Arc)
                    root.GetOrAddChild("Arc").Count++;
                else if (ent is Spline)
                    root.GetOrAddChild("Spline").Count++;
                else if (ent is Hatch)
                    root.GetOrAddChild("Hatch").Count++;
                else
                {
                    // fallback cho loại ít gặp: đếm thẳng ở ModelSpace
                    root.GetOrAddChild(ent.GetType().Name).Count++;
                }
            }
        }

        // Duyệt định nghĩa block (per definition) để xây "thành phần" hiển thị dưới block name
        private void BuildBlockComposition(ObjectId blockDefId, Transaction tr, StatNode blockNode, HashSet<ObjectId> visited)
        {
            if (blockDefId.IsNull) return;
            if (visited.Contains(blockDefId)) return;
            visited.Add(blockDefId);

            var def = (BlockTableRecord)tr.GetObject(blockDefId, OpenMode.ForRead);
            if (def == null || def.IsErased) return;
            if (def.IsLayout) return; // bỏ qua model/paper layout records

            // Nhóm "BlockReference" bên trong định nghĩa (để liệt kê block lồng)
            var innerBrGroup = blockNode.GetOrAddChild("BlockReference", sumChildren: true);

            foreach (ObjectId id in def)
            {
                var ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                if (ent == null || ent.IsErased) continue;

                // Thành phần bên trong block: liệt kê "thô" bằng dấu '-'
                if (ent is DBText)
                    blockNode.GetOrAddChild("DBText").Count++;
                else if (ent is MText)
                    blockNode.GetOrAddChild("MText").Count++;
                else if (ent is Polyline || ent is Polyline2d || ent is Polyline3d)
                    blockNode.GetOrAddChild("Polyline").Count++;
                else if (ent is Circle)
                    blockNode.GetOrAddChild("Circle").Count++;
                else if (ent is Arc)
                    blockNode.GetOrAddChild("Arc").Count++;
                else if (ent is Hatch)
                    blockNode.GetOrAddChild("Hatch").Count++;
                else if (ent is Dimension d)
                    blockNode.GetOrAddChild(GetDimSubtype(d)).Count++; // có thể gặp kích thước trong block
                else if (ent is BlockReference brInner)
                {
                    // Liệt kê block lồng (số lần dùng trong định nghĩa)
                    var subName = GetBlockName(brInner, tr);
                    var subNode = innerBrGroup.GetOrAddChild(subName, sumChildren: false);
                    subNode.Count++;

                    // Thành phần của sub-block (per definition)
                    var subDefId = GetEffectiveBlockDefId(brInner);
                    BuildBlockComposition(subDefId, tr, subNode, visited);
                }
                else
                {
                    // fallback
                    blockNode.GetOrAddChild(ent.GetType().Name).Count++;
                }
            }

            // Cập nhật tổng cho nhóm BlockReference bên trong định nghĩa
            innerBrGroup.Recalculate();
        }

        private static string GetDimSubtype(Dimension dim)
        {
            if (dim is RotatedDimension) return "RotatedDimension";
            if (dim is AlignedDimension) return "AlignedDimension";
            if (dim is RadialDimension) return "RadialDimension";
            if (dim is DiametricDimension) return "DiametricDimension";
            if (dim is ArcDimension) return "ArcDimension";
            if (dim is OrdinateDimension) return "OrdinateDimension";
            return dim.GetType().Name;
        }

        private static string GetBlockName(BlockReference br, Transaction tr)
        {
            try
            {
                var recId = !br.DynamicBlockTableRecord.IsNull ? br.DynamicBlockTableRecord : br.BlockTableRecord;
                var btr = (BlockTableRecord)tr.GetObject(recId, OpenMode.ForRead);
                return btr?.Name ?? "*Anonymous";
            }
            catch { return "*Anonymous"; }
        }

        private static ObjectId GetEffectiveBlockDefId(BlockReference br)
        {
            return !br.DynamicBlockTableRecord.IsNull ? br.DynamicBlockTableRecord : br.BlockTableRecord;
        }

        // In đúng format yêu cầu
        private void PrintModelSpace(Editor ed, StatNode root)
        {
            // In dòng gốc
            ed.WriteMessage("\n- ModelSpace:");

            // In tất cả con trong ModelSpace
            foreach (var node in root.Children.OrderBy(c => c.Name))
            {
                PrintNode(ed, node, "  "); // bắt đầu indent = 2 spaces
            }
        }

        private void PrintNode(Editor ed, StatNode node, string indent)
        {
            // Xác định prefix
            string prefix;
            if (node.ParentName == "BlockReference")
                prefix = "+";              // block name dưới BlockReference
            else if (node.Name == "BlockReference")
                prefix = "-";              // nhóm BlockReference
            else if (node.ParentName == "Text" || node.ParentName == "Dimension")
                prefix = "+";              // DBText, MText, RotatedDimension...
            else
                prefix = "-";              // entity thường

            ed.WriteMessage($"\n{indent}{prefix} {node.Name}: {node.Count}");

            // In đệ quy các con
            foreach (var child in node.Children.OrderBy(c => c.Name))
            {
                child.ParentName = node.Name; // cập nhật cha để in đúng ký hiệu
                PrintNode(ed, child, indent + "  ");
            }
        }


        // Dùng để lưu tổng các entity thực tế (có nhân số lần block)
        private void AccumulateTotals(
            Entity ent, Transaction tr,
            Dictionary<string, int> totals)
        {
            if (ent is DBText) Inc(totals, "DBText");
            else if (ent is MText) Inc(totals, "MText");
            else if (ent is Polyline || ent is Polyline2d || ent is Polyline3d) Inc(totals, "Polyline");
            else if (ent is Circle) Inc(totals, "Circle");
            else if (ent is Arc) Inc(totals, "Arc");
            else if (ent is Hatch) Inc(totals, "Hatch");
            else if (ent is Dimension d) Inc(totals, GetDimSubtype(d));
            else if (ent is BlockReference br)
            {
                // Đếm chính block reference
                Inc(totals, "BlockReference");

                // Lặp số lần chèn (ở đây = 1 vì ent là 1 instance)
                var defId = GetEffectiveBlockDefId(br);
                var def = (BlockTableRecord)tr.GetObject(defId, OpenMode.ForRead);

                foreach (ObjectId id in def)
                {
                    var child = tr.GetObject(id, OpenMode.ForRead) as Entity;
                    if (child != null && !child.IsErased)
                    {
                        // Nhân số lần chèn: gọi đệ quy 1 lần cho mỗi lần chèn
                        AccumulateTotals(child, tr, totals);
                    }
                }
            }
            else
            {
                Inc(totals, ent.GetType().Name);
            }
        }

        private void Inc(Dictionary<string, int> totals, string key)
        {
            if (!totals.ContainsKey(key)) totals[key] = 0;
            totals[key]++;
        }

        private void PrintBlockComposition(Editor ed, StatNode blockNameNode, int indent)
        {
            // Liệt kê mọi thành phần trực tiếp của blockNameNode bằng '-'
            // (DBText, Polyline, Circle, Hatch, Dimension subtype, ...), và cả BlockReference lồng
            var spaces = new string(' ', indent);

            // In trước các loại khác BlockReference
            foreach (var c in blockNameNode.Children.Where(x => !x.Name.Equals("BlockReference", StringComparison.OrdinalIgnoreCase))
                                                    .OrderBy(x => x.Name))
            {
                ed.WriteMessage($"\n{spaces}- {c.Name}: {c.Count}");
            }

            // Nếu có block lồng: "- BlockReference: K" rồi liệt kê "+ SubBlock: m"
            var innerBr = blockNameNode.Children.FirstOrDefault(x => x.Name.Equals("BlockReference", StringComparison.OrdinalIgnoreCase));
            if (innerBr != null && innerBr.Children.Count > 0)
            {
                ed.WriteMessage($"\n{spaces}- BlockReference: {innerBr.Count}");
                foreach (var sub in innerBr.Children.OrderBy(x => x.Name))
                {
                    ed.WriteMessage($"\n{spaces}  + {sub.Name}: {sub.Count}");
                    // Và tiếp tục liệt kê thành phần của sub-block (đệ quy)
                    PrintBlockComposition(ed, sub, indent + 4);
                }
            }
        }
    }
}

