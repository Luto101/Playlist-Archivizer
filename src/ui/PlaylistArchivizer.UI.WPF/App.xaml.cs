using Microsoft.Extensions.DependencyInjection;
using PlaylistArchivizer.UI.WPF.Services;
using PlaylistArchivizer.UI.WPF.Stores;
using PlaylistArchivizer.UI.WPF.ViewModels;
using PlaylistArchivizer.UI.WPF.ViewModels.Home;
using PlaylistArchivizer.UI.WPF.Views;
using System.Windows;

namespace PlaylistArchivizer.UI.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IServiceProvider Services { get; private set; } = default!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();

            ConfigureServices(services);

            Services = services.BuildServiceProvider();

            // Show loading screen
            var navStore = Services.GetRequiredService<NavigationStore>();
            navStore.CurrentViewModel = Services.GetRequiredService<LoadingViewModel>();

            Services.GetRequiredService<MainWindow>().Show();

            // Load Spotify client in a background
            _ = InitializeSpotifyAsync();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ISpotifyClientProvider, SpotifyClientProvider>();
            services.AddSingleton<IPlaylistService, PlaylistService>();

            services.AddSingleton<NavigationStore>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton(sp => new MainWindow
            {
                DataContext = sp.GetRequiredService<MainViewModel>()
            });

            services.AddTransient<HomeViewModel>();
            services.AddTransient<LoadingViewModel>();
            services.AddTransient<Func<Exception, ErrorViewModel>>(sp =>
                ex => new ErrorViewModel(ex));
        }

        private async Task InitializeSpotifyAsync()
        {
            try
            {
                var spotifyProvider = Services.GetRequiredService<ISpotifyClientProvider>();
                var navStore = Services.GetRequiredService<NavigationStore>();

                await spotifyProvider.InitializeAsync();

                // Update UI-bound properties on the UI Thread
                Dispatcher.Invoke((Delegate)(() =>
                {
                    navStore.CurrentViewModel = Services.GetRequiredService<HomeViewModel>();

                    // Active the window
                    Services.GetRequiredService<MainViewModel>().RestoreWindow();
                    Services.GetRequiredService<MainWindow>().Activate();
                }));
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => SwitchToErrorView(ex));
            }
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            SwitchToErrorView(e.Exception);
        }

        private void SwitchToErrorView(Exception ex)
        {
            var navStore = Services.GetRequiredService<NavigationStore>();
            var errorVm = Services.GetRequiredService<Func<Exception, ErrorViewModel>>()(ex);

            navStore.CurrentViewModel = errorVm;
        }
    }
}
