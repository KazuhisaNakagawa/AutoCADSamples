using Autodesk.AutoCAD.Runtime;
using System.Windows;

[assembly: CommandClass(typeof(AutoCADSamples.HelloWorld))]

namespace AutoCADSamples
{
    class HelloWorld
    {
        [CommandMethod("AutoCADSamples", "HelloWorld", "HelloWorld", CommandFlags.Modal)]
        public void HelloWorldCommand()
        {
            MessageBox.Show("Hello World!!");

        }
    }
}
