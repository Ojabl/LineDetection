using Microsoft.Win32;
using System.Windows;

namespace LineDetection
{
    public partial class MainWindow : Window
    {
        LineDetectionUtils utils = new LineDetectionUtils();
        public Image _Image { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
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
                    MessageBoxResult result = utils.WarningMessage("It is unadvised to process pictures with resolution bigger than 1920x1080 due to image processing time.\nDo you want to continue?");
                    if (result == MessageBoxResult.No) return;
                }

                HoughWindow houghWindow = new HoughWindow(_Image);
                houghWindow.Show();
            }
        }

        private void mInfo_Click(object sender, RoutedEventArgs e)
        {
            InfoWindow info = new InfoWindow();
            info.Show();
        }
    }
}
