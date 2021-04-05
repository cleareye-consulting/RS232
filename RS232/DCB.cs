using System;
using System.Runtime.InteropServices;

namespace RS232
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct DCB
    {
        internal uint DCBlength;
        internal uint BaudRate;
        internal uint Bits1;
        internal ushort wReserved;
        internal ushort XonLim;
        internal ushort XoffLim;
        internal byte ByteSize;
        internal byte Parity;
        internal byte StopBits;
        internal char XonChar;
        internal char XoffChar;
        internal char ErrorChar;
        internal char EofChar;
        internal char EvtChar;
        internal ushort wReserved2;
    }
}
