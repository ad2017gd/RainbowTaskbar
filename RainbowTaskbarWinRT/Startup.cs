using Windows.ApplicationModel;

namespace RainbowTaskbarWinRT {
    public class Startup {
        public StartupTask startupTask;
        public bool Enabled { get => startupTask.State == StartupTaskState.Enabled || startupTask.State == StartupTaskState.EnabledByPolicy; 
        set {
                Toggle(value);
                    } }

        public void Toggle(bool enabled) {
            if (startupTask is null) return;
            if (enabled) startupTask.RequestEnableAsync().Wait();
            else startupTask.Disable();
        }
        public Startup() {
            startupTask = StartupTask.GetForCurrentPackageAsync().AsTask().Result.First();
        }
    }
}
