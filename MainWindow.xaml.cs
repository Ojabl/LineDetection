using Microsoft.Win32;
using System.Windows;

namespace LineDetection
{
    public partial class MainWindow : Window
    {
        public Image _Image { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void mOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp";

            if(ofd.ShowDialog() == true)
            {
                _Image = new Image(ofd.FileName);
                OpenedImage openedImageWindow = new OpenedImage(_Image);
                openedImageWindow.Show();
            }
        }

        private void mInfo_Click(object sender, RoutedEventArgs e) //TODO
        {
            string messageBoxContent = @"Praca inżynierska - Odnajdywanie Linii na cyfrowych obrazach mikroskopowych

                                        Autor: Oskar Jabłoński
                                        Promotor: dr. inż. Łukasz Roszkowiak";
            MessageBox.Show(messageBoxContent, "Info", MessageBoxButton.OK);
        }
    }
}
