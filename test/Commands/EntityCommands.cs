using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using System.Collections.Generic;
using test.Parsers;
using test.Parsers.Base;
using test.Services;
using test.Utils;

namespace test.Commands
{
    public class EntityCommands
    {
        [CommandMethod("DRAWLINES")]
        public void DrawLines()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            CommandRunner.Run(
                doc.Editor,
                doc.Database,
                "(dạng: - (x;y),(x;y) - (x;y),len,ang -)",
                LineParser.ParseAll,
                DrawingService.DrawLines,
                "line"
            );
        }

        [CommandMethod("DRAWCIRCLES")]
        public void DrawCircles()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            CommandRunner.Run(
                doc.Editor,
                doc.Database,
                "(dạng: - (x;y),r - (x;y),r -)",
                CircleParser.ParseAll,
                DrawingService.DrawCircles,
                "circle"
            );
        }

        [CommandMethod("DRAWPOLYLINES")]
        public void DrawPolylines()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            CommandRunner.Run(
                doc.Editor,
                doc.Database,
                "(dạng: - (x1;y1),(x2;y2),A(bulge),(x3;y3),C - ... )",
                PolylineParser.ParseAll,
                DrawingService.DrawPolylines,
                "polyline"
            );
        }
        [CommandMethod("DRAWARCS")]
        public void DrawArcs()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            CommandRunner.Run(
                doc.Editor,
                doc.Database,
                "(dạng: - (x1;y1),(x2;y2),(x3;y3) - hoặc - (x;y),r,startAng,endAng -)",
                ArcParser.ParseAll,
                DrawingService.DrawArcs,
                "arc"
            );
        }
        [CommandMethod("DRAWLEADERS")]
        public void DrawLeaders()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            CommandRunner.Run(
                doc.Editor,
                doc.Database,
                "(dạng: - (x1;y1),(x2;y2),\"text\" - hoặc - (x1;y1),(x2;y2),(x3;y3),\"text\" -)",
                LeaderParser.ParseAll,
                DrawingService.DrawLeaders,
                "leader"
            );
        }
        [CommandMethod("DRAWTEXTS")]
        public void DrawTexts()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            CommandRunner.Run(
                doc.Editor,
                doc.Database,
                "(dạng: - (x1;y1),(x2;y2),\"text\"D - hoặc - (x1;y1),(x2;y2),\"text\"M - " +
                "hoặc - (x;y),rotation,length,\"text\"D/M -)",
                TextEntityParser.ParseAll,
                DrawingService.DrawTexts,
                "text"
            );
        }
    }
}
