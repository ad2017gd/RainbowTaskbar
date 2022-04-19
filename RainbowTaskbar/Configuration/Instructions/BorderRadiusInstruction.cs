using System.Runtime.Serialization;
using System.Threading;

namespace RainbowTaskbar.Configuration.Instructions;

[DataContract]
internal class BorderRadiusInstruction : Instruction {
    [field: DataMember] public int Radius { get; set; } = 0;

    public override bool Execute(Taskbar window, CancellationToken _) {
        window.taskbarHelper.Radius = Radius;
        window.windowHelper.Radius = Radius;

        return false;
    }
}