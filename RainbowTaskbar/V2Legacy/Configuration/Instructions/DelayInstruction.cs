using System.Dynamic;
using System.Runtime.Serialization;
using System.Threading;
namespace RainbowTaskbar.V2Legacy.Configuration.Instructions;

[DataContract]
internal class DelayInstruction : Instruction {
    [field: DataMember] public int Time { get; set; } = 1;

    public override string Description {
        get {
            return "";
        }
    }
    public override bool Execute(Taskbar window, CancellationToken token) {
        return true;
    }

}