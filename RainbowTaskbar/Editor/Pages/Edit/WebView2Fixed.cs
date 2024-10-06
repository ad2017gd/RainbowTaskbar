using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RainbowTaskbar.Editor.Pages.Edit {
    public class WebView2Fixed : WebView2 {
        protected override void OnPreviewKeyDown(KeyEventArgs e) {
            return;
        }
    }
}
