using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MarkdownToPdfConverter.Services;
using MarkdownToPdfConverter.Views;

namespace MarkdownToPdfConverter
{
    /// <summary>The main application class that configures theme, localization, and the main window.</summary>
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /// <summary>Applies theme and localization, then creates and shows the main window.</summary>
        public override void OnFrameworkInitializationCompleted()
        {
            // Apply saved theme and language settings
            ThemeService.Instance.ApplyToApplication();
            LocalizationService.Instance.ApplyToApplication();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainWindow = new MainWindow();
                desktop.MainWindow = mainWindow;
                WindowService.MainWindow = mainWindow;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
