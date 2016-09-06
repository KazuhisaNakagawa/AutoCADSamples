using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

[assembly: CommandClass(typeof(AutoCADSamples.Jig.SelectObjectJig))]

namespace AutoCADSamples.Jig
{
    class SelectObjectJig
    {
        /// <summary>
        /// 選択した円をドラッグし、クリックした点に移動するコマンド
        /// </summary>
        [CommandMethod("AutoCADSamples", "SelectObjectJig", CommandFlags.Modal)]
        public void SelectObjectJigCommand()
        {
            var document = Application.DocumentManager.MdiActiveDocument;
            var database = document.Database;
            var editor = document.Editor;

            // 図形を選択
            var pEntityOptions = new PromptEntityOptions("\n円を選択");
            pEntityOptions.SetRejectMessage("\n円を選択してください");
            pEntityOptions.AddAllowedClass(typeof(Circle), true);

            var pEntityResult = editor.GetEntity(pEntityOptions);

            if (pEntityResult.Status != PromptStatus.OK)
                return;
            using (var transaction = database.TransactionManager.StartTransaction())
            {
                var circle = (Circle)transaction.GetObject(pEntityResult.ObjectId, OpenMode.ForWrite);

                var mySelectedObjextJig = new MySelectedObjectJig();
                mySelectedObjextJig.Center = circle.Center;
                mySelectedObjextJig.Normal = circle.Normal;
                mySelectedObjextJig.Radius = circle.Radius;

                Point3d point;
                var res = mySelectedObjextJig.DragMe(pEntityResult.ObjectId, out point);

                if (res.Status == PromptStatus.OK)
                {

                    // ユーザーがクリックした位置に選択したオブジェクトを移動する
                    circle.Center = point;

                    transaction.Commit();
                }


            }
        }
    }

    public class MySelectedObjectJig : DrawJig
    {
        //private ObjectId _objectId = ObjectId.Null;

        public Point3d Center { get; set; }
        public Vector3d Normal { get; set; }
        public double Radius { get; set; }

        /// <summary>
        /// ユーザがクリックするまでドラッグしている円を表示する
        /// </summary>
        /// <param name="iObjctId">選択した円のオブジェクトID</param>
        /// <param name="oClickedPoint">クリックされた点</param>
        /// <returns></returns>
        public PromptResult DragMe(ObjectId iObjctId, out Point3d oClickedPoint)
        {
            //_objectId = iObjctId;
            var document = Application.DocumentManager.MdiActiveDocument;
            var editor = document.Editor;

            var jigRes = editor.Drag(this);
            oClickedPoint = Center;

            return jigRes;
        }

        /// <summary>
        /// 選択した円の現在の場所を更新する
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
            if (pt == Center)
                return SamplerStatus.NoChange;

            Center = pt;
            if (jigRes.Status == PromptStatus.OK)
                return SamplerStatus.OK;

            return SamplerStatus.Cancel;
        }

        /// <summary>
        /// 現在の場所に円を表示する
        /// （このメソッドをオーバーライドする必要があります）
        /// </summary>
        /// <param name="draw"></param>
        /// <returns></returns>
        protected override bool WorldDraw(Autodesk.AutoCAD.GraphicsInterface.WorldDraw draw)
        {
            using (var inMemoryBlockInsert = new Circle(Center, Normal, Radius))
            {
                draw.Geometry.Draw(inMemoryBlockInsert);
            }

            return true;
        }
    }
}
