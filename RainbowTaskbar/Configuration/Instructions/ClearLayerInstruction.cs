using Newtonsoft.Json.Linq;
using System.Dynamic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RainbowTaskbar.Configuration.Instructions
{
    [DataContract]
    class ClearLayerInstruction : Instruction
    {

        [field: DataMember]
        public int Layer { get; set; } = 0;

        public override bool Execute(Taskbar window, System.Threading.CancellationToken _)
        {
            Canvas cvs = (Canvas)window.layers.MainDrawRectangles[Layer].Parent;

            window.Dispatcher.Invoke(() =>
            {
                foreach (UIElement elem in cvs.Children.Cast<UIElement>().ToList())
                {
                    if (!(elem is Rectangle))
                    {
                        cvs.Children.Remove(elem);
                    }
                }
                window.layers.MainDrawRectangles[Layer].Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                cvs.InvalidateVisual();
                cvs.UpdateLayout();
            });
            return false;
        }
        public override JObject ToJSON()
        {
            dynamic data = new ExpandoObject();
            data.Name = GetType().Name;
            data.Layer = Layer;

            return JObject.FromObject(data);
        }
    }
}
