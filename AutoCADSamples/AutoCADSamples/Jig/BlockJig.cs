using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System.Windows;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

[assembly: CommandClass(typeof(AutoCADSamples.Jig.BlockJig))]


namespace AutoCADSamples.Jig
{
    class BlockJig
    {
        /// <summary>
        /// 図面中の TEST という名前のブロックを Jig でドラッグし、クリックした位置に配置するコマンド
        /// </summary>
        [CommandMethod("AutoCADSamples", "BlockJig", CommandFlags.Modal)]
        public void BlockJigCommand()
        {
            var blockName = "TEST";

            var document = Application.DocumentManager.MdiActiveDocument;
            var database = document.Database;

            try
            {
                using (var transaction = database.TransactionManager.StartTransaction())
                {
                    var blockTable = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);
                    var block = (BlockTableRecord)transaction.GetObject(blockTable[blockName], OpenMode.ForRead);

                    // Jig を作成し、ユーザーに挿入位置を指定させる
                    var myBlockJig = new MyBlockJig();
                    Point3d point;
                    var res = myBlockJig.DragMe(block.ObjectId, out point);

                    if (res.Status == PromptStatus.OK)
                    {
                        // ユーザーがクリックした場所にブロックを配置する
                        var curSpace = (BlockTableRecord)transaction.GetObject(database.CurrentSpaceId, OpenMode.ForWrite);
                        var insert = new BlockReference(point, block.ObjectId);

                        curSpace.AppendEntity(insert);
                        transaction.AddNewlyCreatedDBObject(insert, true);
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

    public class MyBlockJig : DrawJig
    {
        public Point3d Point;
        private ObjectId _blockId = ObjectId.Null;

        /// <summary>
        /// ユーザがクリックするまでドラッグしているブロックを表示する
        /// </summary>
        /// <param name="iBlockId">ブロック定義のオブジェクトID</param>
        /// <param name="oClickedPoint">クリックされた点</param>
        /// <returns></returns>
        public PromptResult DragMe(ObjectId iBlockId, out Point3d oClickedPoint)
        {
            _blockId = iBlockId;
            var document = Application.DocumentManager.MdiActiveDocument;
            var editor = document.Editor;

            var jigRes = editor.Drag(this);
            oClickedPoint = Point;

            return jigRes;
        }

        /// <summary>
        /// ブロックの現在の場所を更新する
        /// （このメソッドをオーバーライドする必要があります）
        /// </summary>
        /// <param name="prompts"></param>
        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            var jigOpts = new JigPromptPointOptions
            {
                UserInputControls = (UserInputControls.Accept3dCoordinates | UserInputControls.NullResponseAccepted),
                Message = "挿入位置を指定"
            };
            var jigRes = prompts.AcquirePoint(jigOpts);
            var pt = jigRes.Value;
            if (pt == Point)
                return SamplerStatus.NoChange;

            Point = pt;
            if (jigRes.Status == PromptStatus.OK)
                return SamplerStatus.OK;

            return SamplerStatus.Cancel;
        }

        /// <summary>
        /// 現在の場所にブロックを表示する
        /// （このメソッドをオーバーライドする必要があります）
        /// </summary>
        /// <param name="draw"></param>
        /// <returns></returns>
        protected override bool WorldDraw(Autodesk.AutoCAD.GraphicsInterface.WorldDraw draw)
        {
            using (var inMemoryBlockInsert = new BlockReference(Point, _blockId))
            {
                draw.Geometry.Draw(inMemoryBlockInsert);
            }

            return true;
        }
    }
}
