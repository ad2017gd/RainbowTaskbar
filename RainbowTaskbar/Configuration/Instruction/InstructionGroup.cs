using PropertyChanged;
using RainbowTaskbar.HTTPAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RainbowTaskbar.Configuration.Instruction {
    public class InstructionGroup : INotifyPropertyChanged {

        [OnChangedMethod(nameof(SetupPropertyChanged))]
        public BindingList<Instruction> Instructions { get; set; } = new BindingList<Instruction>();
        [JsonIgnore]
        public Task Task { get; set; } = null;
        private int groupStep = 0;

        public event PropertyChangedEventHandler PropertyChanged;

        private void SetupPropertyChanged() {
            Instructions.ListChanged += (_, _) => OnPropertyChanged(nameof(Instructions));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        public InstructionGroup() {
            SetupPropertyChanged();
        }

        public bool RunOnceTask(CancellationToken token) {
            bool slept = false;
            for (groupStep = 0;
                     groupStep < Instructions.Count && !token.IsCancellationRequested;
                     groupStep++) {

                try {
                    var pHandle = App.GetCurrentProcess();
                    App.SetProcessWorkingSetSize(pHandle, -1, -1);


                    var tasks = new List<Task>();

                    App.taskbars.ForEach(taskbar => {
                        tasks.Add(Task.Run(() => {
                            if (groupStep < Instructions.Count && Instructions[groupStep].Execute(taskbar, token)) slept = true;
                        }));
                    });
                    Task.WaitAll(tasks.ToArray(), token);

                }
                catch (Exception e) {
                    if (e.GetType() == typeof(OperationCanceledException) || e.InnerException is not null && e.InnerException.GetType() == typeof(TaskCanceledException)) {
                        return slept;
                    }
                    MessageBox.Show(
                        $"The \"{Instructions[groupStep].Description}\" instruction at index {groupStep} (starting from 0) threw an exception, it will be removed from the instruction group.\n${e.Message}",
                        "RainbowTaskbar", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Dispatcher.Invoke(() => {
                        Instructions.RemoveAt(groupStep);
                        App.Settings.SelectedConfig.ToFile();
                        App.ReloadTaskbars();
                    });
                    return slept;
                }
            }
            return slept;
        }

        public void LoopTask(CancellationToken token) {
            while (!token.IsCancellationRequested) {
                var slept = RunOnceTask(token);

                if (!slept) break;
            }
        }
        public void StartOnceTask(CancellationToken token) {
            Task = Task.Run(() => RunOnceTask(token));
        }
        public void StartLoopTask(CancellationToken token) {
            Task = Task.Run(() => LoopTask(token));
        }
    }

}
