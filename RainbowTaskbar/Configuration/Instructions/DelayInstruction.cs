using Newtonsoft.Json.Linq;
using System.Dynamic;
using System.Runtime.Serialization;
using System.Threading;

namespace RainbowTaskbar.Configuration.Instructions
{
    [DataContract]
    class DelayInstruction : Instruction
    {

        [field: DataMember]
        public int Time { get; set; } = 1;

        public override bool Execute(Taskbar window, System.Threading.CancellationToken token)
        {
            token.WaitHandle.WaitOne(Time);
            return true;
        }

        public override JObject ToJSON()
        {
            dynamic data = new ExpandoObject();
            data.Name = GetType().Name;
            data.Time = Time;

            return JObject.FromObject(data);
        }

    }
}
