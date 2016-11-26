using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Exception = System.Exception;

[assembly: CommandClass(typeof(AutoCADSamples.Dictionary.ModifyExtentionDictionary))]

namespace AutoCADSamples.Dictionary
{
    class ModifyExtentionDictionary
    {
        /// <summary>
        /// モデル空間にある図形に拡張データがあったらその内容を変更するサンプル
        /// ※ AddExtentionDictionary コマンドで追加した拡張データが対象
        /// </summary>
        [CommandMethod("AutoCADSamples", "ModifyExtentionDictionary", CommandFlags.Modal)]
        public void ModifyExtentionDictionaryCommand()
        {
            var document = Application.DocumentManager.MdiActiveDocument;
            var database = document.Database;

            try
            {
                using (var transaction = database.TransactionManager.StartTransaction())
                {
                    var blockTable = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);
                    var modelSpace = (BlockTableRecord)transaction.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                    foreach (ObjectId objectId in modelSpace)
                    {
                        var entity = (Entity)transaction.GetObject(objectId, OpenMode.ForRead);
                        var extentionId = entity.ExtensionDictionary;

                        if (extentionId == ObjectId.Null)
                            continue;

                        ChengeData(transaction, extentionId);
                    }

                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                Application.ShowAlertDialog(ex.Message);
            }

        }

        /// <summary>
        /// 拡張ディクショナリの 1071 の値に 1 足す
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="extentionId"></param>
        private static void ChengeData(Transaction transaction, ObjectId extentionId)
        {
            var extentionDict = (DBDictionary)transaction.GetObject(extentionId, OpenMode.ForWrite);
            var xRecId = extentionDict.GetAt("TEST");
            var xRec = (Xrecord)transaction.GetObject(xRecId, OpenMode.ForRead);
            var resultBuffer = xRec.Data;
            var data = resultBuffer.AsArray();

            for (int i = 0; i < data.Length; i++)
            {
                var value = data[i];
                if (value.TypeCode == 1071)
                {
                    data[i] = new TypedValue(1071, (int)value.Value + 1);
                }
            }

            extentionDict.Remove(xRecId);
            var newXrec = new Xrecord { Data = new ResultBuffer(data) };
            extentionDict.SetAt("TEST", newXrec);
            transaction.AddNewlyCreatedDBObject(newXrec, true);
        }
    }
}
