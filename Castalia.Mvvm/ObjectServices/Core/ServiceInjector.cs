using Castalia.Mvvm.ObjectServices;
using Castalia.Mvvm.ObjectServices.Dialogs;

namespace Castalia.Mvvm.Services.Core
{
    public static class ServiceInjector
    {
        // Loads service objects into the ServiceContainer on startup.
        public static void InjectServices()
        {
            ServiceContainer.Instance.AddService<IMessageBoxService>(new MessageBoxService());
            ServiceContainer.Instance.AddService<IOpenFileDialogService>(new OpenFileDialogService());
            ServiceContainer.Instance.AddService<IFolderBrowserDialogService>(new FolderBrowserDialogService());
        }
    }
}