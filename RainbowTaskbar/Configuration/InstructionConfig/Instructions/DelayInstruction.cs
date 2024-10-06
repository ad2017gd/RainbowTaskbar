using System.Dynamic;
using System.Runtime.Serialization;
using System.Threading;


namespace RainbowTaskbar.Configuration.InstructionConfig.Instructions;


internal class DelayInstruction : Instruction {
    public int Time { get; set; } = 1;

    public override string Description {
        get {
            return App.localization.InstructionFormat(this, Time);
        }
    }
    public override bool Execute(Taskbar window, CancellationToken token) {
        token.WaitHandle.WaitOne(Time);
        return true;
    }
    /*
    public override JObject ToJSON() {
        dynamic data = new ExpandoObject();
        data.Name = GetType().Name;
        data.Time = Time;

        return JObject.FromObject(data);
    }
    */
}