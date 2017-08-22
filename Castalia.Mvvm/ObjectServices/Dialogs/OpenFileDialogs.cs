using Microsoft.Win32;

namespace Castalia.Mvvm.ObjectServices.Dialogs
{
    internal class OpenFileDialogService : IOpenFileDialogService
    {
        private readonly OpenFileDialog dialog; 
        
        public OpenFileDialogService()
        {
            dialog = new OpenFileDialog { Multiselect = true, Filter = @"All files (*.*)|*.*", };
        }

        bool? IOpenFileDialogService.ShowDialog()
        {
            return dialog.ShowDialog();
        }
        string[] IOpenFileDialogService.FileNames()
        {
            return dialog.FileNames;
        }
    }
}
