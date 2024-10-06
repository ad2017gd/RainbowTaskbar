using PropertyChanged;
using RainbowTaskbar.HTTPAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RainbowTaskbar.Configuration.InstructionConfig {
    public class InstructionGroup : INotifyPropertyChanged {

        [OnChangedMethod(nameof(SetupPropertyChanged))]
        public BindingList<Instruction> Instructions { get; set; } = new BindingList<Instruction>();

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

        public void GroupTask(CancellationToken token) {
            while (!token.IsCancellationRequested) {
                var slept = false;

                for (groupStep = 0;
                     groupStep < Instructions.Count && !token.IsCancellationRequested;
                     groupStep++) {
                    if (API.APISubscribed.Count > 0) {
                        /* Rewrite system text json
                         * 
                        var data = new JObject();
                        data.Add("type", "InstructionStep");
                        data.Add("index", App.Config.configStep);
                        data.Add("instruction", App.Config.Instructions[App.Config.configStep].ToJSON());
                        WebSocketAPIServer.SendToSubscribed(data.ToString());
                        */
                    }

                    try {
                        var pHandle = App.GetCurrentProcess();
                        App.SetProcessWorkingSetSize(pHandle, -1, -1);


                        var tasks = new List<Task>();
                        // todo: config-specific taskbar????
                        /*App.taskbars.ForEach(taskbar => {
                            tasks.Add(Task.Run(() => {
                                if (groupStep < Instructions.Count && Instructions[groupStep].Execute(taskbar, token)) slept = true;
                            }));
                        });*/
                        Task.WaitAll(tasks.ToArray(), token);

                    }
                    catch (Exception e) {
                        if (e.GetType() == typeof(OperationCanceledException) || e.InnerException is not null && e.InnerException.GetType() == typeof(TaskCanceledException)) {
                            return;
                        }
                        // TODO: Localization & config-specific reload
                        MessageBox.Show(
                            $"The \"{Instructions[groupStep].Description}\" instruction at index {groupStep} (starting from 0) threw an exception, it will be removed from the instruction group.\n${e.Message}",
                            "RainbowTaskbar", MessageBoxButton.OK, MessageBoxImage.Error);
                        Application.Current.Dispatcher.Invoke(() => {
                            Instructions.RemoveAt(groupStep);
                            // App.Config.ToFile(); Save
                            //App.ReloadTaskbars();
                        });
                        return;
                    }
                }

                if (!slept) break;
            }
        }
        public void StartTask(CancellationToken token) {
            Task = Task.Run(() => StartTask(token));
        }
    }

}
