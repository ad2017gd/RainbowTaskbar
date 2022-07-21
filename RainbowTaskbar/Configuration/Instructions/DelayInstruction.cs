using System.Dynamic;
using System.Runtime.Serialization;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace RainbowTaskbar.Configuration.Instructions;

[DataContract]
internal class DelayInstruction : Instruction {
    [field: DataMember] public int Time { get; set; } = 1;

    public override string Name {
        get {
            return $"Sleep - " + Time + "ms";
        }
    }
    public override bool Execute(Taskbar window, CancellationToken token) {
        token.WaitHandle.WaitOne(Time);
        return true;
    }

    public override JObject ToJSON() {
        dynamic data = new ExpandoObject();
        data.Name = GetType().Name;
        data.Time = Time;

        return JObject.FromObject(data);
    }
}