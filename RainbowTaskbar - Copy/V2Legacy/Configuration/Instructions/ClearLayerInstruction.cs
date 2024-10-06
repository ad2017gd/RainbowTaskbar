using System.Dynamic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RainbowTaskbar.V2Legacy.Configuration.Instructions;

[DataContract]
internal class ClearLayerInstruction : Instruction {
    [field: DataMember] public int Layer { get; set; } = 0;

    public override string Description {
        get {
            return null;
        }
    }

    public override bool Execute(Taskbar window, CancellationToken token) {
        return false;
    }

}