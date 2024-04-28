﻿using Emgu.CV;
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
        Image _Image { get; set; }
        Bitmap BinarizedSobelBitmap { get; set; }
        int tr = 0;

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
                tr = int.Parse(TbTr.Text);

                //ImgAccum.Source = _Image.HoughTransform(BinarizedSobelBitmap, tr);
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
