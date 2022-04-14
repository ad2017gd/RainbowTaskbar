using System.Windows;

namespace RainbowTaskbar
{
    /// <summary>
    /// Interaction logic for Taskbar.xaml
    /// </summary>
    public partial class Taskbar : Window
    {
        public bool Secondary = false;
        public Drawing.Layers layers;
        public Helpers.TaskbarHelper taskbarHelper;
        public Helpers.WindowHelper windowHelper;
        public TaskbarViewModel viewModel;

        public Taskbar(bool secondary)
        {
            InitializeComponent();
            Secondary = secondary;

            viewModel = new TaskbarViewModel(this);
            Closing += viewModel.OnWindowClosing;
            DataContext = viewModel;
        }


        public Taskbar()
        {
            InitializeComponent();

            viewModel = new TaskbarViewModel(this);
            Closing += viewModel.OnWindowClosing;
            DataContext = viewModel;
        }

        private void RainbowTaskbar_Closed(object sender, System.EventArgs e)
        {
            taskbarHelper.Style = Helpers.TaskbarHelper.TaskbarStyle.ForceDefault;
            taskbarHelper.SetBlur();
        }
    }
}
