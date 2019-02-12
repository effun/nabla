using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Telnet
{
    public class TelnetClient : IDisposable
    {
        public const int DefaultTcpPort = 23;

        public event EventHandler<DataArrivedEventArgs> DataArrived;

        private const int BufferSize = 4096;

        private Thread _receivingThread;
        private bool _receiving;

        private Encoding _encoding;

        public TelnetClient(string host)
            : this(host, DefaultTcpPort)
        {

        }

        public TelnetClient(string host, int port)
        {
            if (string.IsNullOrEmpty(host))
            {
                throw new ArgumentException("Invalid host name, should not be empty.", nameof(host));
            }

            Host = host;
            Port = port;

            _encoding = Encoding.ASCII;
        }

        private TcpClient _tcp;

        public string Host { get; private set; }

        public int Port { get; private set; }

        public bool NoEcho { get; set; } = true;

        public Encoding Encoding { get => _encoding; set => _encoding = value; }

        public async Task<bool> ConnectAsync()
        {
            if (!Connected)
            {
                _tcp = new TcpClient();
                
                try
                {
                    await _tcp.ConnectAsync(Host, Port);
                }
                catch
                {
                    _tcp.Dispose();
                    _tcp = null;
                    return false;
                }

                //await Negotiate();

                _receivingThread = new Thread(ReceiveThread)
                {
                    Name = "telnet_receive",
                    IsBackground = true
                };

                _receiving = true;
                _receivingThread.Start();

                return _tcp.Connected;
            }
            else
                return true;
        }

        public bool Connected
        {
            get => _tcp?.Connected ?? false;
        }

        public void Close()
        {
            if (Connected)
            {
                _tcp.Close();
                _tcp = null;
            }
        }

        private void StopReceive()
        {
            if (_receivingThread != null)
            {
                _receivingThread.Join();
                _receivingThread = null;
            }
        }

        protected virtual void OnDataArrived(DataArrivedEventArgs e)
        {
            DataArrived?.Invoke(this, e);
        }

        private void ReceiveThread()
        {
            TelnetDataReader reader = new TelnetDataReader(_tcp);

            while(_receiving && Connected)
            {
                var block = reader.ReadBlock();

                if (!block.IsEmpty)
                {
                    if (block.Type == DataBlockType.IAC)
                        OnIAC(new IAC(block.Data));
                    else
                        OnData(block);
                }
                else
                    break;
            }
        }

        private void OnData(DataBlock block)
        {
            OnDataArrived(new DataArrivedEventArgs(block));
        }

        private void OnIAC(IAC iac)
        {
            string text = iac.ToString();
            bool reply = true;

            if (iac.Option == Options.ECHO)
            {
                if (NoEcho)
                {
                    if (iac.Command == Verbs.WILL)
                        iac.Command = Verbs.DONT;
                    else
                        reply = false;
                }
                else
                    iac.Command = Verbs.WILL;
            }
            else if (iac.Option == Options.SGA)
            {
                iac.Command = Verbs.DONT;
            }
            else
            {
                switch (iac.Command)
                {
                    case Verbs.DO:
                        iac.Command = Verbs.WONT;
                        break;
                    case Verbs.DONT:
                        iac.Command = Verbs.WONT;
                        break;
                    case Verbs.WILL:
                        iac.Command = iac.Option == Options.SGA ? Verbs.DO : Verbs.DONT;
                        break;
                    case Verbs.WONT:
                        iac.Command = Verbs.DONT;
                        break;
                }
            }

            text += " -> " + iac.ToString();

            if (reply)
            {
                text += " (replied)";
                Write(iac);
            }

            Trace.WriteLine("IAC: " + text);


        }

        //private async Task Negotiate()
        //{
        //    string result = string.Empty;
        //    if (Connected)
        //    {
        //        while (true)
        //        {
        //            var pkg = await ReadIAC();

        //            if (pkg != null)
        //            {
        //                var iac = pkg.Value;

        //                //Console.WriteLine(iac.ToString());

        //                switch (iac.Command)
        //                {
        //                    case Verbs.DO:
        //                        iac.Command = iac.Option == Options.ECHO ? Verbs.WILL : Verbs.WONT;
        //                        break;
        //                    case Verbs.DONT:
        //                        iac.Command = Verbs.WONT;
        //                        break;
        //                    case Verbs.WILL:
        //                        iac.Command = iac.Option == Options.SGA ? Verbs.DO : Verbs.DONT;
        //                        break;
        //                    case Verbs.WONT:
        //                        iac.Command = Verbs.DONT;
        //                        break;
        //                    default:
        //                        continue;
        //                }

        //                Write(iac);
        //            }
        //            else
        //                break;
        //        }
        //    }
        //}

        private void Write(IAC iac)
        {
            Write(iac.ToByteArray());
        }

        private Task Write(byte[] data)
        {
            if (Connected)
            {
                return _tcp.GetStream().WriteAsync(data, 0, data.Length);
            }
            else
                return Task.CompletedTask;
        }

        //public Task WriteLineAsync(string text)
        //{
        //    text += Environment.NewLine;

        //    return Write(_encoding.GetBytes(text));
        //}

        public Task WriteAsync(char c)
        {
            return Write(_encoding.GetBytes(new char[] { c }));
        }

        public Task WriteAsync(string text)
        {
            return Write(_encoding.GetBytes(text));
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_tcp != null)
                    {
                        Close();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~TelnetClient() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

        const byte CR = 13, LF = 10;

        enum Verbs : byte
        {
            WILL = 251,
            WONT = 252,
            DO = 253,
            DONT = 254,
            IAC = 255
        }

        enum Options : byte
        {
            ECHO = 1,
            SGA = 3
        }

        struct IAC
        {
            public IAC(byte cmd, byte option)
            {
                Command = (Verbs)cmd;
                Option = (Options)option;
            }

            public IAC(byte[] data)
            {
                if (data == null)
                    throw new ArgumentNullException(nameof(data));

                int length = data.Length;

                if (length < 2)
                    throw new ArgumentException();

                if (length >= 3)
                {
                    if (data[0] != (byte)Verbs.IAC)
                        throw new ArgumentException();

                    Command = (Verbs)data[1];
                    Option = (Options)data[2];
                }
                else
                {
                    Command = (Verbs)data[0];
                    Option = (Options)data[1];
                }
            }

            public Verbs Command { get; set; }
            public Options Option { get; set; }

            public byte[] ToByteArray()
            {
                return new byte[] { (byte)Verbs.IAC, (byte)Command, (byte)Option };
            }

            public override string ToString()
            {
                return $"{Command} {Option}";
            }
        }
    }

}
