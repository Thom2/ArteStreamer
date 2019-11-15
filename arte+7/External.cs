using System;
using System.Runtime.InteropServices;
using System.Text;

namespace arte_7
{
    public enum SpecialFolderType
    {
        MyVideo = 0x000e        // "My Videos" folder
    }

    public static class External
    {
        [DllImport("shell32.dll")]
        static extern int SHGetFolderPath(IntPtr hwndOwner, int nFolder, IntPtr hToken,
           uint dwFlags, [Out] StringBuilder pszPath);

        static public string GetSpecialFolderPath(SpecialFolderType spfolder)
        {
            const int MaxPath = 260;
            StringBuilder sb = new StringBuilder(MaxPath);
            SHGetFolderPath(IntPtr.Zero, (int)spfolder, IntPtr.Zero, 0, sb);
            return sb.ToString();
        }
    }
}
