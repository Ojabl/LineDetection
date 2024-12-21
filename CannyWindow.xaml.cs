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
    /// <summary>
    /// Interaction logic for CannyWindow.xaml
    /// </summary>
    public partial class CannyWindow : Window
    {
        public int _LowerThreshold { get; set; }
        public int _UpperThreshold { get; set; }

        public CannyWindow()
        {
            InitializeComponent();

            _LowerThreshold = 0;
            _UpperThreshold = 0;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _LowerThreshold = int.Parse(lowThreshold.Text);
                _UpperThreshold = int.Parse(highThreshold.Text);

                this.Close();
            }
            catch(Exception ex) 
            {
                MessageBox.Show("low and high threshold must be numberss");
            }
        }

        public int GetLowerThreshold()
        {
            return _LowerThreshold;
        }

        public int GetUpperThreshold()
        {
            return _UpperThreshold;
        }
    }
}
