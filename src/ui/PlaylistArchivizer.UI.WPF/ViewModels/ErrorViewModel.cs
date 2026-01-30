using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace PlaylistArchivizer.UI.WPF.ViewModels
{
    public partial class ErrorViewModel : ObservableObject
    {
        public string ExceptionTitle { get; set; }
        public string ExceptionDetails { get; set; }
        public string ExceptionType { get; set; }

        public ErrorViewModel(Exception exception)
        {
            ExceptionTitle = exception.Message;
            ExceptionType = exception.GetType().Name;

            if (exception.StackTrace != null)
                ExceptionDetails = exception.StackTrace;
            else
                ExceptionDetails = "Brak szczegółów";
        }

        [RelayCommand]
        private static void Close()
        {
            Application.Current.Shutdown();
        }
    }
}
