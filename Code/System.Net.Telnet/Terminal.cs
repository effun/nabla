using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Telnet
{
    public class Terminal : IDisposable
    {
        //AutoResetEvent _wait;
        //KeywordFilter[] _filters;
        //KeywordFilter _foundFilter;

        VirtualScreen _screen;

        Queue<DataArrivedEventArgs> _dataQueue;

        public Terminal(string host, int port = TelnetClient.DefaultTcpPort, Encoding encoding = null)
        {
            Client = new TelnetClient(host, port)
            {
                Encoding = encoding ?? Encoding.ASCII
            };

            _dataQueue = new Queue<DataArrivedEventArgs>();
            Client.DataArrived += Client_DataArrived;
            _screen = new VirtualScreen(Client.Encoding);
        }

        private void Client_DataArrived(object sender, DataArrivedEventArgs e)
        {
            if (e.Type == DataBlockType.Data)
            {
                //Trace.Write(Client.Encoding.GetString(e.Data));
            }

            lock (_dataQueue)
            {
                _dataQueue.Enqueue(e);
            }
        }

        //public KeywordFilter LastFound => _foundFilter;

        public VirtualScreen Screen => _screen;

        public TelnetClient Client { get; }

        public bool Connected => Client.Connected;

        public int Timeout { get; set; } = 10000;

        public Task<bool> ConnectAsync()
        {
            return Client.ConnectAsync();
        }

        public async Task<KeywordFilter> WaitFor(params KeywordFilter[] filters)
        {
            Stopwatch counter = new Stopwatch();

            counter.Start();

            while (true)
            {
                DataArrivedEventArgs[] array;

                lock (_dataQueue)
                {
                    array = _dataQueue.ToArray();
                    _dataQueue.Clear();
                }

                StringBuilder find = new StringBuilder();

                foreach (var e in array)
                {
                    _screen.Append(e, find);
                }

                //var found = _screen.Find(filters);
                //if (found != null)
                //    return found;

                string input = find.ToString();

                foreach (var keyword in filters)
                {
                    //Trace.WriteLine($"Looking for \"{keyword.Keyword}\" in \"{Regex.Escape(input)}\"");
                    if (keyword.IsMatch(input))
                        return keyword;
                }

                while (_dataQueue.Count == 0)
                {
                    if (counter.ElapsedMilliseconds > Timeout)
                        return null;

                    //Trace.WriteLine("Keyword not found, wait for 500 sec to try again.");
                    await Task.Delay(500);

                }
            }
        }

        public void Flush()
        {
            lock(_dataQueue)
            {
                while(_dataQueue.Count > 0)
                {
                    _screen.Append(_dataQueue.Dequeue(), null);
                }
            }
        }

        //public Task<KeywordFilter> WaitFor(params KeywordFilter[] filters)
        //{
        //    KeywordFilter found;

        //    if ((found = _screen.Find(filters)) != null)
        //        return Task.FromResult(found);

        //    if (_filters != null)
        //        throw new InvalidOperationException("A previous filter already applied.");

        //    _wait = new AutoResetEvent(false);
        //    _filters = filters;

        //    return Task.Run(() =>
        //    {
        //        var hit = _wait.WaitOne(Timeout);
        //        _filters = null;
        //        _wait.Dispose();
        //        _wait = null;
        //        return _foundFilter;
        //    });

        //}

        public async Task<bool> WaitForTextAsync(string text, bool ignoreCase = false)
        {
            return await WaitFor(new KeywordFilter(text, false) { IgnoreCase = ignoreCase }) != null;
        }

        public async Task<bool> WaitForRegexAsync(Regex regex)
        {
            return await WaitFor(new KeywordFilter(regex)) != null;
        }

        public async Task<bool> WaitForRegexAsync(string pattern, bool ignoreCase = false)
        {
            return await WaitFor(new KeywordFilter(pattern, true) { IgnoreCase = ignoreCase }) != null;
        }

        public Task SendLineAsync(bool simulate = true)
        {
            return SendLineAsync(null, simulate);
        }

        public Task SendLineAsync(string text, bool simulate = true)
        {
            if (!simulate)
                return SendAsync(text + "\r\n");
            else
                return UserInput(text, true);
        }

        public Task SendAsync(string text, bool simulate = true)
        {
            if (!simulate)
                return Client.WriteAsync(text);
            else
                return UserInput(text, false);
        }

        private async Task UserInput(string text, bool crlf)
        {
            if (text != null)
            {
                Random rnd = new Random();

                foreach (char ch in text)
                {
                    await Client.WriteAsync(ch);
                    await Task.Delay((int)(150 * rnd.NextDouble()));
                }
            }

            if (crlf)
            {
                await Client.WriteAsync("\r\n");
            }
        }

        public void Dispose()
        {
            Close();
        }

        private void Close()
        {
            Client.Close();
            //_wait?.Dispose();
        }
    }
}
