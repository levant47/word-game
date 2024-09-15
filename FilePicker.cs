public static class FilePicker
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct OpenFileName
    {
        public int lStructSize;
        public IntPtr hwndOwner;
        public IntPtr hInstance;
        public string lpstrFilter;
        public string lpstrCustomFilter;
        public int nMaxCustFilter;
        public int nFilterIndex;
        public string lpstrFile;
        public int nMaxFile;
        public string lpstrFileTitle;
        public int nMaxFileTitle;
        public string lpstrInitialDir;
        public string lpstrTitle;
        public int Flags;
        public short nFileOffset;
        public short nFileExtension;
        public string lpstrDefExt;
        public IntPtr lCustData;
        public IntPtr lpfnHook;
        public string lpTemplateName;
        public IntPtr pvReserved;
        public int dwReserved;
        public int flagsEx;
    }

    public static string? Open(string[] fileExtensions)
    {
        var openFileName = new OpenFileName();
        openFileName.lStructSize = Marshal.SizeOf(openFileName);
        openFileName.lpstrFilter = $"{string.Join(" ", fileExtensions)}\0{string.Join(";", fileExtensions)}\0";
        openFileName.lpstrFile = new(new char[256]);
        openFileName.nMaxFile = openFileName.lpstrFile.Length;
        openFileName.lpstrFileTitle = new(new char[64]);
        openFileName.nMaxFileTitle = openFileName.lpstrFileTitle.Length;
        openFileName.lpstrTitle = "Open File Dialog...";
        return GetOpenFileName(ref openFileName) ? openFileName.lpstrFile : null;
    }

    [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool GetOpenFileName(ref OpenFileName ofn);
}
