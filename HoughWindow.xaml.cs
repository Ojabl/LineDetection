using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LineDetection
{
    public partial class HoughWindow : Window
    {
        Image _Image { get; set; }

        public HoughWindow(Image inputImage)
        {
            InitializeComponent();
            _Image = inputImage;
            Title = _Image.GetPath();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
    }
}
