using Newtonsoft.Json.Linq;
using System.Dynamic;
using System.Runtime.Serialization;

namespace RainbowTaskbar.Configuration.Instructions
{
    [DataContract]
    class BorderRadiusInstruction : Instruction
    {

        [field: DataMember]
        public int Radius { get; set; } = 0;

        public override bool Execute(Taskbar window, System.Threading.CancellationToken _)
        {
            window.taskbarHelper.Radius = Radius;
            window.windowHelper.Radius = Radius;

            return false;
        }

        public override JObject ToJSON()
        {
            dynamic data = new ExpandoObject();
            data.Name = GetType().Name;
            data.Radius = Radius;

            return JObject.FromObject(data);
        }
    }
}
