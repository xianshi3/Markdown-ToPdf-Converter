using Avalonia.Controls;
using Avalonia.Interactivity;

namespace MarkdownToPdfConverter.Views
{
    public partial class ConfirmDialog : Window
    {
        public ConfirmDialog()
        {
            InitializeComponent();
        }

        public string Message
        {
            get => MessageText?.Text ?? string.Empty;
            set
            {
                if (MessageText != null)
                    MessageText.Text = value;
            }
        }

        public string YesText
        {
            set { if (YesButton != null) YesButton.Content = value; }
        }

        public string NoText
        {
            set { if (NoButton != null) NoButton.Content = value; }
        }

        private void OnYesClick(object? sender, RoutedEventArgs e)
        {
            Close(true);
        }

        private void OnNoClick(object? sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}
