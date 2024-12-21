using Emgu.CV;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using Point = System.Drawing.Point;

namespace LineDetection
{
    public partial class HoughWindow : Window
    {
        LineDetectionUtils utils = new LineDetectionUtils();
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
                ImgAccum.Source = _Image.HoughTransform(BinarizedSobelBitmap, tr).ToMat().ToBitmapSource();

                Bitmap img = _Image.GetBitmap();
                Graphics g = Graphics.FromImage(img);
                Pen pen = new Pen(Color.Red, 3);

                if (TbTr.Text == "Auto")
                {
                    tr = AutoThreshold(img, g, pen);
                    return;
                }

                int dp = (int)Math.Round(Math.Sqrt(Math.Pow(_Image.GetBgrImage().Width, 2) + Math.Pow(_Image.GetBgrImage().Height, 2)));
                Point size = new Point(180, dp);
                HashSet<Point> detectedLines = new HashSet<Point>();

                try
                {
                    while (true)
                    {
                        try 
                        {
                            Point testpt = _Image.SearchLine(size, tr, detectedLines);
                            if (testpt.X == -1) break;

                            long y1 = (int)((-Math.Cos(testpt.X * (Math.PI / 180)) / Math.Sin(testpt.X * (Math.PI / 180))) * 0 + (double)testpt.Y / Math.Sin(testpt.X * (Math.PI / 180)));
                            long y2 = (int)((-Math.Cos(testpt.X * (Math.PI / 180)) / Math.Sin(testpt.X * (Math.PI / 180))) * _Image.GetBgrImage().Width + (double)testpt.Y / Math.Sin(testpt.X * (Math.PI / 180)));
                            g.DrawLine(pen, 0, y1, _Image.GetBgrImage().Width, y2);
                        }
                        catch (Exception ex)
                        {
                            continue;
                        }
                    }
                }
                catch (Exception ex)
                {
                    utils.WarningMessage("C# cannot handle so many lines!");
                }
                
                //if (pt.X == -1) break;
                //if (pt.X > 0)
                //{
                //    int y1 = (int)((-Math.Cos(pt.X * (Math.PI / 180)) / Math.Sin(pt.X * (Math.PI / 180))) * 0 + (double)pt.Y / Math.Sin(pt.X * (Math.PI / 180)));

                //    int y2 = (int)((-Math.Cos(pt.X * (Math.PI / 180)) / Math.Sin(pt.X * (Math.PI / 180))) * _Image.GetBgrImage().Width + (double)pt.Y / Math.Sin(pt.X * (Math.PI / 180)));

