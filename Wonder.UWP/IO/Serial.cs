using System;
using Wonder.UWP.Constants;
using Wonder.UWP.Helpers;

namespace Wonder.UWP.IO
{
    internal static class Serial
    {
        /// <summary>
        /// 获取串口号列表
        /// </summary>
        /// <returns></returns>
        /// <exception cref="PlatformNotSupportedException"></exception>
        public static string[] GetPortNames()
        {
            Span<uint> portNumbers = stackalloc uint[16];
            uint portNumbersFound;
            int error;
            try
            {
                error = Win32Helper.GetCommPorts(portNumbers, out portNumbersFound);
            }
            catch (Exception ex) when (ex is EntryPointNotFoundException || ex is DllNotFoundException)
            {
                throw new PlatformNotSupportedException();
            }
            while (error == Win32Errors.ERROR_MORE_DATA)
            {
                portNumbers = new uint[portNumbersFound];
                error = Win32Helper.GetCommPorts(portNumbers, out portNumbersFound);
            }
            if (error != Win32Errors.ERROR_SUCCESS)
            {
                return Array.Empty<string>();
            }
            var portNames = new string[portNumbersFound];
            for (int i = 0; i < portNumbersFound; i++)
            {
                portNames[i] = "COM" + portNumbers[i].ToString();
            }
            return portNames;
        }
    }
}
