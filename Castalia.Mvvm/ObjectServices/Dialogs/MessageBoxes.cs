using System.Windows;

namespace Castalia.Mvvm.ObjectServices.Dialogs
{
    /// <summary>
    /// A service that shows message boxes.
    /// </summary>
    internal class MessageBoxService : IMessageBoxService
    {
        MessageBoxResult IMessageBoxService.Show(
            string text,
            string caption,
            MessageBoxButton buttons,
            MessageBoxImage image)
        {
            return MessageBox.Show(text, caption, buttons, image);
        }
    }
}