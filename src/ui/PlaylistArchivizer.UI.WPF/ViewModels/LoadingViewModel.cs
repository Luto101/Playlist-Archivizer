using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Threading;

namespace PlaylistArchivizer.UI.WPF.ViewModels
{
    public partial class LoadingViewModel : ObservableObject
    {
        private readonly DispatcherTimer timer;
        private int _dotCount;

        [ObservableProperty]
        private string loadingText;

        public LoadingViewModel()
        {
            _dotCount = 0;
            LoadingText = "Ładowanie";

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };

            timer.Tick += (_, _) =>
            {
                _dotCount = (_dotCount + 1) % 4;
                LoadingText = "Ładowanie" + new string('.', _dotCount);
            };
        }

        [RelayCommand]
        public void StopTimer() => timer.Stop();

        [RelayCommand]
        public void StartTimer() => timer.Start();
    }
}
