using Avalonia.Controls;
using Avalonia.Interactivity;

namespace MarkdownToPdfConverter.Views
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        private void OnCloseClick(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
