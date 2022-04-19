using System.Runtime.Serialization;
using System.Threading;

namespace RainbowTaskbar.Configuration.Instructions;

[DataContract]
internal class BorderRadiusInstruction : Instruction {
    [DataMember] public int Radius { get; set; }

    public override bool Execute(Taskbar window, CancellationToken _) {
        window.taskbarHelper.Radius = Radius;
        window.windowHelper.Radius = Radius;

        return false;
    }
}