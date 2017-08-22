using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Castalia.Media.Burner.ExtensionMethods
{
    public static class BitmapSourceExtension
    {
        public static BitmapSource ToBitmapSource(this Image source)
        {
            var bitmap = new Bitmap(source);
            var bitSrc = bitmap.ToBitmapSource();
            bitmap.Dispose();
            return bitSrc;
        }

        public static BitmapSource ToBitmapSource(this Bitmap source)
        {
            BitmapSource bitSrc;
            var hBitmap = source.GetHbitmap();
            try
            {
                bitSrc = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero,
                                                               Int32Rect.Empty,
                                                               BitmapSizeOptions.FromEmptyOptions
                                                                   ());
            }
            catch (Win32Exception)
            {
                bitSrc = null;
            }
            finally
            {
                NativeMethods.DeleteObject(hBitmap);
            }
            return bitSrc;
        }
    }

    internal static class NativeMethods
    {
        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)] 
        internal static extern bool DeleteObject(IntPtr hObject);
    }
}
