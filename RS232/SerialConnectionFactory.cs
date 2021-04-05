using System;
using System.IO.Ports;

namespace ClearEye.RS232
{
    public static class SerialConnectionFactory
    {
        public static ISerialConnection GetSerialConnection(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits, bool useOverlappedIO)
        {
            if (useOverlappedIO)
            {
                return new OverlappedSerialConnection(portName, baudRate, parity, dataBits, stopBits);
            }
            return new NonoverlappedSerialConnection(portName, baudRate, parity, dataBits, stopBits);
        }
    }
}