                //    g.DrawLine(pen, 0, y1, _Image.GetBgrImage().Width, y2);
                //}
                //}
                ImgResult.Source = img.ToMat().ToBitmapSource();
            }
        }

        #region Utils

        public bool IsThresholdValid()
        {
            string trText = TbTr.Text;

            if (ImgSource.Source == null)
            {
                utils.ErrorMessage("Source Image is empty!");
                return false;
            }
            else if(trText == "Auto")
            {
                return true;
            }
            if (trText == null || trText == "")
            {
                utils.ErrorMessage("Threshold value must be filled!");
                return false;
            }
            else if (!int.TryParse(trText, out tr))
            {
                utils.ErrorMessage("Value of the threshold field must be an integer!");
                return false;
            }
            else if (trText.Length > 3)
            {
                utils.ErrorMessage("Threshold value cannot be longer than 3 digits!");
                return false;
            }
            return true;
        }

        public int AutoThreshold(Bitmap img, Graphics g, Pen pen, int lineCount = 10)
        {
            tr = 300;
            HashSet<Point> detectedLines = new HashSet<Point>();
            int dp = (int)Math.Round(Math.Sqrt(Math.Pow(_Image.GetBgrImage().Width, 2) + Math.Pow(_Image.GetBgrImage().Height, 2)));
            Point size = new Point(180, dp);
            
            while (tr > 0)
            {
                while (true)
                {
                    Point testpt = _Image.SearchLine(size, tr, detectedLines);
                    if (testpt.X == -1) break;

                    int y1 = (int)((-Math.Cos(testpt.X * (Math.PI / 180)) / Math.Sin(testpt.X * (Math.PI / 180))) * 0 + (double)testpt.Y / Math.Sin(testpt.X * (Math.PI / 180)));
                    int y2 = (int)((-Math.Cos(testpt.X * (Math.PI / 180)) / Math.Sin(testpt.X * (Math.PI / 180))) * _Image.GetBgrImage().Width + (double)testpt.Y / Math.Sin(testpt.X * (Math.PI / 180)));
                    g.DrawLine(pen, 0, y1, _Image.GetBgrImage().Width, y2);
                }

                if (detectedLines.Count == lineCount)
                {
                    TbTr.Text = tr.ToString();
                    return tr;
                }
                if (detectedLines.Count > lineCount)
                {
                    utils.Message($"Exactly {lineCount} lines cannot be found, showing {detectedLines.Count} lines.");
                    TbTr.Text = tr.ToString();
                    return tr;
                }

                ImgResult.Source = img.ToMat().ToBitmapSource();
                tr -= 5;
            }

            utils.ErrorMessage($"{lineCount} lines cannot be found!");
            return 0;
        }

        #endregion

        #region save

        private void BtnSaveAcum_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg";
            if(sfd.ShowDialog() == true)
            {
                BitmapSource accumImage = (BitmapSource)ImgAccum.Source;
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(accumImage));

                using(var fileStream = new System.IO.FileStream(sfd.FileName, System.IO.FileMode.Create))
                {
                    encoder.Save(fileStream);
                }
            }
        }

        private void BtnSaveImg_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg";
            if (sfd.ShowDialog() == true)
            {
                BitmapSource accumImage = (BitmapSource)ImgResult.Source;
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(accumImage));

                using (var fileStream = new System.IO.FileStream(sfd.FileName, System.IO.FileMode.Create))
                {
                    encoder.Save(fileStream);
                }
            }
        }

        #endregion

        #region Main lines detection

        private void BtnOneLine_Click(object sender, RoutedEventArgs e)
        {
            if (IsThresholdValid())
            {
                ImgAccum.Source = _Image.HoughTransform(BinarizedSobelBitmap, tr).ToMat().ToBitmapSource();

                Bitmap img = _Image.GetBitmap();
                Graphics g = Graphics.FromImage(img);
                Pen pen = new Pen(Color.Red, 3);

                AutoThreshold(img, g, pen, 1);

                int dp = (int)Math.Round(Math.Sqrt(Math.Pow(_Image.GetBgrImage().Width, 2) + Math.Pow(_Image.GetBgrImage().Height, 2)));
                Point size = new Point(180, dp);

                ImgResult.Source = img.ToMat().ToBitmapSource();
            }
        }

        private void BtnTwoLines_Click(object sender, RoutedEventArgs e)
        {
            if (IsThresholdValid())
            {
                ImgAccum.Source = _Image.HoughTransform(BinarizedSobelBitmap, tr).ToMat().ToBitmapSource();

                Bitmap img = _Image.GetBitmap();
                Graphics g = Graphics.FromImage(img);
                Pen pen = new Pen(Color.Red, 3);

                AutoThreshold(img, g, pen, 2);

                int dp = (int)Math.Round(Math.Sqrt(Math.Pow(_Image.GetBgrImage().Width, 2) + Math.Pow(_Image.GetBgrImage().Height, 2)));
                Point size = new Point(180, dp);

                ImgResult.Source = img.ToMat().ToBitmapSource();
            }
        }

        private void BtnThreeLines_Click(object sender, RoutedEventArgs e)
        {
            if (IsThresholdValid())
            {
                ImgAccum.Source = _Image.HoughTransform(BinarizedSobelBitmap, tr).ToMat().ToBitmapSource();

                Bitmap img = _Image.GetBitmap();
                Graphics g = Graphics.FromImage(img);
                Pen pen = new Pen(Color.Red, 3);

                AutoThreshold(img, g, pen, 3);

                int dp = (int)Math.Round(Math.Sqrt(Math.Pow(_Image.GetBgrImage().Width, 2) + Math.Pow(_Image.GetBgrImage().Height, 2)));
                Point size = new Point(180, dp);

                ImgResult.Source = img.ToMat().ToBitmapSource();
            }
        }

        private void BtnFourLines_Click(object sender, RoutedEventArgs e)
        {
            if (IsThresholdValid())
            {
                ImgAccum.Source = _Image.HoughTransform(BinarizedSobelBitmap, tr).ToMat().ToBitmapSource();

                Bitmap img = _Image.GetBitmap();
                Graphics g = Graphics.FromImage(img);
                Pen pen = new Pen(Color.Red, 3);

                AutoThreshold(img, g, pen, 4);

                int dp = (int)Math.Round(Math.Sqrt(Math.Pow(_Image.GetBgrImage().Width, 2) + Math.Pow(_Image.GetBgrImage().Height, 2)));
                Point size = new Point(180, dp);

                ImgResult.Source = img.ToMat().ToBitmapSource();
            }
        }

        #endregion 
    }
}
