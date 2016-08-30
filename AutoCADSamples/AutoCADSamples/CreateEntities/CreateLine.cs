using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System.Windows;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

[assembly: CommandClass(typeof(AutoCADSamples.CreateEntities.CreateLine))]

namespace AutoCADSamples.CreateEntities
{
    class CreateLine
    {
        [CommandMethod("AutoCADSamples", "CreateLine", CommandFlags.Modal)]
        public void CreateLineCommand()
        {
            var document = Application.DocumentManager.MdiActiveDocument;
            var database = document.Database;
            var editor = document.Editor;

            try
            {
                using (var transaction = database.TransactionManager.StartTransaction())
                {
                    var blockTable = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);
                    var modelSpace =
                        (BlockTableRecord)
                            transaction.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                    var startPt = new Point3d(0.0, 0.0, 0.0);
                    var endPt = new Point3d(100.0, 100.0, 0.0);

                    var line = new Line(startPt, endPt);

                    modelSpace.AppendEntity(line);
                    transaction.AddNewlyCreatedDBObject(line, true);

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
