using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace MarkdownToPdfConverter.Views.Components
{
    public partial class TitleBarComponent : UserControl
    {
        private Window? _window;

        public TitleBarComponent()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            _window = this.GetVisualRoot() as Window;
            if (_window != null)
            {
                _window.PropertyChanged += OnWindowPropertyChanged;
                UpdateMaximizeRestoreState(_window.WindowState);
            }

            MinimizeButton.Click += OnMinimizeClick;
            MaximizeRestoreButton.Click += OnMaximizeRestoreClick;
            CloseButton.Click += OnCloseClick;
            TitleBarRoot.PointerPressed += OnTitleBarPointerPressed;
        }

        private void OnWindowPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == Window.WindowStateProperty && _window != null)
                UpdateMaximizeRestoreState(_window.WindowState);
        }

        private void UpdateMaximizeRestoreState(WindowState state)
        {
            var isMaximized = state == WindowState.Maximized;
            MaximizeIcon.IsVisible = !isMaximized;
            RestoreIcon.IsVisible = isMaximized;
            ToolTip.SetTip(MaximizeRestoreButton, isMaximized ? "Restore" : "Maximize");
        }

        private void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (_window == null) return;

            if (IsOverButton(e))
                return;

            if (e.ClickCount == 2)
            {
                ToggleMaximizeRestore();
                return;
            }

            _window.BeginMoveDrag(e);
        }

        private static bool IsOverButton(PointerPressedEventArgs e)
        {
            var source = e.Source as Visual;
            while (source != null)
            {
                if (source is Button)
                    return true;
                source = source.GetVisualParent();
            }
            return false;
        }

        private void OnMinimizeClick(object? sender, RoutedEventArgs e)
        {
            if (_window != null)
                _window.WindowState = WindowState.Minimized;
        }

        private void OnMaximizeRestoreClick(object? sender, RoutedEventArgs e)
        {
            ToggleMaximizeRestore();
        }

        private void ToggleMaximizeRestore()
        {
            if (_window == null) return;
            _window.WindowState = _window.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void OnCloseClick(object? sender, RoutedEventArgs e)
        {
            _window?.Close();
        }
    }
}
