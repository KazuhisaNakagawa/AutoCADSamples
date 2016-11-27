using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Exception = System.Exception;

[assembly: CommandClass(typeof(AutoCADSamples.ModifyEntities.AddAttributeToBlockReference))]

namespace AutoCADSamples.ModifyEntities
{
    class AddAttributeToBlockReference
    {
        /// <summary>
        /// モデル空間にある TEST という名前のブロック参照に属性を追加
        /// </summary>
        [CommandMethod("AutoCADSamples", "AddAttributeToBlockReference", CommandFlags.Modal)]
        public void AddAttributeToBlockReferenceCommand()
        {
            var document = Application.DocumentManager.MdiActiveDocument;
            var database = document.Database;

            try
            {
                using (var transaction = database.TransactionManager.StartTransaction())
                {
                    // ブロック定義に属性定義を追加
                    var blockName = "TEST";
                    var blockTable = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);
                    var testBlock = (BlockTableRecord)transaction.GetObject(blockTable[blockName], OpenMode.ForRead);

                    var textStyleName = (string)Application.GetSystemVariable("TEXTSTYLE");
                    var textStyleTable = (TextStyleTable)transaction.GetObject(database.TextStyleTableId, OpenMode.ForRead);
                    var textStyleRec = (TextStyleTableRecord)transaction.GetObject(textStyleTable[textStyleName], OpenMode.ForRead);

                    var attDefPosition = new Point3d(10, 10, 0);
                    var attDef1 = new AttributeDefinition(attDefPosition, "100", "Price", "Enter price:", textStyleRec.ObjectId);

                    testBlock.UpgradeOpen();
                    testBlock.AppendEntity(attDef1);
                    transaction.AddNewlyCreatedDBObject(attDef1, true);


                    // モデル空間に配置されているブロック参照に属性を追加
                    var modelSpace = (BlockTableRecord)transaction.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                    foreach (var objectId in modelSpace)
                    {
                        var entity = (Entity)transaction.GetObject(objectId, OpenMode.ForRead);
                        if (entity is BlockReference && ((BlockReference)entity).Name == blockName)
                        {
                            var blockRef = (BlockReference)entity;
                            var tagName = "Price";

                            // すでに同じタグが存在するか確認
                            var hasTag = false;
                            var attColl = blockRef.AttributeCollection;
                            foreach (ObjectId id in attColl)
                            {
                                var attRef = (AttributeReference)transaction.GetObject(id, OpenMode.ForRead);
                                if (attRef.Tag == tagName)
                                    hasTag = true;
                            }
                            if (hasTag)
                                continue;

                            // 属性を追加
                            var mat3d = Matrix3d.Displacement(new Vector3d(attDefPosition.X, attDefPosition.Y, attDefPosition.Z));
                            var attRefPotision = blockRef.Position.TransformBy(mat3d);

                            var attRef1 = new AttributeReference(attRefPotision, "999", tagName, textStyleRec.ObjectId);

                            blockRef.UpgradeOpen();
                            blockRef.AttributeCollection.AppendAttribute(attRef1);
                            transaction.AddNewlyCreatedDBObject(attRef1, true);
                        }
                    }


                    transaction.Commit();
                }

            }
            catch (Exception ex)
            {
                Application.ShowAlertDialog(ex.Message);
            }

        }
    }
}
