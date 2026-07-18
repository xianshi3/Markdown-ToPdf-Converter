using Avalonia.Controls;
using Avalonia.Interactivity;

namespace MarkdownToPdfConverter.Views
{
    /// <summary>Dialog window that prompts the user with a yes/no confirmation question.</summary>
    public partial class ConfirmDialog : Window
    {
        public ConfirmDialog()
        {
            InitializeComponent();
        }

        /// <summary>Gets or sets the confirmation message text.</summary>
        public string Message
        {
            get => MessageText?.Text ?? string.Empty;
            set
            {
                if (MessageText != null)
                    MessageText.Text = value;
            }
        }

        /// <summary>Sets the text of the Yes button.</summary>
        public string YesText
        {
            set { if (YesButton != null) YesButton.Content = value; }
        }

        /// <summary>Sets the text of the No button.</summary>
        public string NoText
        {
            set { if (NoButton != null) NoButton.Content = value; }
        }

        /// <summary>Closes the dialog with a true (yes) result.</summary>
        private void OnYesClick(object? sender, RoutedEventArgs e)
        {
            Close(true);
        }

        /// <summary>Closes the dialog with a false (no) result.</summary>
        private void OnNoClick(object? sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}
