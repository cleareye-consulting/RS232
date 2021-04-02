using System;
using System.Runtime.InteropServices;

namespace RS232
{

    [StructLayout(LayoutKind.Sequential, Pack =1)]
    public struct Overlapped
    {
        public int Internal;
        public int InternalHigh;
        public int Offset;
        public int OffsetHigh;
        public IntPtr hEvent;
    }
}
