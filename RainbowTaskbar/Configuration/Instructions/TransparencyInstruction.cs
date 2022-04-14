using Newtonsoft.Json.Linq;
using System.Dynamic;
using System.Runtime.Serialization;
using System.Windows.Controls;

namespace RainbowTaskbar.Configuration.Instructions
{
    [DataContract]
    public class TransparencyInstruction : Instruction
    {

        [field: DataMember]
        public TransparencyInstructionType Type { get; set; }

        [field: DataMember]
        public double Opacity { get; set; } = 1;

        [field: DataMember]
        public TransparencyInstructionStyle Style { get; set; }

        [field: DataMember]
        public int Layer { get; set; }

        public override JObject ToJSON()
        {
            dynamic data = new ExpandoObject();
            data.Name = GetType().Name;
            data.Opacity = Opacity;
            data.Layer = Layer;
            data.Style = Style;
            data.Type = Type;

            return JObject.FromObject(data);
        }

        public enum TransparencyInstructionType
        {
            Taskbar,
            RainbowTaskbar,
            All,
            Style,
            Layer
        }

        public enum TransparencyInstructionStyle
        {
            Default,
            Blur,
            Transparent
        }

        public override bool Execute(Taskbar window, System.Threading.CancellationToken _)
        {
            switch (Type)
            {
                case TransparencyInstructionType.Taskbar:

                    window.taskbarHelper.SetAlpha(Opacity);
                    break;

                case TransparencyInstructionType.RainbowTaskbar:
                    window.Dispatcher.Invoke(() =>
                    {
                        if (window.Opacity != Opacity) window.Opacity = Opacity;
                    });
                    break;

                case TransparencyInstructionType.All:
                    window.Dispatcher.Invoke(() =>
                    {
                        if (window.Opacity != Opacity) window.Opacity = Opacity;
                    });
                    window.taskbarHelper.SetAlpha(Opacity);
                    break;

                case TransparencyInstructionType.Style:

                    switch (Style)
                    {
                        case TransparencyInstructionStyle.Default:
                            if (window.taskbarHelper.Style != Helpers.TaskbarHelper.TaskbarStyle.Default)
                            {
                                window.taskbarHelper.Style = Helpers.TaskbarHelper.TaskbarStyle.ForceDefault;
                            }
                            break;
                        case TransparencyInstructionStyle.Blur:
                            window.taskbarHelper.Style = Helpers.TaskbarHelper.TaskbarStyle.Blur;
                            break;
                        case TransparencyInstructionStyle.Transparent:
                            window.taskbarHelper.Style = Helpers.TaskbarHelper.TaskbarStyle.Transparent;
                            break;
                    }
                    break;

                case TransparencyInstructionType.Layer:
                    window.Dispatcher.Invoke(() => ((Canvas)window.layers.MainDrawRectangles[Layer].Parent).Opacity = Opacity);
                    break;


            }
            return false;

        }

    }
}
