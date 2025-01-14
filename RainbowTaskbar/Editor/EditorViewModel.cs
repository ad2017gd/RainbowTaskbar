using RainbowTaskbar.Editor.Pages.Edit;
using RainbowTaskbar.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RainbowTaskbar.Editor {
    public class EditorViewModel {
        public EditPage EditPage { get; set; }
        public string? LatestUpdateInfo { get; set; } = null;
    }
}
