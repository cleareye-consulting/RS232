﻿using System;
using System.Runtime.InteropServices;

namespace RS232
{
    internal class SafeExternalMethods
    {

        internal const uint GENERIC_READ = 0x80000000;
        internal const uint GENERIC_WRITE = 0x40000000;
        internal const uint OPEN_EXISTING = 3;
        internal const uint FILE_FLAG_OVERLAPPED = 0x40000000;

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



        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr CreateFile(
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
        internal static extern int WriteFile(
              IntPtr hFile,
              byte[] Buffer,
              uint nNumberOfBytesToWrite,
              ref uint lpNumberOfBytesWritten,
              ref Overlapped lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int ReadFile(
          IntPtr hFile,
          [Out] byte[] Buffer,
          int nNumberOfBytesToRead,
          ref uint lpNumberOfBytesRead,
          ref Overlapped lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int ClearCommError(
          IntPtr hFile,
          ref int lpErrors,
          ref ComStat lpComStat);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int PurgeComm(IntPtr hFile, uint dwFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int GetCommState(IntPtr hCommDev, ref DCB lpDCB);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern int BuildCommDCB(string lpDef, ref DCB lpDCB);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int SetCommState(IntPtr hCommDev, ref DCB lpDCB);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int SetupComm(IntPtr hFile, uint dwInQueue, uint dwOutQueue);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr CreateEvent(
          IntPtr lpEventAttributes,
          int bManualReset,
          int bInitialState,
          string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int GetOverlappedResult(
         IntPtr hFile,
         ref Overlapped lpOverlapped,
         ref uint lpNumberOfBytesTransferred,
         int bWait);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);
    }
}
