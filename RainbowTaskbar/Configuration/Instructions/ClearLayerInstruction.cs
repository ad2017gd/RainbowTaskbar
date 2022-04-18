using System.Runtime.Serialization;
using System.Threading;

namespace RainbowTaskbar.Configuration.Instructions;

[DataContract]
internal class ClearLayerInstruction : Instruction {
    [field: DataMember] public int Layer { get; set; } = 0;

    public override bool Execute(Taskbar window, CancellationToken _) {
        window.Dispatcher.Invoke(() => { window.layers.renderTargets[Layer].Clear(); });
        return false;
    }
}