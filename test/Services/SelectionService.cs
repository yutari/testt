using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using test.Data;

namespace test.Services
{
    public static class SelectionService
    {
        public static SelectionInput GetUserInput(Editor ed)
        {
            var input = new SelectionInput();

            // 1. Hỏi entity types
            var entRes = ed.GetString("\nEnter entity types (-Line-Circle-...): ");
            if (entRes.Status != PromptStatus.OK) return null;
            input.EntityTypes.AddRange(
                entRes.StringResult.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries)
            );

            // 2. Hỏi filter types
            var filterRes = ed.GetString("\nEnter filters (-Layer-Colour-Block-... or empty): ");
            if (filterRes.Status != PromptStatus.OK) return null;
            input.FilterTypes.AddRange(
                filterRes.StringResult.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries)
            );

            // 3. Với mỗi filter type, hỏi tiếp value
            foreach (var filter in input.FilterTypes)
            {
                switch (filter.Trim().ToLower())
                {
                    case "layer":
                        {
                            var val = ed.GetString("\nEnter layer names (-Layer1-Layer2-...): ");
                            if (val.Status != PromptStatus.OK) return null;
                            input.FilterValues["Layer"] = new List<string>(
                                val.StringResult.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries)
                            );
                        }
                        break;

                    case "colour":
                    case "color":
                        {
                            var val = ed.GetString("\nEnter colours (-Red-Blue-...): ");
                            if (val.Status != PromptStatus.OK) return null;
                            input.FilterValues["Colour"] = new List<string>(
                                val.StringResult.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries)
                            );
                        }
                        break;

                    case "block":
                        {
                            var val = ed.GetString("\nEnter block names (-Bolt-M12-...): ");
                            if (val.Status != PromptStatus.OK) return null;
                            input.FilterValues["Block"] = new List<string>(
                                val.StringResult.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries)
                            );
                        }
                        break;

                    default:
                        ed.WriteMessage($"\nFilter type '{filter}' not recognized.");
                        break;
                }
            }

            // 4. Hỏi scope
            var scopeRes = ed.GetInteger("\nScope [1=All/2=Block/3=Window/4=Pick]: ");
            if (scopeRes.Status != PromptStatus.OK) return null;
            input.Scope = scopeRes.Value;

            return input;
        }

        public static List<Entity> SelectEntities(Editor ed, Database db, SelectionInput input)
        {
            var result = new List<Entity>();

            // Build SelectionFilter từ input
            var filter = BuildSelectionFilter(input);

            using (var tr = db.TransactionManager.StartTransaction())
            {
                switch (input.Scope)
                {
                    case 1: // ALL
                        {
                            var selRes = filter == null ? ed.SelectAll() : ed.SelectAll(filter);
                            if (selRes.Status == PromptStatus.OK)
                            {
                                foreach (ObjectId id in selRes.Value.GetObjectIds())
                                {
                                    var ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                                    if (ent != null) result.Add(ent);
                                }
                            }
                        }
                        break;

                    case 2: // BLOCK
                        {
                            // yêu cầu block name
                            var blockNameRes = ed.GetString("\nEnter block name: ");
                            if (blockNameRes.Status != PromptStatus.OK) break;
                            var blockName = blockNameRes.StringResult;

                            // thêm block name vào filter
                            var blockFilter = BuildSelectionFilter(input, blockName);

                            var selRes = blockFilter == null ? ed.SelectAll() : ed.SelectAll(blockFilter);
                            if (selRes.Status == PromptStatus.OK)
                            {
                                foreach (ObjectId id in selRes.Value.GetObjectIds())
                                {
                                    var ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                                    if (ent != null) result.Add(ent);
                                }
                            }
                        }
                        break;

                    case 3: // WINDOW
                        {
                            var p1 = ed.GetPoint("\nSpecify first corner: ");
                            if (p1.Status != PromptStatus.OK) break;

                            var p2 = ed.GetCorner("\nSpecify opposite corner: ", p1.Value);
                            if (p2.Status != PromptStatus.OK) break;

                            var selRes = filter == null
                                ? ed.SelectWindow(p1.Value, p2.Value)
                                : ed.SelectWindow(p1.Value, p2.Value, filter);

                            if (selRes.Status == PromptStatus.OK)
                            {
                                foreach (ObjectId id in selRes.Value.GetObjectIds())
                                {
                                    var ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                                    if (ent != null) result.Add(ent);
                                }
                            }
                        }
                        break;
                    default:
                        ed.WriteMessage("\nInvalid scope value.");
                        break;
                    }

                    tr.Commit();
                }

            return result;
         
        }

        // ===========================
        // Build SelectionFilter từ input
        // ===========================
        private static SelectionFilter BuildSelectionFilter(SelectionInput input, string blockName = null)
        {
            var values = new List<TypedValue>();

            // EntityTypes
            if (input.EntityTypes.Count > 0)
            {
                if (input.EntityTypes.Count == 1)
                {
                    values.Add(new TypedValue((int)DxfCode.Start, input.EntityTypes[0].ToUpper()));
                }
                else
                {
                    values.Add(new TypedValue((int)DxfCode.Operator, "<OR"));
                    foreach (var entType in input.EntityTypes)
                        values.Add(new TypedValue((int)DxfCode.Start, entType.ToUpper()));
                    values.Add(new TypedValue((int)DxfCode.Operator, "OR>"));
                }
            }

            // Layer filter
            if (input.FilterValues.ContainsKey("Layer"))
            {
                var layers = input.FilterValues["Layer"];
                if (layers.Count == 1)
                {
                    values.Add(new TypedValue((int)DxfCode.LayerName, layers[0]));
                }
                else if (layers.Count > 1)
                {
                    values.Add(new TypedValue((int)DxfCode.Operator, "<OR"));
                    foreach (var l in layers)
                        values.Add(new TypedValue((int)DxfCode.LayerName, l));
                    values.Add(new TypedValue((int)DxfCode.Operator, "OR>"));
                }
            }

            // Colour filter
            if (input.FilterValues.ContainsKey("Colour"))
            {
                var colors = input.FilterValues["Colour"];
                if (colors.Count == 1)
                {
                    values.Add(new TypedValue((int)DxfCode.ColorName, colors[0]));
                }
                else if (colors.Count > 1)
                {
                    values.Add(new TypedValue((int)DxfCode.Operator, "<OR"));
                    foreach (var c in colors)
                        values.Add(new TypedValue((int)DxfCode.ColorName, c));
                    values.Add(new TypedValue((int)DxfCode.Operator, "OR>"));
                }
            }

            // Block filter
            if (!string.IsNullOrEmpty(blockName))
            {
                values.Add(new TypedValue((int)DxfCode.BlockName, blockName));
            }
            else if (input.FilterValues.ContainsKey("Block"))
            {
                var blocks = input.FilterValues["Block"];
                if (blocks.Count == 1)
                {
                    values.Add(new TypedValue((int)DxfCode.BlockName, blocks[0]));
                }
                else if (blocks.Count > 1)
                {
                    values.Add(new TypedValue((int)DxfCode.Operator, "<OR"));
                    foreach (var b in blocks)
                        values.Add(new TypedValue((int)DxfCode.BlockName, b));
                    values.Add(new TypedValue((int)DxfCode.Operator, "OR>"));
                }
            }

            return values.Count > 0 ? new SelectionFilter(values.ToArray()) : null;
        }
    }
}
