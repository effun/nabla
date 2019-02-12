using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace System.Net.Telnet
{
    internal class TelnetDataReader
    {
        const int BufferSize = 4096;

        TcpClient _tcp;
        byte[] _buffer;
        private int _length;
        private int _offset;

        public TelnetDataReader(TcpClient tcp)
        {
            _tcp = tcp;
            _buffer = new byte[BufferSize];
        }

        private bool Connected => _tcp.Connected;

        private int FillBuffer(bool force)
        {
            if (Connected)
            {

                var available = _tcp.Available;

                if (available > 0 || force)
                {
                    var stream = _tcp.GetStream();
                    int read;

                    if (_length == 0)
                    {
                        _offset = 0;
                    }
                    else if (_offset > 0)
                    {
                        byte[] temp = new byte[_length];

                        Array.Copy(_buffer, _offset, temp, 0, _length);
                        Array.Copy(temp, _buffer, _length);

                        _offset = 0;
                    }

                    try
                    {
                        int expected;

                        if (available > 0)
                        {
                            expected = available;
                            if (expected + _offset + _length > BufferSize)
                                expected = BufferSize - _offset - _length;
                        }
                        else
                            expected = 1;

                        read = stream.Read(_buffer, _offset, expected);

                        //Trace.WriteLine($"{read} out of {expected} bytes retrieved.");

                        _length += read;

                        return read;
                    }
                    catch(SocketException)
                    {
                        return EndOfStream;
                    }
                    catch(IOException)
                    {
                        return EndOfStream;
                    }
                    catch (ObjectDisposedException)
                    {
                        return EndOfStream;
                    }
                }
                else
                    return NoData;
            }

            return EndOfStream;
        }

        private int PeekByte(bool force)
        {
            if (!Connected)
                return EndOfStream;

            if (_length == 0)
            {
                var result = FillBuffer(force);

                if (result == EndOfStream || result == NoData)
                    return result;
            }

            if (_length == 0)
                return NoData;

            int value = _buffer[_offset];

            return value;
        }

        private void MoveNext()
        {
            _offset++;
            _length--;
        }

        private byte ReadByte()
        {
            int value = PeekByte(true);

            if (value == EndOfStream || value == NoData)
                throw new EndOfStreamException();

            MoveNext();

            return (byte)value;
        }

        public DataBlock ReadBlock()
        {
            using (var ms = new MemoryStream())
            {
                int test = PeekByte(true);
                DataBlockType type = DataBlockType.Unknown;

                if (test == EndOfStream || test == NoData)
                    return DataBlock.Empty;

                ms.WriteByte((byte)test);
                MoveNext();

                if (test == IAC)
                {
                    ms.WriteByte(ReadByte());
                    ms.WriteByte(ReadByte());
                    type = DataBlockType.IAC;
                }
                else
                {
                    type = DataBlockType.Data;

                    while (true)
                    {
                        test = PeekByte(false);

                        if (test == EndOfStream)
                            return DataBlock.Empty;

                        if (test == NoData || test == IAC)
                            break;

                        ms.WriteByte((byte)test);
                        MoveNext();
                    }
                }

                return new DataBlock { Data = ms.ToArray(), Type = type };
            }
        }

        const int IAC = 255, EndOfStream = -2, NoData = -1;

    }
}
