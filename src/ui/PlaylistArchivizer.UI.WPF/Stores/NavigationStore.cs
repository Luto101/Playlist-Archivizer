using CommunityToolkit.Mvvm.ComponentModel;

namespace PlaylistArchivizer.UI.WPF.Stores
{
    public partial class NavigationStore
    {
        private ObservableObject _currentViewModel = default!;

        public ObservableObject CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;
                OnCurrentViewModelChanged();
            }
        }

        public event Action? CurrentViewModelChanged;

        private void OnCurrentViewModelChanged() => CurrentViewModelChanged?.Invoke();
    }
}
