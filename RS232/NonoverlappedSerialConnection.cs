using System;
using System.IO.Ports;
using System.Text;

namespace ClearEye.RS232
{
    public class NonoverlappedSerialConnection : ISerialConnection
    {

        private readonly SerialPort serialPort;
        private bool disposedValue;

        public NonoverlappedSerialConnection(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
            serialPort.WriteTimeout = 1000; //ms
            serialPort.ReadTimeout = 1000; //ms
        }

        public void Open()
        {
            serialPort.Open();
        }

        public string Read(int numberOfBytes)
        {
            var buffer = new byte[numberOfBytes];
            var bytesRead = serialPort.Read(buffer, 0, numberOfBytes);
            if (bytesRead != numberOfBytes)
            {
                throw new InvalidOperationException($"Expected to read {numberOfBytes} bytes, read {bytesRead}");
            }
            return Encoding.ASCII.GetString(buffer);
        }

        public void Write(string value)
        {
            serialPort.Write(value);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    serialPort.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
