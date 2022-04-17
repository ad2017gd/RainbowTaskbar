using System.Dynamic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;

namespace RainbowTaskbar.Configuration.Instructions;

[DataContract]
internal class ClearLayerInstruction : Instruction {
    [field: DataMember] public int Layer { get; set; } = 0;

    public override bool Execute(Taskbar window, CancellationToken _) {

        window.Dispatcher.Invoke(() => {
            window.layers.renderTargets[Layer].Clear();
        });
        return false;
    }

    public override JObject ToJSON() {
        dynamic data = new ExpandoObject();
        data.Name = GetType().Name;
        data.Layer = Layer;

        return JObject.FromObject(data);
    }
}