using System.Windows;

namespace Castalia.Mvvm.ObjectServices
{
    public interface IMessageBoxService
    {
        MessageBoxResult Show(string message, string title, MessageBoxButton buttons, MessageBoxImage image);
    }
}