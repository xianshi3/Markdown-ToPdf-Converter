using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MarkdownToPdfConverter.Services;
using MarkdownToPdfConverter.Views;

namespace MarkdownToPdfConverter
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
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
