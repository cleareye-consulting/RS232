using System;
using System.Runtime.InteropServices;

namespace RS232
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ComStat
    {
        public int fBitFields;
        public int cbInQue;
        public int cbOutQue;
    }
}
