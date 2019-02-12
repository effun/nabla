namespace System.Net.Telnet
{
    struct DataBlock
    {
        public static readonly DataBlock Empty = new DataBlock();

        public byte[] Data { get; set; }

        public DataBlockType Type { get; set; }

        public bool IsEmpty => Data == null;
    }
}
