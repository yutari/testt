using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using test.Converters;
using test.Parsers.Base;
using test.Reporting;
using test.Services;

namespace test.Utils
{
    /// <summary>
    /// CommandRunner cung cấp flow chuẩn: Prompt → Parse → Validate → Draw
    /// </summary>
    public static class CommandRunner
    {
        /// <summary>
        /// Chạy một command chung với parser & drawer được cung cấp.
        /// </summary>
        /// <typeparam name="T">Kiểu dữ liệu entity (LineData, CircleData, PolylineData, ...)</typeparam>
        /// <param name="ed">Editor để in ra thông báo</param>
        /// <param name="db">Database để vẽ</param>
        /// <param name="promptMessage">Message hiện khi yêu cầu input</param>
        /// <param name="parser">Hàm parser: string → ParseResult&lt;T&gt;</param>
        /// <param name="drawer">Hàm drawer: IEnumerable&lt;T&gt; → Database</param>
        /// <param name="entityName">Tên entity hiển thị (ví dụ: "line", "circle")</param>
        public static void Run<T>(
            Editor ed,
            Autodesk.AutoCAD.DatabaseServices.Database db,
            string promptMessage,
            Func<string, ParseResult<T>> parser,
            Action<IEnumerable<T>, Autodesk.AutoCAD.DatabaseServices.Database> drawer,
            string entityName)
        {
            var pr = new PromptStringOptions($"\nNhập chuỗi {entityName} {promptMessage}")
            { AllowSpaces = true };

            var res = ed.GetString(pr);
            if (res.Status != PromptStatus.OK) return;

            var parsed = parser(res.StringResult);

            // Nếu có lỗi → in ra và dừng
            if (parsed.HasError)
            {
                var errorBlock = BuildErrorBlock(parsed.InvalidHighlighted, entityName);
                ed.WriteMessage($"\n{errorBlock}");
                ed.WriteMessage($"\nHãy sửa lại các {entityName} bị lỗi rồi chạy lại lệnh.");
                return;
            }

            // Nếu hợp lệ → vẽ toàn bộ
            if (parsed.Valid.Count > 0)
            {
                drawer(parsed.Valid, db);
                ed.WriteMessage($"\n✔ Đã vẽ {parsed.Valid.Count} {entityName} hợp lệ.");
            }
            else
            {
                ed.WriteMessage($"\nℹ Không có {entityName} nào để vẽ.");
            }
        }

        private static string BuildErrorBlock(IReadOnlyList<string> invalids, string entityName)
        {
            if (invalids == null || invalids.Count == 0)
                return $"Không có {entityName} lỗi.";

            var sb = new StringBuilder();
            sb.Append($"Những {entityName} sau không hợp lệ:");
            foreach (var s in invalids)
            {
                sb.Append(" - ");
                sb.Append(s);
            }
            return sb.ToString();
        }
    }
    public static class SelectionRunner
    {
        public static void Run(Editor ed, Database db)
        {
            // Lấy input từ user
            var input = SelectionService.GetUserInput(ed);
            if (input == null)
            {
                ed.WriteMessage("\nSelection cancelled or invalid input.");
                return;
            }

            // Thực hiện select trong AutoCAD
            var entities = SelectionService.SelectEntities(ed, db, input);

            // Convert sang Data Model
            var selectedData = EntityConverter.Convert(entities);

            // Xuất JSON
            string path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    "selection_output.json"
                    );
            JsonReportExporter.Export(selectedData, "selection_output.json");

            ed.WriteMessage($"\nExported {selectedData.Count} entities to selection_output.json");
        }
    }
}
