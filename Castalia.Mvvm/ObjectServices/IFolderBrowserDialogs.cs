using System.Windows.Forms;

namespace Castalia.Mvvm.ObjectServices
{
    public interface IFolderBrowserDialogService
    {
        DialogResult ShowDialog();
        string SelectedPath();
    }
}
