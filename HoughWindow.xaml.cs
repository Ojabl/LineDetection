using Emgu.CV;
using System;
using System.CodeDom;
using System.Drawing;
using System.Windows;

namespace LineDetection
{
    public partial class HoughWindow : Window
    {
        Image _Image { get; set; }
        Image BinarizedSobelImg { get; set; }

        public HoughWindow(Image inputImage)
        {
            InitializeComponent();

            _Image = inputImage;
            BinarizedSobelImg = new Image(_Image.Sobel(new Bitmap(_Image.GetPath()))); //TODO overload for Image constructor
            
            Title = _Image.GetPath();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            ImgSource.Source = _Image.GetBgrImage().ToBitmapSource();
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if (IsThresholdValid())
            {
                ImgAccum.Source = Image.HoughTransform(new Bitmap(_Image.GetPath()));
                ImgResult.Source =  ;
                
            }
        }

        #region Utils

        public bool IsThresholdValid()
        {
            if (ImgSource.Source == null)
            {
                ErrorMessage("Source Image is empty!");
                return false;
            }
            else if (TbTr.Text == null || TbTr.Text == "")
            {
                ErrorMessage("Threshold value must be filled!");
                return false;
            }
            else if (!int.TryParse(TbTr.Text, out int tr))
            {
                ErrorMessage("Value of the threshold field must be an integer!");
                return false;
            }
            else if (TbTr.Text.Length > 3)
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
