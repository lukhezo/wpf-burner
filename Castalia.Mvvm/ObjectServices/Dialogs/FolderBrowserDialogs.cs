using System;
using System.Windows.Forms;

namespace Castalia.Mvvm.ObjectServices.Dialogs
{
    internal class FolderBrowserDialogService : IFolderBrowserDialogService
    {
        private readonly FolderBrowserDialog dialog;

        public FolderBrowserDialogService()
        {
            dialog = new FolderBrowserDialog
                         {ShowNewFolderButton = true, RootFolder = Environment.SpecialFolder.Desktop};
        }

        DialogResult IFolderBrowserDialogService.ShowDialog()
        {
            return dialog.ShowDialog();
        }
        string IFolderBrowserDialogService.SelectedPath()
        {
            return dialog.SelectedPath;
        }

    }
}
