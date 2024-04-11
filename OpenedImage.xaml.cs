using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Media.Imaging;

namespace LineDetection
{
    public partial class OpenedImage : Window
    {
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
                MessageBox.Show("aby wyświetlić histogram, obraz musi być szaroodcieniowy", "Błąd", MessageBoxButton.OK);
            }
        }

        #endregion

        #region Convert
        private void mGray_Click(object sender, RoutedEventArgs e)
        {
            this.imageCanvas.Source = _Image.BGR2GRAY().ToBitmapSource();
            _Image.isGray = true;
        }

        private void mBgr_Click(object sender, RoutedEventArgs e)
        {
            this.imageCanvas.Source = _Image.GetBgrImage().ToBitmapSource();
            _Image.isGray = false;
        }

        #endregion

        #region Edge detection

        private void Canny_Click(object sender, RoutedEventArgs e)
        {
            this.imageCanvas.Source = _Image.ApplyCannyEdgeDetection().ToBitmapSource();
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
            if (_Image.isGray)
            {
                this.imageCanvas.Source = _Image.Hough().ToBitmapSource();
            }
            else
            {
                MessageBox.Show("Obraz musi być szaroodcieniowy\nPrzekonwertuj obraz na jednokanałowy", "Błąd", MessageBoxButton.OK);
            }
        }

        #endregion

        
    }
}
