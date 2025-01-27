using Emgu.CV;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        public int tolerance;
        
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
                ImgAccum.Source = _Image.HoughTransform(BinarizedSobelBitmap, tr, 100, false).ToMat().ToBitmapSource();
                if (_Image.accum != null) BtnShowAccum.IsEnabled = true;

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
                    try
                    {
                        tolerance = Int32.Parse(TxtTolerance.Text);
                    }
                    catch(Exception ex)
                    {
                        utils.ErrorMessage("Tolerance value must be filled with a natural number!");
                        return;
                    }

                    while (true)
                    {
                        try 
                        {
                            Point testpt = _Image.SearchLine(size, tr, detectedLines, tolerance);
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
            try
            {
                tolerance = Int32.Parse(TxtTolerance.Text);
            }
            catch (Exception ex)
            {
                utils.ErrorMessage("Tolerance value must be filled with a natural number!");
                return 0 ;
            }

            tr = 300;
            HashSet<Point> detectedLines = new HashSet<Point>();
            int dp = (int)Math.Round(Math.Sqrt(Math.Pow(_Image.GetBgrImage().Width, 2) + Math.Pow(_Image.GetBgrImage().Height, 2)));
            Point size = new Point(180, dp);
            
            while (tr > 0)
            {
                while (true)
                {
                    try
                    {
                        Point testpt = _Image.SearchLine(size, tr, detectedLines, tolerance);
                        if (testpt.X == -1) break;

                        int y1 = (int)((-Math.Cos(testpt.X * (Math.PI / 180)) / Math.Sin(testpt.X * (Math.PI / 180))) * 0 + (double)testpt.Y / Math.Sin(testpt.X * (Math.PI / 180)));
                        int y2 = (int)((-Math.Cos(testpt.X * (Math.PI / 180)) / Math.Sin(testpt.X * (Math.PI / 180))) * _Image.GetBgrImage().Width + (double)testpt.Y / Math.Sin(testpt.X * (Math.PI / 180)));
                        g.DrawLine(pen, 0, y1, _Image.GetBgrImage().Width, y2);
                    }
                    catch(OverflowException ex)
                    {
                        continue;
                    }
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

        #region Save and Clear

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

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            //ponowne wygenerowanie przestrzeni akumulatorów obrazu
            ImgAccum.Source = _Image.HoughTransform(BinarizedSobelBitmap, tr, 100, false).ToMat().ToBitmapSource();

            //ponowne pokazanie obrazu wynikowego bez narysowanych linii
            ImgResult.Source = _Image.GetBgrImage().ToUMat().ToBitmapSource();
        }

        #endregion

        #region Main lines detection

        private void BtnMainLines_Click(object sender, RoutedEventArgs e)
        {
            if (IsThresholdValid())
            {
                if (TbTr.Text == "Auto")
                {
                    utils.Message("Finding main lines cannot be used with 'Auto' threshold value.");
                    return;
                }

                int maximasToFind = -1;

                try
                {
                    maximasToFind = Convert.ToInt32(TxtMainLines.Text);
                }
                catch (Exception ex)
                {
                    utils.ErrorMessage("Number of main lines needs to be a number.");
                    return;
                }

                ImgAccum.Source = _Image.HoughTransform(BinarizedSobelBitmap, tr, maximasToFind).ToMat().ToBitmapSource();

                Bitmap img = _Image.GetBitmap();
                Graphics g = Graphics.FromImage(img);
                Pen pen = new Pen(Color.Red, 3);

                List<Point> localMaxima = _Image.localMaxima;

                foreach (Point p in localMaxima)
                {
                    int y1 = (int)((-Math.Cos(p.X * (Math.PI / 180)) / Math.Sin(p.X * (Math.PI / 180))) * 0 + (double)p.Y / Math.Sin(p.X * (Math.PI / 180)));
                    int y2 = (int)((-Math.Cos(p.X * (Math.PI / 180)) / Math.Sin(p.X * (Math.PI / 180))) * _Image.GetBgrImage().Width + (double)p.Y / Math.Sin(p.X * (Math.PI / 180)));

                    g.DrawLine(pen, 0, y1, _Image.GetBgrImage().Width, y2);
                }

                ImgResult.Source = img.ToMat().ToBitmapSource();
            }
        }

        #endregion

        #region Show Accumulator

        private void BtnShowAccum_Click(object sender, RoutedEventArgs e)
        {
            if(_Image.accum.GetLength(0) > 200)
            {
                MessageBoxResult decision = utils.WarningMessage($"Table of accumulators of this image is really big {_Image.accum.GetLength(0)}x{_Image.accum.GetLength(1)}\nAre you sure you want to create such a big table?");
                
                if(decision == MessageBoxResult.No)
                {
                    return;
                }
            }

            // Tworzenie nowego okna
            Window accumWindow = new Window
            {
                Title = "Accumulator Values",
                Width = 800, // Szerokość okna
                Height = 600 // Wysokość okna
            };

            // Tworzenie DataGrid
            DataGrid dataGrid = new DataGrid
            {
                AutoGenerateColumns = false,
                CanUserAddRows = false,
                HeadersVisibility = DataGridHeadersVisibility.None, // Ukrywanie nagłówków kolumn
                IsReadOnly = true
            };

            // Dodanie kolumn do DataGrid
            int columns = _Image.accum.GetLength(1);
            for (int i = 0; i < columns; i++)
            {
                dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = $"Col {i}",
                    Binding = new System.Windows.Data.Binding($"[{i}]"),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star)
                });
            }

            // Dodanie wierszy do DataGrid
            int rows = _Image.accum.GetLength(0);
            for (int y = 0; y < rows; y++)
            {
                var row = new int[columns];
                for (int x = 0; x < columns; x++)
                {
                    row[x] = _Image.accum[y, x];
                }
                dataGrid.Items.Add(row);
            }

            // Dodanie DataGrid do ScrollViewer dla przewijania
            ScrollViewer scrollViewer = new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = dataGrid
            };

            // Ustawienie zawartości okna
            accumWindow.Content = scrollViewer;

            // Wyświetlenie okna
            accumWindow.ShowDialog();
        }


        #endregion
    }
}
