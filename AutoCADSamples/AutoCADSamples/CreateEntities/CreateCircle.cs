using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System.Windows;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

[assembly: CommandClass(typeof(AutoCADSamples.CreateEntities.CreateCircle))]

namespace AutoCADSamples.CreateEntities
{
    class CreateCircle
    {
        [CommandMethod("AutoCADSamples", "CreateCircle", CommandFlags.Modal)]
        public void CreateCircleCommand()
        {
            var document = Application.DocumentManager.MdiActiveDocument;
            var database = document.Database;

            try
            {
                using (var transaction = database.TransactionManager.StartTransaction())
                {
                    var blockTable = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);
                    var modelSpace =
                        (BlockTableRecord)
                            transaction.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                    var centerPt = new Point3d(0.0, 0.0, 0.0);
                    var radius = 50.0;

                    var circle = new Circle(centerPt, Vector3d.ZAxis, radius);

                    modelSpace.AppendEntity(circle);
                    transaction.AddNewlyCreatedDBObject(circle, true
                        );

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
