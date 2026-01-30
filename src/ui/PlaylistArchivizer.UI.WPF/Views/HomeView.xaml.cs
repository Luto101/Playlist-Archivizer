using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace PlaylistArchivizer.UI.WPF.Views
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();
        }

        private void ScrollToTop()
        {
            dataGrid.SelectedIndex = -1;

            // Use Background priority to ensure the UI has finished 
            // generating the rows before we try to scroll
            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (VisualTreeHelper.GetChild(dataGrid, 0) is Decorator border &&
                    border.Child is ScrollViewer scrollViewer)
                {
                    scrollViewer.ScrollToTop();
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void dataGrid_Sorting(object sender, DataGridSortingEventArgs e) => ScrollToTop();
        private void dataGrid_TargetUpdated(object sender, DataTransferEventArgs e) => ScrollToTop();
    }
}