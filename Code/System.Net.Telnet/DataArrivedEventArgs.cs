using System.Text;

namespace System.Net.Telnet
{
    public class DataArrivedEventArgs : EventArgs
    {
        internal DataArrivedEventArgs(DataBlock data)
        {
            Data = data.Data;
            Type = data.Type;
        }

        public DataBlockType Type { get; private set; }

        public byte[] Data { get; private set; }

    }

}
