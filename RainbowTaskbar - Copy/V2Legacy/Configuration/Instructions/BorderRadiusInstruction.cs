using System.Dynamic;
using System.Runtime.Serialization;
using System.Threading;

namespace RainbowTaskbar.V2Legacy.Configuration.Instructions;

[DataContract]
internal class BorderRadiusInstruction : Instruction {
    [field: DataMember] public int Radius { get; set; } = 0;

    public override string Description {
        get {
            return null;
        }
    }

    public override bool Execute(Taskbar window, CancellationToken _) {
        return false;
    }

}