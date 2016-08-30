using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System.Windows;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = System.Exception;

[assembly: CommandClass(typeof(AutoCADSamples.CreateEntities.CreatePolyLine))]

namespace AutoCADSamples.CreateEntities
{
    class CreatePolyLine
    {
        [CommandMethod("AutoCADSamples", "CreatePolyLine", CommandFlags.Modal)]
        public void CreatePolyLineCommand()
        {
            var document = Application.DocumentManager.MdiActiveDocument;
            var database = document.Database;

            try
            {
                using (var transaction = database.TransactionManager.StartTransaction())
                {
                    var blockTable = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);
                    var modelSpace = (BlockTableRecord)transaction.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                    var polyLine = new Polyline();
                    polyLine.AddVertexAt(0, new Point2d(0.0, 0.0), 0.0, 0.0, 0.0);
                    polyLine.AddVertexAt(1, new Point2d(100.0, 0.0), 0.0, 0.0, 0.0);
                    polyLine.AddVertexAt(2, new Point2d(100.0, 100.0), 0.0, 0.0, 0.0);
                    polyLine.AddVertexAt(3, new Point2d(0.0, 100.0), 0.0, 0.0, 0.0);
                    polyLine.Closed = true;

                    modelSpace.AppendEntity(polyLine);
                    transaction.AddNewlyCreatedDBObject(polyLine, true);

                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
