﻿using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

using ScottPlot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace LineDetection
{
    public class Image
    {
        public string _bgrImagePath;
        public Image<Bgr, byte> _bgrImage;
        public Image<Gray, byte> _grayImage;

        public bool isGray = false;

        LineDetectionUtils utils = new LineDetectionUtils();

        public List<Point> localMaxima = new List<Point>();

        public Image(string path)
        {
            _bgrImage = new Image<Bgr, byte>(path);
            _bgrImagePath = path;

            localMaxima = new List<Point>();
        }

        #region histogram

        public void Histogram()
        {
            int[] histogramValues = new int[256];

            for (int x = 0; x < _bgrImage.Height; x++)
            {
                for (int y = 0; y < _bgrImage.Width; y++)
                {
                    Bgr pixel = _bgrImage[x, y];
                    int gray = (int)Math.Round(((pixel.Red * 0.299) + (pixel.Green * 0.587) + (pixel.Blue * 0.114)));
                    histogramValues[gray]++;
                }
            }

            double[] histogramValuesDouble = histogramValues.Select(v => (double)v).ToArray();
            double[] positions = Enumerable.Range(0,256).Select(i => (double)i).ToArray();

            Histogram histogramWindow = new Histogram();
            histogramWindow.Title = _bgrImagePath + " Histogram";
            
            var wpfPlot = new WpfPlot();
            wpfPlot.Plot.PlotBar(positions, histogramValuesDouble);
            wpfPlot.Plot.Title("");
            wpfPlot.Plot.XLabel("Grayscale value");
            wpfPlot.Plot.YLabel("Count");

            histogramWindow.histogramPlotContainer.Children.Add(wpfPlot);
            histogramWindow.Show();
            wpfPlot.Refresh();
        }

        #endregion

        #region convert
        public Image<Gray, byte> BGR2GRAY()
        {
            _grayImage = _bgrImage.Convert<Gray, byte>();
            return _bgrImage.Convert<Gray, byte>();
        }

        #endregion

        #region Edge detection

        public Image<Gray,byte> ApplySobelEdgeDetection()
        {
            Mat image = _bgrImage.Mat;

            Mat gaussianBlur = new Mat();
            Mat sobelX = new Mat();
            Mat sobelY = new Mat();
            Mat sobelXY = new Mat();

            image.CopyTo(sobelX);
            image.CopyTo(sobelY);
            image.CopyTo(sobelXY);

            CvInvoke.GaussianBlur(image, gaussianBlur, new System.Drawing.Size(3, 3), 5.0);

            CvInvoke.Sobel(gaussianBlur, sobelX, Emgu.CV.CvEnum.DepthType.Default, 1, 0, 5);
            CvInvoke.Sobel(gaussianBlur, sobelY, Emgu.CV.CvEnum.DepthType.Default, 0, 1, 5);
            CvInvoke.Sobel(gaussianBlur, sobelXY, Emgu.CV.CvEnum.DepthType.Default, 1, 1, 5);

            _bgrImage = sobelXY.ToImage<Bgr, byte>();
            return sobelXY.ToImage<Gray,byte>();
        }

        public Image<Gray, byte> ApplyCannyEdgeDetection(double lowerThreshold, double upperThreshold)
        {
            Mat image = _bgrImage.Mat;
            Mat gaussianBlur = new Mat();
            Mat cannyMat = new Mat();

            CvInvoke.GaussianBlur(image, gaussianBlur, new Size(3, 3), 5.0);

            var average = image.ToImage<Gray, byte>().GetAverage();

            CvInvoke.Canny(gaussianBlur, cannyMat, lowerThreshold, upperThreshold, 3);

            _bgrImage = cannyMat.ToImage<Bgr, byte>();
            return cannyMat.ToImage<Gray, byte>();
        }

        public Image<Gray, byte> ApplyCannyEdgeDetectionAuto()
        {
            Mat image = _bgrImage.Mat;
            Mat gaussianBlur = new Mat();
            Mat cannyMat = new Mat();

            CvInvoke.GaussianBlur(image, gaussianBlur, new Size(3, 3), 5.0);

            var average = image.ToImage<Gray, byte>().GetAverage();

            var lowerThreshold = Math.Max(0, (1.0 - 0.33) * average.Intensity);
            var upperThreshold = Math.Min(255, (1.0 + 0.33) * average.Intensity);

            CvInvoke.Canny(gaussianBlur, cannyMat, lowerThreshold, upperThreshold, 3);

            _bgrImage = cannyMat.ToImage<Bgr, byte>();
            return cannyMat.ToImage<Gray, byte>();
        }


        public Image<Gray,byte> ApplyPrewittEdgeDetection()
        {
            Mat image = _bgrImage.Mat;

            Mat PrewittX = new Mat();
            Mat PrewittY = new Mat();

            CvInvoke.Sobel(image, PrewittX, DepthType.Cv64F, 1, 0, 3);
            CvInvoke.Sobel(image, PrewittY, DepthType.Cv64F, 0, 1, 3);

            Mat absPrewittX = new Mat();
            Mat absPrewittY = new Mat();
            
            CvInvoke.ConvertScaleAbs(PrewittX, absPrewittX, 1, 0);
            CvInvoke.ConvertScaleAbs(PrewittY, absPrewittY, 1, 0);

            Mat PrewittEdges = new Mat();
            CvInvoke.AddWeighted(absPrewittX, 0.5, absPrewittY, 0.5, 0, PrewittEdges);

            _bgrImage = PrewittEdges.ToImage<Bgr, byte>();
            return PrewittEdges.ToImage<Gray, byte>();
        }

        public Image<Gray, byte> ApplyLaplacianEdgeDetection()
        {
            Mat image = _bgrImage.Mat;

            Mat LaplacianEdges = new Mat();
            CvInvoke.Laplacian(image, LaplacianEdges, DepthType.Cv64F);

            Mat absLaplacianEdges = new Mat();
            CvInvoke.ConvertScaleAbs(LaplacianEdges, absLaplacianEdges, 1, 0);

            _bgrImage = absLaplacianEdges.ToImage<Bgr, byte>();
            return absLaplacianEdges.ToImage<Gray, byte>();
        }

        #endregion

        #region Hough

        public int[,] accum;

        public Bitmap Sobel(Bitmap src)
        {
            Bitmap dst = new Bitmap(src.Width, src.Height);
            
            int[,] dx = { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            int[,] dy = { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };

            GrayScale(src);

            int sumX, sumY, sum;
            for (int y = 0; y < src.Height - 1; y++)
                for (int x = 0; x < src.Width - 1; x++)
                {
                    sumX = sumY = 0;
                    if (y == 0 || y == src.Height - 1) sum = 0;
                    else if (x == 0 || x == src.Width - 1) sum = 0;
                    else
                    {
                        for (int i = -1; i < 2; i++)
                            for (int j = -1; j < 2; j++)
                            {
                                int c = src.GetPixel(x + i, y + j).R;
                                sumX += c * dx[i + 1, j + 1];
                                sumY += c * dy[i + 1, j + 1];
                            }
                        sum = (int)Math.Sqrt(Math.Pow(sumX, 2) + Math.Pow(sumY, 2));
                    }
                    if (sum > 255) sum = 255;
                    else if (sum < 0) sum = 0;
                    dst.SetPixel(x, y, Color.FromArgb(255, sum, sum, sum));
                }
            return dst;
        }

        public void GrayScale(Bitmap img)
        {
            for (int y = 0; y < img.Height; y++)
                for (int x = 0; x < img.Width; x++)
                {
                    Color c = img.GetPixel(x, y);

                    int px = (int)((c.R * 0.3) + (c.G * 0.59) + (c.B * 0.11));
                    img.SetPixel(x, y, Color.FromArgb(c.A, px, px, px));
                }
        }

        public Bitmap HoughTransform(Bitmap img, int tr, int maximasToFind, bool showMaximas = true)
        {
            Point Size = new Point();
            int mang = 180;

            Size.Y = (int)Math.Round(Math.Sqrt(Math.Pow(img.Width, 2) + Math.Pow(img.Height, 2)));
            Size.X = mang;
            accum = new int[(int)Size.Y, Size.X];

            double dt = Math.PI / mang;

            for(int y = 0; y < img.Height; y++)
                for(int x = 0; x < img.Width; x++)
                    if(img.GetPixel(x,y).R == 255)
                    {
                        for(int i = 0; i < mang; i++)
                        {
                            int row = (int)Math.Round(x * Math.Cos(dt * i) + y * Math.Sin(dt * i));
                            if (row < Size.Y && row > 0) accum[row, i]++;
                        }
                    }
            int amax = AccumMax(Size);

            //TEST - do usunięcia
            //using (StreamWriter writer = new StreamWriter("C:\\Users\\osaja\\Desktop\\test.txt"))
            //{
            //    int rows = accum.GetLength(0);
            //    int cols = accum.GetLength(1);

            //    for (int i = 0; i < rows; i++)
            //    {
            //        string[] rowValues = new string[cols];
            //        for (int j = 0; j < cols; j++)
            //        {
            //            rowValues[j] = accum[i, j].ToString();
            //        }
            //        writer.WriteLine(string.Join(",", rowValues));
            //    }
            //}

            if (showMaximas) localMaxima = FindLocalMaxima(Size, tr, maximasToFind);

            if(amax != 0)
            {
                img = new Bitmap(Size.X, Size.Y);
                Normalize(Size, amax);

                for(int y = 0; y < Size.Y; y++)
                {
                    for(int x = 0; x < Size.X; x++)
                    {
                        int c = accum[y, x];
                        img.SetPixel(x, y, Color.FromArgb(c, c, c));
                    }
                }

                // zaznaczenie na czerwono znalezionych maksimów lokalnych na przestrzeni akumulatorów
                if (showMaximas)
                {
                    using (Graphics g = Graphics.FromImage(img))
                    {
                        foreach (var max in localMaxima)
                        {
                            int squareSize = 5;

                            int x = max.X - squareSize / 2;
                            int y = max.Y - squareSize / 2;

                            g.FillRectangle(Brushes.Red, x, y, squareSize / 2 + 1, squareSize);
                        }
                    }
                }
            }
            return img;
        }

        public int AccumMax(Point Size)
        {
            int amax = 0;
            for(int y = 0; y < Size.Y; y++)
            {
                for(int x = 0; x < Size.X; x++)
                {
                    if (accum[y, x] > amax) amax = accum[y, x];
                }
            }
            return amax;
        }

        public void Normalize(Point Size, int amax)
        {
            for(int y = 0; y < Size.Y; y++)
            {
                for(int x = 0; x < Size.X; x++)
                {
                    int c = (int)(((double)accum[y, x] / (double)amax) * 255.0);
                    accum[y, x] = c;
                }
            }
        }

        public Point SearchLine(Point size, int tr, HashSet<Point> detectedLines = null, int tolerance = 5)
        {
            int max = 0;
            Point pt = new Point(-1,-1);

            for(int y = 0; y < size.Y; y++)
                for(int x = 0; x < size.X; x++)
                {
                    if (max < accum[y, x])
                    {
                        if (detectedLines != null && detectedLines.Any(d => Math.Abs(d.X - x) < tolerance && Math.Abs(d.Y - y) < tolerance))
                            continue;

                        max = accum[y, x];
                        pt.X = x; 
                        pt.Y = y;
                    }
                }

            if (max < tr) pt.X = -1;
            else detectedLines?.Add(pt);

            return pt;
        }

        public List<Point> FindLocalMaxima(Point size, int threshold, int maximasToFind)
        {
            List<potentialMaximas> maxima = new List<potentialMaximas>();

            for (int y = 1; y < size.Y - 1; y++)
            {
                for (int x = 1; x < size.X - 1; x++)
                {
                    int value = accum[y, x];

                    // Sprawdzanie, czy wartość przekracza próg
                    if (value < threshold) continue;

                    // Sprawdzanie sąsiedztwa 5x5
                    bool isLocalMax = true;
                    for (int dy = -2; dy <= 2; dy++)
                    {
                        for (int dx = -2; dx <= 2; dx++)
                        {
                            if (dy == 0 && dx == 0) continue;
                            try
                            {
                                if (accum[y + dy, x + dx] > value)
                                {
                                    isLocalMax = false;
                                    break;
                                }
                            }
                            catch(Exception ex)
                            {
                                break;
                            }
                            
                        }
                        if (!isLocalMax) break;
                    }

                    if (isLocalMax) maxima.Add(new potentialMaximas(value, new Point(x, y)));
                    //if(maxima.Count == maximasToFind) return GetTopPoints(maxima, maximasToFind);
                }
            }

            if(maxima.Count == 0) 
            {
                utils.Message("Local maxima were not found.\nLower the threshold to find them.");
            }

            if(maxima.Count < maximasToFind)
            {
                utils.Message($"{maximasToFind} local maximas were not found.\nFound {maxima.Count} local maximas.\nLower the threshold to find more local maximas");
            }

            return GetTopPoints(maxima, maximasToFind);
        }

        public class potentialMaximas
        {
            public int Value { get; set; }
            public Point Point { get; set; }

            public potentialMaximas(int value, Point point) 
            { 
                Value = value; 
                Point = point;
            }
        }

        public List<Point> GetTopPoints(List<potentialMaximas> maximas, int top)
        {
            return maximas
                .OrderByDescending(m => m.Value)
                .Take(top)
                .Select(m => m.Point)
                .ToList();
        }

        #endregion

        #region get/set
        public string GetPath() { return _bgrImagePath; }

        public Image<Bgr, byte> GetBgrImage() { return _bgrImage; }

        public Bitmap GetBitmap() { return _bgrImage.ToBitmap(); }

        #endregion
    }
}
