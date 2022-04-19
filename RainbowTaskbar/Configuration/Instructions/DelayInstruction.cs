using System.Runtime.Serialization;
using System.Threading;

namespace RainbowTaskbar.Configuration.Instructions;

[DataContract]
internal class DelayInstruction : Instruction {
    [DataMember] public int Time { get; set; } = 1;

    public override bool Execute(Taskbar window, CancellationToken token) {
        token.WaitHandle.WaitOne(Time);
        return true;
    }
}