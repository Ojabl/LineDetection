using Emgu.CV;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Media.Imaging;

namespace LineDetection
{
    public partial class OpenedImage : Window
    {
        LineDetectionUtils utils = new LineDetectionUtils();
        Image _Image { get; set; }

        public OpenedImage(Image inputImage)
        {
            InitializeComponent();
            _Image = inputImage;
            imageCanvas.Source = _Image.GetBgrImage().ToBitmapSource();
            Title = _Image.GetPath();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        #region Save
        public void mSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp";
            if (sfd.ShowDialog() == true)
            {
                BitmapSource actualImage = (BitmapSource)imageCanvas.Source;
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(actualImage));

                using (var fileStream = new System.IO.FileStream(sfd.FileName, System.IO.FileMode.Create))
                {
                    encoder.Save(fileStream);
                }
            }
        }
        #endregion

        private void mClone_Click(object sender, RoutedEventArgs e)
        {
            OpenedImage openedImageWindow = new OpenedImage(_Image);
            openedImageWindow.Show();
        }

        #region Analyze
        private void mHistogram_Click(object sender, RoutedEventArgs e)
        {
            if (_Image.isGray)
            {
                _Image.Histogram();
            }
            else
            {
                utils.ErrorMessage("To display a histogram, the image must be grayscale");
            }
        }

        #endregion

        #region Convert
        private void mGray_Click(object sender, RoutedEventArgs e)
        {
            this.imageCanvas.Source = _Image.BGR2GRAY().ToBitmapSource();
            _Image.isGray = true;
        }

        #endregion

        #region Edge detection

        private void mCanny_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Canny edge detection relies on 2 thresholds, do you want to set them?\n\nChoosing \"No\" will result in automatic thresholds settings", "Canny Edge detection", MessageBoxButton.YesNo);

            if(result == MessageBoxResult.Yes)
            {
                CannyWindow cannyWindow = new CannyWindow();
                cannyWindow.ShowDialog();

                if (cannyWindow.GetLowerThreshold() == 0 && cannyWindow.GetUpperThreshold() == 0)
                {
                    utils.ErrorMessage("Please set the thresholds if you want to use Canny Edge detection");
                }
                else
                {
                    this.imageCanvas.Source = _Image.ApplyCannyEdgeDetection(cannyWindow.GetLowerThreshold(), cannyWindow.GetUpperThreshold()).ToBitmapSource();
                }
            }
            else
            {
                this.imageCanvas.Source = _Image.ApplyCannyEdgeDetectionAuto().ToBitmapSource();
            }
        }

        public void ApplyCannyEdgeDetection(int lowThreshold, int highThreshold)
        {
            this.imageCanvas.Source = _Image.ApplyCannyEdgeDetection(lowThreshold, highThreshold).ToBitmapSource();
        }

        private void mSobel_Click(object sender, RoutedEventArgs e)
        {
            this.imageCanvas.Source = _Image.ApplySobelEdgeDetection().ToBitmapSource();
        }

        private void mPrewitt_Click(object sender, RoutedEventArgs e)
        {
            this.imageCanvas.Source = _Image.ApplyPrewittEdgeDetection().ToBitmapSource();
        }

        private void mLaplacian_Click(object sender, RoutedEventArgs e)
        {
            this.imageCanvas.Source = _Image.ApplyLaplacianEdgeDetection().ToBitmapSource();
        }

        #endregion

        #region Hough
        private void mHough_Click(object sender, RoutedEventArgs e)
        {
            HoughWindow houghWindow = new HoughWindow(_Image);
            houghWindow.Show();
            Close();
        }

        #endregion
    }
}
