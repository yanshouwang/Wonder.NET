using System.Runtime.InteropServices;
using System;
using Wonder.UWP.Constants;

namespace Wonder.UWP.Helpers
{
    internal static class Win32Helper
    {
        [DllImport(Win32Libraries.CoreComm_L1_1_2, SetLastError = true)]
        private static unsafe extern int GetCommPorts(
            uint* lpPortNumbers,
            uint uPortNumbersCount,
            out uint puPortNumbersFound);

        public static unsafe int GetCommPorts(
            Span<uint> portNumbers,
            out uint portNumbersFound)
        {
            fixed (uint* portNumbersBuffer = &MemoryMarshal.GetReference(portNumbers))
            {
                return GetCommPorts(portNumbersBuffer, (uint)portNumbers.Length, out portNumbersFound);
            }
        }
    }
}
