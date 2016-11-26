using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Exception = System.Exception;

[assembly: CommandClass(typeof(AutoCADSamples.Dictionary.AddExtentionDictionary))]

namespace AutoCADSamples.Dictionary
{
    class AddExtentionDictionary
    {
        /// <summary>
        /// モデル空間にあるすべての図形に拡張ディクショナリを追加するサンプル
        /// </summary>
        [CommandMethod("AutoCADSamples", "AddExtentionDictionary", CommandFlags.Modal)]
        public void AddExtentionDictionaryCommand()
        {
            var document = Application.DocumentManager.MdiActiveDocument;
            var database = document.Database;

            try
            {
                using (var transaction = database.TransactionManager.StartTransaction())
                {
                    var blockTable = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);
                    var modelSpace = (BlockTableRecord)transaction.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                    var count = 1;
                    foreach (ObjectId objectId in modelSpace)
                    {
                        var entity = (Entity)transaction.GetObject(objectId, OpenMode.ForRead);
                        var extentionId = entity.ExtensionDictionary;

                        if (extentionId == ObjectId.Null)
                        {
                            entity.UpgradeOpen();
                            entity.CreateExtensionDictionary();
                            extentionId = entity.ExtensionDictionary;
                        }

                        SetExtentionDictionaryData(transaction, extentionId, count);

                        count += 1;
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
        /// 拡張ディクショナリのデータを設定
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="extentionId"></param>
        /// <param name="count"></param>
        private static void SetExtentionDictionaryData(Transaction transaction, ObjectId extentionId, int count)
        {
            var extentionDict = (DBDictionary) transaction.GetObject(extentionId, OpenMode.ForRead);

            if (!extentionDict.Contains("TEST"))
            {
                extentionDict.UpgradeOpen();
                var xRec = new Xrecord();
                var resultBuffer = new ResultBuffer();
                resultBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataAsciiString, "Data"));
                resultBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataInteger32, count));

                xRec.Data = resultBuffer;

                extentionDict.SetAt("TEST", xRec);
                transaction.AddNewlyCreatedDBObject(xRec, true);
            }
        }
    }
}
