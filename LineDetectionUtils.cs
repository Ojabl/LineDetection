using System.Windows;

namespace LineDetection
{
    public class LineDetectionUtils
    {
        public void ErrorMessage(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        public MessageBoxResult WarningMessage(string message)
        {
            MessageBoxResult choice = MessageBox.Show(message, "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            return choice;
        }
    }
}
