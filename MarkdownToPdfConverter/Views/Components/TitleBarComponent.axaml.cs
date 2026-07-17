using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace MarkdownToPdfConverter.Views.Components
{
    /// <summary>Custom title bar with window control buttons and drag-to-move support.</summary>
    public partial class TitleBarComponent : UserControl
    {
        private Window? _window;

        public TitleBarComponent()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        /// <summary>Subscribes to window events and sets up control buttons on load.</summary>
        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            _window = this.GetVisualRoot() as Window;
            if (_window != null)
            {
                // Track window state changes to update the maximize/restore icon
                _window.PropertyChanged += OnWindowPropertyChanged;
                UpdateMaximizeRestoreState(_window.WindowState);
            }

            MinimizeButton.Click += OnMinimizeClick;
            MaximizeRestoreButton.Click += OnMaximizeRestoreClick;
            CloseButton.Click += OnCloseClick;
            TitleBarRoot.PointerPressed += OnTitleBarPointerPressed;
        }

        /// <summary>Updates the maximize/restore button icon when the window state changes.</summary>
        private void OnWindowPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == Window.WindowStateProperty && _window != null)
                UpdateMaximizeRestoreState(_window.WindowState);
        }

        /// <summary>Toggles the maximize/restore icon and tooltip based on the current window state.</summary>
        private void UpdateMaximizeRestoreState(WindowState state)
        {
            var isMaximized = state == WindowState.Maximized;
            MaximizeIcon.IsVisible = !isMaximized;
            RestoreIcon.IsVisible = isMaximized;
            ToolTip.SetTip(MaximizeRestoreButton, isMaximized ? "Restore" : "Maximize");
        }

        /// <summary>Starts window dragging on pointer press, or toggles maximize on double-click.</summary>
        private void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (_window == null) return;

            // Don't start drag if the press was on a button
            if (IsOverButton(e))
                return;

            // Double-click the title bar to toggle maximize/restore
            if (e.ClickCount == 2)
            {
                ToggleMaximizeRestore();
                return;
            }

            _window.BeginMoveDrag(e);
        }

        /// <summary>Checks if the pointer event originated from a button control.</summary>
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

        /// <summary>Minimizes the application window.</summary>
        private void OnMinimizeClick(object? sender, RoutedEventArgs e)
        {
            if (_window != null)
                _window.WindowState = WindowState.Minimized;
        }

        /// <summary>Toggles the window between maximized and normal states.</summary>
        private void OnMaximizeRestoreClick(object? sender, RoutedEventArgs e)
        {
            ToggleMaximizeRestore();
        }

        /// <summary>Switches the window between maximized and normal states.</summary>
        private void ToggleMaximizeRestore()
        {
            if (_window == null) return;
            _window.WindowState = _window.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        /// <summary>Closes the application window.</summary>
        private void OnCloseClick(object? sender, RoutedEventArgs e)
        {
            _window?.Close();
        }
    }
}
