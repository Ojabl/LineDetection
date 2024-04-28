using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.XImgproc;
using OpenCvSharp;

using ScottPlot;
using System;
using System.Drawing;
using System.Linq;
using Mat = Emgu.CV.Mat;

namespace LineDetection
{
    public class Image
    {
        public string _bgrImagePath;
        public Image<Bgr, byte> _bgrImage;
        public Image<Gray, byte> _grayImage;

        public bool isGray = false;

        public Image(string path)
        {
            _bgrImage = new Image<Bgr, byte>(path);
            _bgrImagePath = path;
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
            histogramWindow.Title = "Histogram " + _bgrImagePath;
            
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
            // https://www.youtube.com/watch?v=wuQsW-LZ3kw

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

        public Image<Gray, byte> ApplyCannyEdgeDetection()
        {
            // https://www.youtube.com/watch?v=wuQsW-LZ3kw

            Mat image = _bgrImage.Mat;
            Mat gaussianBlur = new Mat();
            Mat cannyMat = new Mat();

            CvInvoke.GaussianBlur(image, gaussianBlur, new System.Drawing.Size(3,3), 5.0);

            var average = image.ToImage<Gray, byte>().GetAverage();
            
            var lowerThreshold = Math.Max(0, (1.0 - 0.33) * average.Intensity);
            var upperThreshold = Math.Min(255, (1.0 + 0.33) * average.Intensity);

            CvInvoke.Canny(gaussianBlur, cannyMat, lowerThreshold, upperThreshold, 3);

            _bgrImage = cannyMat.ToImage<Bgr, byte>();
            return cannyMat.ToImage<Gray, byte>();
        }

        public Image<Gray,byte> ApplyPrewittEdgeDetection()
        {
            // ChatGpt

            Mat image = _bgrImage.Mat;

            Mat PrewittX = new Mat();
            Mat PrewittY = new Mat();

            CvInvoke.Sobel(image, PrewittX, Emgu.CV.CvEnum.DepthType.Cv64F, 1, 0, 3);
            CvInvoke.Sobel(image, PrewittY, Emgu.CV.CvEnum.DepthType.Cv64F, 0, 1, 3);

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
            // ChatGpt

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

        public void GrayScale(Bitmap img)
        {
            for (int y = 0; y < img.Height; y++)
                for (int x = 0; x < img.Width; x++)
                {
                    Color c = img.GetPixel(x, y);
                    
                    // wzór na skalę szarości
                    int px = (int)((c.R * 0.3) + (c.G * 0.59) + (c.B * 0.11));
                    img.SetPixel(x, y, Color.FromArgb(c.A, px, px, px));
                }
        }

        public Bitmap Sobel(Bitmap src)
        {
            Bitmap dst = new Bitmap(src.Width, src.Height);
            
            // Operator Sobela
            int[,] dx = { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            int[,] dy = { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };

            // Konwertuj na skalę szarości 
            GrayScale(src);

            int sumX, sumY, sum;
            // Przechodzimy przez cały obraz
            for (int y = 0; y < src.Height - 1; y++)
                for (int x = 0; x < src.Width - 1; x++)
                {
                    sumX = sumY = 0;
                    if (y == 0 || y == src.Height - 1) sum = 0;
                    else if (x == 0 || x == src.Width - 1) sum = 0;
                    else
                    {
                        // Cykl splotu za pomocą operatora Sobela
                        for (int i = -1; i < 2; i++)
                            for (int j = -1; j < 2; j++)
                            {
                                // Zapisuję wartość piksela
                                int c = src.GetPixel(x + i, y + j).R;
                                // Znajdź sumę iloczynów piksela przez wartość z macierzy X
                                sumX += c * dx[i + 1, j + 1];
                                // i suma iloczynów piksela przez wartość z macierzy Y
                                sumY += c * dy[i + 1, j + 1];
                            }
                        // Znajdź przybliżoną wartość gradientu
                        // sum = Math.Abs(sumX) + Math.Abs(sumY);
                        sum = (int)Math.Sqrt(Math.Pow(sumX, 2) + Math.Pow(sumY, 2));
                    }
                    // Przeprowadź normalizację
                    if (sum > 255) sum = 255;
                    else if (sum < 0) sum = 0;
                    // Zapisz wynik na obrazie wejściowym
                    dst.SetPixel(x, y, Color.FromArgb(255, sum, sum, sum));
                }
            //Binarization(dst);
            return dst;
        }

        #endregion

        #region get/set
        public string GetPath() { return _bgrImagePath; }

        public Image<Bgr, byte> GetBgrImage() { return _bgrImage; }

        public Bitmap GetBitmap() { return _bgrImage.ToBitmap(); }

        #endregion
    }
}
