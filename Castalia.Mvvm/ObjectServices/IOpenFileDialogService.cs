namespace Castalia.Mvvm.ObjectServices
{
    public interface IOpenFileDialogService
    {
        bool? ShowDialog();
        string[] FileNames();
    } 
}
