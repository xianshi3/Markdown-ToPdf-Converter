using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace MarkdownToPdfConverter.Views
{
    public partial class ErrorDialog : Window
    {
        public ErrorDialog()
        {
            InitializeComponent();
        }

        public string ErrorMessage
        {
            get => ErrorTextBox?.Text ?? string.Empty;
            set
            {
                if (ErrorTextBox != null)
                    ErrorTextBox.Text = value;
            }
        }

        private async void OnCopyClick(object? sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel?.Clipboard != null)
                await topLevel.Clipboard.SetTextAsync(ErrorMessage);
        }

        private void OnCloseClick(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
