using CommunityToolkit.Mvvm.ComponentModel;
using PlaylistArchivizer.UI.WPF.Stores;
using System.Windows;

namespace PlaylistArchivizer.UI.WPF.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly NavigationStore _navigationStore;

        public ObservableObject CurrentViewModel => _navigationStore.CurrentViewModel;

        [ObservableProperty]
        private WindowState currentWindowState;

        public MainViewModel(NavigationStore navigationStore)
        {
            _navigationStore = navigationStore;

            _navigationStore.CurrentViewModelChanged += () =>
            {
                OnPropertyChanged(nameof(CurrentViewModel));
            };

        }

        public void RestoreWindow()
        {
            if (CurrentWindowState == WindowState.Minimized)
                CurrentWindowState = WindowState.Normal;
        }
    }
}
