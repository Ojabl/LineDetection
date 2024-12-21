using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;

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
                    MessageBoxResult result = utils.WarningMessage("It is unadvised to process pictures with resolution bigger than 1920x1080 due to image processing time.\nShrink the image using the Gaussian pyramid?");
                    if(result == MessageBoxResult.Yes)
                    {
                        GaussianPyramid(_Image.GetBgrImage());
                    }
                    else return;
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

        #region Utils

        private void GaussianPyramid(Image<Bgr,byte> image)
        {
            while (image.Width > 1920 || image.Height > 1080)
            {
                image = image.PyrDown();
            }

            _Image._bgrImage = image;

            MessageBox.Show($"Image has been successfully shrunk.\nNew resolution of the image is {image.Width}x{image.Height}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion
    }
}
