using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace ClearEye.RS232
{
    internal class SafeNativeMethods
    {

        internal const uint GENERIC_READ = 0x80000000;
        internal const uint GENERIC_WRITE = 0x40000000;
        internal const uint OPEN_EXISTING = 3;
        internal const uint FILE_FLAG_OVERLAPPED = 0x40000000;
        internal const uint FILE_SHARE_READ = 0x00000001;
        internal const uint FILE_SHARE_WRITE = 0x00000002;

        //See https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-clearcommerror#parameters
        internal const uint CE_BREAK = 0x0010;
        internal const uint CE_FRAME = 0x0008;
        internal const uint CE_OVERRUN = 0x0002;
        internal const uint CE_RXOVER = 0x0001;
        internal const uint CE_RXPARITY = 0x0003;

        //See https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-purgecomm#parameters
        internal const uint PURGE_RXCLEAR = 0x0008;
        internal const uint PURGE_TXCLEAR = 0x0004;

        //See https://docs.microsoft.com/en-us/windows/win32/api/synchapi/nf-synchapi-waitforsingleobject#return-value
        internal const uint WAIT_OBJECT_0 = 0x00000000;
        internal const uint WAIT_TIMEOUT = 0x00000102;
        internal const uint WAIT_FAILED = 0xFFFFFFFF;

        internal const uint ERROR_IO_PENDING = 0x3E5;
            



        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern SafeFileHandle CreateFile(
          string lpFileName,
          uint dwDesiredAccess,
          uint dwShareMode,
          int lpSecurityAttributes,
          uint dwCreationDisposition,
          uint dwFlagsAndAttributes,
          IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool CloseHandle(IntPtr hObject);

        

        

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int ClearCommError(
          SafeHandle hFile,
          ref int lpErrors,
          ref ComStat lpComStat);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int PurgeComm(SafeHandle hFile, uint dwFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int GetCommState(SafeHandle hCommDev, ref DCB lpDCB);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern int BuildCommDCB(string lpDef, ref DCB lpDCB);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int SetCommState(SafeHandle hCommDev, ref DCB lpDCB);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int SetupComm(SafeHandle hFile, uint dwInQueue, uint dwOutQueue);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr CreateEvent(
          IntPtr lpEventAttributes,
          int bManualReset,
          int bInitialState,
          string lpName);

        

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);
    }
}
