using System;
using System.Runtime.InteropServices;

namespace ClearEye.RS232
{

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct Overlapped
    {
        internal int Internal;
        internal int InternalHigh;
        internal int Offset;
        internal int OffsetHigh;
        internal IntPtr hEvent;
    }
}
