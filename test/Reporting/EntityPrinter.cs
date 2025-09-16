using Autodesk.AutoCAD.EditorInput;
using System.Linq;
using test.Services;

namespace test.Reporting
{
    public static class EntityPrinter
    {
        public static void Print(Editor ed, StatNode root)
        {
            ed.WriteMessage($"\n- {root.Name}: {root.Count}");

            foreach (var node in root.Children.OrderBy(c => c.Name))
                PrintNode(ed, node, "  ");
        }

        private static void PrintNode(Editor ed, StatNode node, string indent)
        {
            string prefix = node.ParentName == "BlockReference" ? "+" : "-";

            ed.WriteMessage($"\n{indent}{prefix} {node.Name}: {node.Count}");

            foreach (var child in node.Children.OrderBy(c => c.Name))
            {
                child.ParentName = node.Name;
                PrintNode(ed, child, indent + "  ");
            }
        }
    }
}
