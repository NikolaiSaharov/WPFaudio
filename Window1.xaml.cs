using System.Windows;

namespace AudioPlayerWPF
{
    public partial class PlayHistoryWindow : Window
    {
        public PlayHistoryWindow(System.Collections.IEnumerable historyItems)
        {
            InitializeComponent();
            historyListBox.ItemsSource = historyItems;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}