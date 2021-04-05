using System;
namespace RS232
{
    public interface ISerialConnection : IDisposable
    {
        void Open();
        void Write(string value);
        string Read(int numberOfBytes);
    }
}
