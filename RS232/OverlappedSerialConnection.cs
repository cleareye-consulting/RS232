using System;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Text;

namespace RS232
{
    public class OverlappedSerialConnection : ISerialConnection
    {

        private const int bufferSize = 64;

        private static readonly TimeSpan readTimeout = TimeSpan.FromSeconds(1);

        private readonly string fileName;
        private readonly int baudRate;
        private readonly Parity parity;
        private readonly int dataBits;
        private readonly StopBits stopBits;
        private IntPtr fileHandle;
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
            fileHandle = SafeExternalMethods.CreateFile(fileName,
                (SafeExternalMethods.GENERIC_READ | SafeExternalMethods.GENERIC_WRITE),
                0,
                0,
                SafeExternalMethods.OPEN_EXISTING,
                SafeExternalMethods.FILE_FLAG_OVERLAPPED,
                IntPtr.Zero);
            int errorCode;
            if (fileHandle == IntPtr.Zero)
            {
                errorCode = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"Error opening COM port. Last Win32 error code is {errorCode}.");
            }

            ComStat comStat = default;
            int errors = 0;            
            if (SafeExternalMethods.ClearCommError(fileHandle, ref errors, ref comStat) == 0)
            {
                errorCode = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"ClearCommError returned FALSE. Last Win32 error code is {errorCode}. Error buffer from method contains {errors}.");
            }
            if (SafeExternalMethods.PurgeComm(fileHandle, SafeExternalMethods.PURGE_RXCLEAR | SafeExternalMethods.PURGE_TXCLEAR) == 0)
            {
                errorCode = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"PurgeComm returned FALSE. Last Win32 error code is {errorCode}.");
            }
            DCB dcb = default;
            if (SafeExternalMethods.GetCommState(fileHandle, ref dcb) == 0)
            {
                errorCode = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"GetCommState returned FALSE. Last Win32 error code is {errorCode}.");
            }
            //Parity in the DCB format is a single character, which happens to correspond to the first character of the enum symbols.
            var dcbString = $"baud={baudRate} parity={parity.ToString()[0]} data={dataBits} stop={(int)stopBits}";
            if (SafeExternalMethods.BuildCommDCB(dcbString, ref dcb) == 0)
            {
                errorCode = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"BuildCommDCB returned FALSE. Last Win32 error code is {errorCode}.");
            }
            if (SafeExternalMethods.SetCommState(fileHandle, ref dcb) == 0)
            {
                errorCode = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"SetCommState returned FALSE. Last Win32 error code is {errorCode}.");
            }
            if (SafeExternalMethods.SetupComm(fileHandle, (uint)bufferSize, (uint)bufferSize) == 0)
            {
                errorCode = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"SetupComm returned FALSE. Last Win32 error code is {errorCode}.");
            }
        }

        public void Write(string value)
        {
            if (value.Length > bufferSize)
            {
                throw new ArgumentException($"Value '{value}' longer than specified buffer size of {bufferSize}", nameof(value));
            }
            Overlapped overlapped = default;
            overlapped.hEvent = SafeExternalMethods.CreateEvent(IntPtr.Zero, 1, 0, null);
            int errorCode;
            if (overlapped.hEvent == IntPtr.Zero)
            {
                errorCode = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"CreateEvent returned FALSE. Last Win32 error code is {errorCode}.");
            }
            try
            {
                var valueAsByteArray = Encoding.ASCII.GetBytes(value);
                uint bytesWritten = 0;
                if (SafeExternalMethods.WriteFile(fileHandle, valueAsByteArray, (uint)value.Length, ref bytesWritten, ref overlapped) == 0)
                {
                    errorCode = Marshal.GetLastWin32Error();
                    throw new InvalidOperationException($"WriteFile returned FALSE. Last Win32 error code is {errorCode}.");
                }
                if (bytesWritten != value.Length)
                {
                    throw new InvalidOperationException($"Expected to write {value.Length} bytes, wrote {bytesWritten}");
                }
                uint bytesTransferred = 0;
                if (SafeExternalMethods.GetOverlappedResult(fileHandle, ref overlapped, ref bytesTransferred, 1) == 0)
                {
                    errorCode = Marshal.GetLastWin32Error();
                    throw new InvalidOperationException($"GetOverlappedResult returned FALSE. Last Win32 error code is {errorCode}.");
                }
                if (bytesTransferred != value.Length)
                {
                    throw new InvalidOperationException($"Expected to transfer {value.Length} bytes, transferred {bytesTransferred}");
                }
            }
            finally
            {
                _ = SafeExternalMethods.CloseHandle(overlapped.hEvent);
            }
        }

        public string Read(int numberOfBytes)
        {
            if (numberOfBytes > bufferSize)
            {
                throw new ArgumentException($"Cannot read {numberOfBytes} bytes because buffer size specified in constructor is only {bufferSize}.", nameof(numberOfBytes));
            }
            Overlapped overlapped = default;
            overlapped.hEvent = SafeExternalMethods.CreateEvent(IntPtr.Zero, 1, 0, null);
            int errorCode;
            if (overlapped.hEvent == IntPtr.Zero)
            {
                errorCode = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"CreateEvent returned FALSE. Last Win32 error code is {errorCode}.");
            }
            try
            {
                var buffer = new byte[numberOfBytes];
                uint bytesRead = 0;
                if (SafeExternalMethods.ReadFile(fileHandle, buffer, numberOfBytes, ref bytesRead, ref overlapped) == 0)
                {
                    errorCode = Marshal.GetLastWin32Error();
                    throw new InvalidOperationException($"ReadFile returned FALSE. Last Win32 error code is {errorCode}.");
                }
                if (bytesRead != numberOfBytes)
                {
                    throw new InvalidOperationException($"Expected to read {numberOfBytes}, read {bytesRead}");
                }
                var overlappedResult = SafeExternalMethods.WaitForSingleObject(overlapped.hEvent, (uint) readTimeout.TotalMilliseconds);
                if (overlappedResult == SafeExternalMethods.WAIT_OBJECT_0)
                {
                    uint bytesTransferred = 0;
                    if (SafeExternalMethods.GetOverlappedResult(fileHandle, ref overlapped, ref bytesTransferred, 1) == 0)
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
                if (overlappedResult == SafeExternalMethods.WAIT_TIMEOUT)
                {
                    throw new InvalidOperationException($"WaitForSingleObject timed out after {readTimeout.TotalMilliseconds} ms");
                }
                if (overlappedResult == SafeExternalMethods.WAIT_FAILED)
                {
                    errorCode = Marshal.GetLastWin32Error();
                    throw new InvalidOperationException($"WaitForSingleObject returned WAIT_FAILED. Last Win32 error code is {errorCode}.");
                }
                throw new InvalidOperationException("Unexpected error occurred in read");
            }
            finally
            {
                _ = SafeExternalMethods.CloseHandle(overlapped.hEvent);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }
                if (fileHandle != IntPtr.Zero) {
                    _ = SafeExternalMethods.CloseHandle(fileHandle);
                }
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
