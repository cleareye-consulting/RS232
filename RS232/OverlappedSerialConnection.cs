using System;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;

namespace ClearEye.RS232
{

    [SupportedOSPlatform("windows")]
    public class OverlappedSerialConnection : ISerialConnection
    {

        private const int bufferSize = 64;

        private static readonly TimeSpan readTimeout = TimeSpan.FromSeconds(1);

        private readonly string fileName;
        private readonly int baudRate;
        private readonly Parity parity;
        private readonly int dataBits;
        private readonly StopBits stopBits;
        private SafeHandle fileHandle;
        private bool disposedValue;

        public OverlappedSerialConnection(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            fileName = $@"\\.\{portName}";
            this.baudRate = baudRate;
            this.parity = parity;
            this.dataBits = dataBits;
            this.stopBits = stopBits;
        }

        public void Open()
        {
            fileHandle = SafeNativeMethods.CreateFile(fileName,
                SafeNativeMethods.GENERIC_READ | SafeNativeMethods.GENERIC_WRITE,
                SafeNativeMethods.FILE_SHARE_READ | SafeNativeMethods.FILE_SHARE_WRITE,
                0,
                SafeNativeMethods.OPEN_EXISTING,
                SafeNativeMethods.FILE_FLAG_OVERLAPPED,
                IntPtr.Zero);
            int errorCode;
            if (fileHandle.IsInvalid)
            {
                errorCode = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"Error opening COM port. Last Win32 error code is {errorCode}.");
            }

            ComStat comStat = default;
            int errors = 0;
            if (SafeNativeMethods.ClearCommError(fileHandle, ref errors, ref comStat) == 0)
            {
                errorCode = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"ClearCommError returned FALSE. Last Win32 error code is {errorCode}. Error buffer from method contains {errors}.");
            }
            if (SafeNativeMethods.PurgeComm(fileHandle, SafeNativeMethods.PURGE_RXCLEAR | SafeNativeMethods.PURGE_TXCLEAR) == 0)
            {
                errorCode = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"PurgeComm returned FALSE. Last Win32 error code is {errorCode}.");
            }
            DCB dcb = default;
            if (SafeNativeMethods.GetCommState(fileHandle, ref dcb) == 0)
            {
                errorCode = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"GetCommState returned FALSE. Last Win32 error code is {errorCode}.");
            }
            //Parity in the DCB format is a single character, which happens to correspond to the first character of the enum symbols.
            var dcbString = $"baud={baudRate} parity={parity.ToString()[0]} data={dataBits} stop={(int)stopBits}";
            if (SafeNativeMethods.BuildCommDCB(dcbString, ref dcb) == 0)
            {
                errorCode = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"BuildCommDCB returned FALSE. Last Win32 error code is {errorCode}.");
            }
            if (SafeNativeMethods.SetCommState(fileHandle, ref dcb) == 0)
            {
                errorCode = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"SetCommState returned FALSE. Last Win32 error code is {errorCode}.");
            }
            if (SafeNativeMethods.SetupComm(fileHandle, (uint)bufferSize, (uint)bufferSize) == 0)
            {
                errorCode = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"SetupComm returned FALSE. Last Win32 error code is {errorCode}.");
            }
        }

        unsafe void WriteCompletionCallback(uint errorCode, uint numBytes, NativeOverlapped* nativeOverlapped)
        {
            try
            {
                // ...
            }
            finally
            {
                Overlapped.Unpack(nativeOverlapped);
                Overlapped.Free(nativeOverlapped);
            }
        }

        unsafe void ReadCompletionCallback(uint errorCode, uint numBytes, NativeOverlapped *nativeOverlapped)
        {
            try
            {
                var overlapped = Overlapped.Unpack(nativeOverlapped);
            }
            finally
            {   Overlapped.Free(nativeOverlapped);
            }
        }

        unsafe public void Write(string value)
        {
            if (value.Length > bufferSize)
            {
                throw new ArgumentException($"Value '{value}' longer than specified buffer size of {bufferSize}", nameof(value));
            }
            int errorCode;
            var overlapped = new Overlapped();

            NativeOverlapped* nativeOverlapped = overlapped.Pack(WriteCompletionCallback, null);
            var valueAsByteArray = Encoding.ASCII.GetBytes(value);
            uint bytesWritten = 0;
            if (UnsafeNativeMethods.WriteFile(fileHandle, valueAsByteArray, (uint)value.Length, ref bytesWritten, nativeOverlapped) == 0)
            {
                errorCode = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"WriteFile returned FALSE. Last Win32 error code is {errorCode}.");
            }
            if (bytesWritten != value.Length)
            {
                throw new InvalidOperationException($"Expected to write {value.Length} bytes, wrote {bytesWritten}");
            }
            uint bytesTransferred = 0;
            if (UnsafeNativeMethods.GetOverlappedResult(fileHandle, nativeOverlapped, ref bytesTransferred, 1) == 0)
            {
                errorCode = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"GetOverlappedResult returned FALSE. Last Win32 error code is {errorCode}.");
            }
            if (bytesTransferred != value.Length)
            {
                throw new InvalidOperationException($"Expected to transfer {value.Length}, transferred {bytesTransferred}");
            }
            return;
        }

        unsafe public string Read(int numberOfBytes)
        {
            if (numberOfBytes > bufferSize)
            {
                throw new ArgumentException($"Cannot read {numberOfBytes} bytes because buffer size specified in constructor is only {bufferSize}.", nameof(numberOfBytes));
            }
            var overlapped = new Overlapped();
            NativeOverlapped* nativeOverlapped = overlapped.Pack(ReadCompletionCallback, null);
            int errorCode;
            var buffer = new byte[numberOfBytes];
            uint bytesRead = 0;
            if (UnsafeNativeMethods.ReadFile(fileHandle, buffer, numberOfBytes, ref bytesRead, nativeOverlapped) == 0)
            {
                errorCode = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"ReadFile returned FALSE. Last Win32 error code is {errorCode}.");
            }
            if (bytesRead != numberOfBytes)
            {
                throw new InvalidOperationException($"Expected to read {numberOfBytes}, read {bytesRead}");
            }

            uint bytesTransferred = 0;
            if (UnsafeNativeMethods.GetOverlappedResult(fileHandle, nativeOverlapped, ref bytesTransferred, 1) == 0)
            {
                errorCode = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"GetOverlappedResult returned FALSE. Last Win32 error code is {errorCode}.");
            }
            if (bytesTransferred != numberOfBytes)
            {
                throw new InvalidOperationException($"Expected to transfer {numberOfBytes} bytes, transferred {bytesTransferred}");
            }
            return Encoding.ASCII.GetString(buffer);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }
                fileHandle.Dispose();
                disposedValue = true;
            }
        }

        ~OverlappedSerialConnection()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
