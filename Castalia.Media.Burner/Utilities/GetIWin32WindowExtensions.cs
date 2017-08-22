namespace Castalia.Media.Burner.Utilities
{  
    /// <summary>
    /// Extionsion method that return the Win32 window for cunsuption in WPF applications
    /// that require access to the window
    /// </summary>
    public static class GetIWin32WindowExtensions
    {
        /// <summary>
        /// Static extension method that return an IntPtr as a handle
        /// </summary>
        /// <param name="visual">this System.Windows.Media.Visual</param>
        /// <returns> System.Windows.Forms.IWin32Window</returns>
        public static System.Windows.Forms.IWin32Window GetIWin32Window(this System.Windows.Media.Visual visual)
        {
            var source = System.Windows.PresentationSource.FromVisual(visual) as System.Windows.Interop.HwndSource;
            System.Windows.Forms.IWin32Window win = new OldWindow(source.Handle);
            return win;
        }

        /// <summary>
        /// Private class that implements IWin32Window
        /// </summary>
        private class OldWindow : System.Windows.Forms.IWin32Window
        {
            /// <summary>
            /// Readonly IntPtr used as the handle
            /// </summary>
            private readonly System.IntPtr handle;
            /// <summary>
            /// Constructor for the OldWindow
            /// </summary>
            /// <param name="handle"></param>
            public OldWindow(System.IntPtr handle)
            {
                this.handle = handle;
            }

            #region IWin32Window Members
            /// <summary>
            /// The IWin32Window.Handle
            /// </summary>
            System.IntPtr System.Windows.Forms.IWin32Window.Handle
            {
                get { return handle; }
            }
            #endregion
        }
    }
}
