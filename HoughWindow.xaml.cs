using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.CodeDom;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Effects;

namespace LineDetection
{
    public partial class HoughWindow : Window
    {
        public Image _Image { get; set; }
        public Bitmap BinarizedSobelBitmap { get; set; }
        public int tr;

        public HoughWindow(Image inputImage)
        {
            InitializeComponent();

            _Image = inputImage;
            BinarizedSobelBitmap = _Image.Sobel(_Image.GetBitmap());
            
            Title = _Image.GetPath();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            ImgSource.Source = _Image.GetBgrImage().ToBitmapSource();
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if (IsThresholdValid())
            {
                //tr = int.Parse(TbTr.Text);

                ImgAccum.Source = _Image.HoughTransform(BinarizedSobelBitmap, tr).ToMat().ToBitmapSource();
            }
        }

        #region Utils

        public bool IsThresholdValid()
        {
            string trText = TbTr.Text;

            if (ImgSource.Source == null)
            {
                ErrorMessage("Source Image is empty!");
                return false;
            }
            else if(trText == "Auto")
            {
                // Auto threshold function
                return true;
            }
            if (trText == null || trText == "")
            {
                ErrorMessage("Threshold value must be filled!");
                return false;
            }
            else if (!int.TryParse(trText, out tr))
            {
                ErrorMessage("Value of the threshold field must be an integer!");
                return false;
            }
            else if (trText.Length > 3)
            {
                ErrorMessage("Threshold value cannot be longer than 3 digits!");
                return false;
            }
            return true;
        }

        public void ErrorMessage(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        #endregion
    }
}
