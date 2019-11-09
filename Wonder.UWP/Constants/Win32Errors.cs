using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wonder.UWP.Constants
{
    /// <summary>
    /// <see cref="https://github.com/dotnet/corefx/blob/master/src/Common/src/CoreLib/Interop/Windows/Interop.Errors.cs"/>
    /// </summary>
    internal static class Win32Errors
    {
        public const int ERROR_SUCCESS = 0x0;
        public const int ERROR_FILE_NOT_FOUND = 0x2;
        public const int ERROR_PATH_NOT_FOUND = 0x3;
        public const int ERROR_ACCESS_DENIED = 0x5;
        public const int ERROR_INVALID_HANDLE = 0x6;
        public const int ERROR_NOT_ENOUGH_MEMORY = 0x8;
        public const int ERROR_INVALID_DRIVE = 0xF;
        public const int ERROR_NO_MORE_FILES = 0x12;
        public const int ERROR_NOT_READY = 0x15;
        public const int ERROR_SHARING_VIOLATION = 0x20;
        public const int ERROR_HANDLE_EOF = 0x26;
        public const int ERROR_NOT_SUPPORTED = 0x32;
        public const int ERROR_FILE_EXISTS = 0x50;
        public const int ERROR_INVALID_PARAMETER = 0x57;
        public const int ERROR_BROKEN_PIPE = 0x6D;
        public const int ERROR_INSUFFICIENT_BUFFER = 0x7A;
        public const int ERROR_INVALID_NAME = 0x7B;
        public const int ERROR_BAD_PATHNAME = 0xA1;
        public const int ERROR_ALREADY_EXISTS = 0xB7;
        public const int ERROR_ENVVAR_NOT_FOUND = 0xCB;
        public const int ERROR_FILENAME_EXCED_RANGE = 0xCE;
        public const int ERROR_NO_DATA = 0xE8;
        public const int ERROR_MORE_DATA = 0xEA;
        public const int ERROR_NO_MORE_ITEMS = 0x103;
        public const int ERROR_NOT_OWNER = 0x120;
        public const int ERROR_TOO_MANY_POSTS = 0x12A;
        public const int ERROR_ARITHMETIC_OVERFLOW = 0x216;
        public const int ERROR_MUTANT_LIMIT_EXCEEDED = 0x24B;
        public const int ERROR_OPERATION_ABORTED = 0x3E3;
        public const int ERROR_IO_PENDING = 0x3E5;
        public const int ERROR_NO_UNICODE_TRANSLATION = 0x459;
        public const int ERROR_NOT_FOUND = 0x490;
        public const int ERROR_BAD_IMPERSONATION_LEVEL = 0x542;
        public const int ERROR_NO_SYSTEM_RESOURCES = 0x5AA;
        public const int ERROR_TIMEOUT = 0x000005B4;
    }
}
