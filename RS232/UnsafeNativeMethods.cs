using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClearEye.RS232
{
    internal static class UnsafeNativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        unsafe internal static extern int WriteFile(
              SafeHandle hFile,
              byte[] Buffer,
              uint nNumberOfBytesToWrite,
              ref uint lpNumberOfBytesWritten,
              NativeOverlapped *lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        unsafe internal static extern int ReadFile(
          SafeHandle hFile,
          [Out] byte[] Buffer,
          int nNumberOfBytesToRead,
          ref uint lpNumberOfBytesRead,
          NativeOverlapped *lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        unsafe internal static extern int GetOverlappedResult(
         SafeHandle hFile,
         NativeOverlapped *lpOverlapped,
         ref uint lpNumberOfBytesTransferred,
         int bWait);

       
    }
}
