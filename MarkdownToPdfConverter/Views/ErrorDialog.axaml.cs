using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace MarkdownToPdfConverter.Views
{
    /// <summary>Dialog window that displays an error message with copy and close options.</summary>
    public partial class ErrorDialog : Window
    {
        public ErrorDialog()
        {
            InitializeComponent();
        }

        /// <summary>Gets or sets the error text displayed in the dialog.</summary>
        public string ErrorMessage
        {
            get => ErrorTextBox?.Text ?? string.Empty;
            set
            {
                if (ErrorTextBox != null)
                    ErrorTextBox.Text = value;
            }
        }

        /// <summary>Copies the error message text to the system clipboard.</summary>
        private async void OnCopyClick(object? sender, RoutedEventArgs e)
        {
            try
            {
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel?.Clipboard != null)
                    await topLevel.Clipboard.SetTextAsync(ErrorMessage);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ErrorDialog copy failed: {ex.Message}");
            }
        }

        /// <summary>Closes the dialog window.</summary>
        private void OnCloseClick(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
