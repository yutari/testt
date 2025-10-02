using System;
using System.Linq;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Xunit;

[assembly: CommandClass(typeof(test.Tests.TestRunner))]

namespace test.Tests
{
    public class TestRunner
    {
        [CommandMethod("RUN_GEOMETRY_TESTS")]
        public void RunGeometryTests()
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage("\n=== Running Geometry Tests ===");

            int passed = 0;
            int failed = 0;

            try
            {
                // Lấy assembly hiện tại (project test)
                var asm = Assembly.GetExecutingAssembly();

                // Lấy tất cả class trong assembly có Fact
                var testClasses = asm.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract &&
                        t.GetMethods().Any(m => m.GetCustomAttributes(typeof(FactAttribute), false).Any()))
                    .ToList();


                foreach (var cls in testClasses)
                {
                    object instance = null;
                    try
                    {
                        instance = Activator.CreateInstance(cls);
                    }
                    catch (System.Exception ex)
                    {
                        ed.WriteMessage($"\n❌ Cannot create {cls.Name}: {ex.Message}");
                        continue;
                    }

                    // Lấy tất cả method có [Fact]
                    var factMethods = cls.GetMethods()
                        .Where(m => m.GetCustomAttributes(typeof(FactAttribute), false).Any());

                    foreach (var method in factMethods)
                    {
                        try
                        {
                            method.Invoke(instance, null);
                            ed.WriteMessage($"\n✔ {cls.Name}.{method.Name} PASSED");
                            passed++;
                        }
                        catch (System.Exception ex)
                        {
                            var msg = ex.InnerException?.Message ?? ex.Message;
                            ed.WriteMessage($"\n❌ {cls.Name}.{method.Name} FAILED: {msg}");
                            failed++;
                        }
                    }

                    // Dispose nếu class implement IDisposable
                    (instance as IDisposable)?.Dispose();
                }

                ed.WriteMessage($"\n=== Tests finished: {passed} passed, {failed} failed ===");
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage($"\n❌ Runner crashed: {ex.Message}");
            }
        }
    }
}
