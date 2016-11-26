using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Windows;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;


[assembly: CommandClass(typeof(AutoCADSamples.CreateEntities.CreareArc))]


namespace AutoCADSamples.CreateEntities
{
    class CreareArc
    {
        [CommandMethod("AutoCADSamples", "CreateArc", CommandFlags.Modal)]
        public void CreateArcCommand()
        {
            var document = Application.DocumentManager.MdiActiveDocument;
            var database = document.Database;

            try
            {
                using (var transaction = database.TransactionManager.StartTransaction())
                {
                    var blockTable = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);
                    var modelSpace = (BlockTableRecord)transaction.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                    var centerPt = new Point3d(0.0, 0.0, 0.0);
                    var radius = 50.0;

                    var arc = new Arc(centerPt, Vector3d.ZAxis, radius, 0.0, Math.PI / 2);

                    modelSpace.AppendEntity(arc);
                    transaction.AddNewlyCreatedDBObject(arc, true);

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
