using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace test.Services
{
    public class StatNode
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public string ParentName { get; set; }
        public bool SumChildren { get; set; }
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

    public static class ReadingService
    {
        public static void CountModelSpace(BlockTableRecord ms, Transaction tr, StatNode root)
        {
            var brGroup = root.GetOrAddChild("BlockReference", sumChildren: true);

            foreach (ObjectId id in ms)
            {
                var ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                if (ent == null || ent.IsErased) continue;

                if (ent is BlockReference br)
                {
                    var name = GetBlockName(br, tr);
                    var blockNode = brGroup.GetOrAddChild(name);
                    blockNode.Count++;
                    if (blockNode.Children.Count == 0)
                    {
                        var defId = GetEffectiveBlockDefId(br);
                        var visited = new HashSet<ObjectId>();
                        BuildBlockComposition(defId, tr, blockNode, visited);
                    }
                }
                else
                {
                    root.GetOrAddChild(ent.GetType().Name).Count++;
                }
            }
        }

        private static void BuildBlockComposition(ObjectId blockDefId, Transaction tr, StatNode blockNode, HashSet<ObjectId> visited)
        {
            if (blockDefId.IsNull || visited.Contains(blockDefId)) return;
            visited.Add(blockDefId);

            var def = (BlockTableRecord)tr.GetObject(blockDefId, OpenMode.ForRead);
            if (def == null || def.IsErased || def.IsLayout) return;

            var innerBrGroup = blockNode.GetOrAddChild("BlockReference", sumChildren: true);

            foreach (ObjectId id in def)
            {
                var ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                if (ent == null || ent.IsErased) continue;

                if (ent is BlockReference brInner)
                {
                    var subName = GetBlockName(brInner, tr);
                    var subNode = innerBrGroup.GetOrAddChild(subName);
                    subNode.Count++;
                    var subDefId = GetEffectiveBlockDefId(brInner);
                    BuildBlockComposition(subDefId, tr, subNode, visited);
                }
                else
                {
                    blockNode.GetOrAddChild(ent.GetType().Name).Count++;
                }
            }
            innerBrGroup.Recalculate();
        }

        public static void AccumulateTotals(Entity ent, Transaction tr, Dictionary<string, int> totals)
        {
            if (ent is BlockReference br)
            {
                var defId = GetEffectiveBlockDefId(br);
                var def = (BlockTableRecord)tr.GetObject(defId, OpenMode.ForRead);

                foreach (ObjectId id in def)
                {
                    var child = tr.GetObject(id, OpenMode.ForRead) as Entity;
                    if (child != null && !child.IsErased)
                        AccumulateTotals(child, tr, totals);
                }
            }
            else
            {
                Inc(totals, ent.GetType().Name);
            }
        }

        private static void Inc(Dictionary<string, int> totals, string key)
        {
            if (!totals.ContainsKey(key)) totals[key] = 0;
            totals[key]++;
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
    }
}
