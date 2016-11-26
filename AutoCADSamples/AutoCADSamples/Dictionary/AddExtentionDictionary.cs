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

                        AddtExtentionDictionaryData(transaction, entity, count);

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
        /// <param name="entity"></param>
        /// <param name="count">拡張ディクショナリに設定する値</param>
        private static void AddtExtentionDictionaryData(Transaction transaction, DBObject entity, int count)
        {
            var extentionId = entity.ExtensionDictionary;

            if (extentionId == ObjectId.Null)
            {
                entity.UpgradeOpen();
                entity.CreateExtensionDictionary();
                extentionId = entity.ExtensionDictionary;
            }

            var extentionDict = (DBDictionary)transaction.GetObject(extentionId, OpenMode.ForRead);

            if (extentionDict.Contains("TEST"))
                return;

            extentionDict.UpgradeOpen();
            var xRec = new Xrecord();
            var resultBuffer = new ResultBuffer();
            resultBuffer.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, "Data"));
            resultBuffer.Add(new TypedValue((int)DxfCode.ExtendedDataInteger32, count));

            xRec.Data = resultBuffer;

            extentionDict.SetAt("TEST", xRec);
            transaction.AddNewlyCreatedDBObject(xRec, true);
        }
    }
}
