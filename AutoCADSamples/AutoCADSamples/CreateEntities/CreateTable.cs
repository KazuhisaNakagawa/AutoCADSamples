using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Exception = System.Exception;

[assembly: CommandClass(typeof(AutoCADSamples.CreateEntities.CreateTable))]

namespace AutoCADSamples.CreateEntities
{
    class CreateTable
    {
        [CommandMethod("AutoCADSample", "CreateTable", CommandFlags.Modal)]
        public void CreateTableCommand()
        {
            try
            {
                var document = Application.DocumentManager.MdiActiveDocument;
                var database = document.Database;
                var editor = document.Editor;

                Point3d tableInsertPoint;
                if (!GetTableInsertPoint(editor, out tableInsertPoint))
                    return;

                var table = SetTableStyle(database, tableInsertPoint);

                var str = SetTableData();

                SetTableValues(table, str);

                AddTableToDatabase(database, table);
            }
            catch (Exception ex)
            {
                Application.ShowAlertDialog(ex.Message);
            }
        }

        private static bool GetTableInsertPoint(Editor editor, out Point3d tableInsertPoint)
        {
            tableInsertPoint = new Point3d(0, 0, 0);

            var pointOptions = new PromptPointOptions("\n表の挿入点を指定");
            pointOptions.AllowNone = false;
            var pointResult = editor.GetPoint(pointOptions);
            if (pointResult.Status != PromptStatus.OK)
                return false;

            tableInsertPoint = pointResult.Value;
            return true;
        }

        private static Table SetTableStyle(Database database, Point3d tableInsertPoint)
        {
            var table = new Table();
            table.TableStyle = database.Tablestyle;
            table.SetSize(5, 3);
            table.SetRowHeight(3);

            // 最初の行は結合されているのでそれを解除する
            var row1 = table.Rows[0];
            if (row1.IsMerged.HasValue && row1.IsMerged.Value)
                table.UnmergeCells(row1);

            for (var i = 0; i < 3; i++)
            {
                table.Columns[i].Width = 15;
            }
            table.Position = tableInsertPoint;
            return table;
        }

        private static string[,] SetTableData()
        {
            var str = new string[5, 3];
            str[0, 0] = "Part No.";
            str[0, 1] = "Name ";
            str[0, 2] = "Material ";
            str[1, 0] = "1876-1";
            str[1, 1] = "Flange";
            str[1, 2] = "Perspex";
            str[2, 0] = "0985-4";
            str[2, 1] = "Bolt";
            str[2, 2] = "Steel";
            str[3, 0] = "3476-K";
            str[3, 1] = "Tile";
            str[3, 2] = "Ceramic";
            str[4, 0] = "8734-3";
            str[4, 1] = "Kean";
            str[4, 2] = "Mostly water";
            return str;
        }

        private static void SetTableValues(Table table, string[,] str)
        {
            for (var i = 0; i < 5; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    table.Cells[i, j].TextHeight = 1;
                    table.Cells[i, j].TextString = str[i, j];
                    table.Cells[i, j].Alignment = CellAlignment.MiddleCenter;
                }
            }
        }

        private static void AddTableToDatabase(Database database, Table table)
        {
            using (var transaction = database.TransactionManager.StartTransaction())
            {
                var blockTable = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);
                var modelSpace =
                    (BlockTableRecord)transaction.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                modelSpace.AppendEntity(table);
                transaction.AddNewlyCreatedDBObject(table, true);

                transaction.Commit();
            }
        }
    }
}
