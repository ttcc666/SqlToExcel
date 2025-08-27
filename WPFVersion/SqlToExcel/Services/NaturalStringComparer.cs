
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;

namespace SqlToExcel.Services
{
    [SuppressUnmanagedCodeSecurity]
    internal static class SafeNativeMethods
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern int StrCmpLogicalW(string psz1, string psz2);
    }

    public sealed class NaturalStringComparer : IComparer<string>
    {
        public int Compare(string? a, string? b)
        {
            if (a == null && b == null) return 0;
            if (a == null) return -1;
            if (b == null) return 1;
            return SafeNativeMethods.StrCmpLogicalW(a, b);
        }
    }
}
