using System;
using System.Runtime.InteropServices;

namespace RS232
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DCB
    {
        public uint DCBlength;
        public uint BaudRate;
        public uint Bits1;
        public ushort wReserved;
        public ushort XonLim;
        public ushort XoffLim;
        public byte ByteSize;
        public byte Parity;
        public byte StopBits;
        public char XonChar;
        public char XoffChar;
        public char ErrorChar;
        public char EofChar;
        public char EvtChar;
        public ushort wReserved2;
    }
}
