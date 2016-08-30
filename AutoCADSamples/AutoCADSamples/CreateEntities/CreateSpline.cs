using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System.Windows;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

[assembly: CommandClass(typeof(AutoCADSamples.CreateEntities.CreateSpline))]


namespace AutoCADSamples.CreateEntities
{
    class CreateSpline
    {
        [CommandMethod("AutoCADSamples", "CreateSpline", CommandFlags.Modal)]
        public void CreateSplineCommand()
        {
            var document = Application.DocumentManager.MdiActiveDocument;
            var database = document.Database;

            try
            {
                using (var transaction = database.TransactionManager.StartTransaction())
                {
                    var blockTable = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);
                    var modelSpace = (BlockTableRecord)transaction.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                    var pointColl = new Point3dCollection
                    {
                        new Point3d(0, 0, 0),
                        new Point3d(50, 50, 0),
                        new Point3d(100, 0, 0),
                        new Point3d(150, 50, 0)
                };


                    var vecTan = new Point3d(0.5, 0.5, 0).GetAsVector();


                    using (var spline = new Spline(pointColl, KnotParameterizationEnum.Uniform, 3, 0))
                    {
                        modelSpace.AppendEntity(spline);
                        transaction.AddNewlyCreatedDBObject(spline, true);
                    }

                    transaction.Commit();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
