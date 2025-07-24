using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RainbowTaskbar.Editor.Pages.Controls {
    public class UnsafeImage : System.Windows.Controls.Image, IDisposable {
        public void Dispose() {
            Source = null;
            UpdateLayout();
        }
    }
}
