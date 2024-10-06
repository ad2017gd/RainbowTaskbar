using RainbowTaskbar.Configuration;
using RainbowTaskbar.Editor.Pages.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RainbowTaskbar.Editor.Pages.Edit {
    public class EditPage : Page {
        public Config Config { get; set; }
        public bool Modified { get; set; } = false;
    }
}
