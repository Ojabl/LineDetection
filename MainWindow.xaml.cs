using Microsoft.Win32;
using System.Windows;

namespace LineDetection
{
    public partial class MainWindow : Window
    {
        public Image _Image { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        private void mOpenEdgeDetection_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp";

            if(ofd.ShowDialog() == true)
            {
                _Image = new Image(ofd.FileName);
                OpenedImage openedImageWindow = new OpenedImage(_Image);
                openedImageWindow.Show();
            }
        }

        private void mOpenHough_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp";

            if (ofd.ShowDialog() == true)
            {
                _Image = new Image(ofd.FileName);

                if (_Image.GetBgrImage().Width > 1920 || _Image.GetBgrImage().Height > 1080)
                {
                    MessageBoxResult result = WarningMessage("It is unadvised to process pictures which resolution is bigger than 1920x1080 due to image processing time.\nDo you want to continue?");
                    if (result == MessageBoxResult.No) return;
                }

                HoughWindow houghWindow = new HoughWindow(_Image);
                houghWindow.Show();
            }
        }

        private void mInfo_Click(object sender, RoutedEventArgs e) //TODO
        {
            string messageBoxContent = @"Praca inżynierska - Odnajdywanie Linii na cyfrowych obrazach mikroskopowych

                                        Autor: Oskar Jabłoński
                                        Promotor: dr. inż. Łukasz Roszkowiak";
            MessageBox.Show(messageBoxContent, "Info", MessageBoxButton.OK);
        }

        #region Utils

        public MessageBoxResult WarningMessage(string message)
        {
            MessageBoxResult choice = MessageBox.Show(message, "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            return choice;
        }

        #endregion
    }
}
