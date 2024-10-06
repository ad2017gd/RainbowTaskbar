using System.Dynamic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;


namespace RainbowTaskbar.Configuration.Instruction.Instructions;


internal class ClearLayerInstruction : Instruction {
    public int Layer { get; set; } = 0;

    public override string Description {
        get {
            return App.localization.InstructionFormat(this, Layer);
        }
    }

    public override bool Execute(Taskbar window, CancellationToken token) {
        window.Dispatcher.Invoke(() => {
            window.canvasManager.layers.MakeIfNeeded(Layer);
            window.canvasManager.layers.renderTargets[Layer].Clear();
        }, System.Windows.Threading.DispatcherPriority.Normal, token);
        return false;
    }
    /*
    public override JObject ToJSON() {
        dynamic data = new ExpandoObject();
        data.Name = GetType().Name;
        data.Layer = Layer;

        return JObject.FromObject(data);
    }
    */
}