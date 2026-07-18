using Avalonia.Controls;
using Avalonia.Interactivity;

namespace MarkdownToPdfConverter.Views
{
    /// <summary>Dialog window that displays application version and credits.</summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        /// <summary>Closes the about dialog.</summary>
        private void OnCloseClick(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
