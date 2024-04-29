using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.CodeDom;
using System.Drawing;
using Point = System.Drawing.Point;
using System.Security.Cryptography.X509Certificates;
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
                if(TbTr.Text == "Auto") tr = 150; //auto threshold function, manually inserted tr value works well

                ImgAccum.Source = _Image.HoughTransform(BinarizedSobelBitmap, tr).ToMat().ToBitmapSource();

                Bitmap img = _Image.GetBitmap();
                Graphics g = Graphics.FromImage(img);
                Pen pen = new Pen(Color.Red, 3);

                int dp = (int)Math.Round(Math.Sqrt(Math.Pow(_Image.GetBgrImage().Width, 2) + Math.Pow(_Image.GetBgrImage().Height, 2))); //is _Image.GetBgrImage().Width working well?
                Point size = new Point(180, dp);

                while (true)
                {
                    Point pt = _Image.SearchLine(size, tr);
                    if(pt.X == -1) break;
                    if(pt.X > 0)
                    {
                        int y1 = (int)((-Math.Cos(pt.X * (Math.PI / 180)) / Math.Sin(pt.X * (Math.PI / 180))) * 0 + (double)pt.Y / Math.Sin(pt.X * (Math.PI / 180)));

                        int y2 = (int)((-Math.Cos(pt.X * (Math.PI / 180)) / Math.Sin(pt.X * (Math.PI / 180))) * _Image.GetBgrImage().Width + (double)pt.Y / Math.Sin(pt.X * (Math.PI / 180)));

                        g.DrawLine(pen, 0, y1, _Image.GetBgrImage().Width, y2);
                    }
                }
                ImgResult.Source = img.ToMat().ToBitmapSource();
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
