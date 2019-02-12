using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace System.Net.Telnet
{
    public class VirtualScreen
    {
        List<List<char>> _lines;
        int _x, _y;
        Encoding _encoding;
#if DEBUG
        List<char> _all;
#endif
        internal VirtualScreen(Encoding encoding)
        {
            _lines = new List<List<char>>();
            NewLine();

            _encoding = encoding;

#if DEBUG
            _all = new List<char>(1024);
#endif
        }

        private void NewLine()
        {
            _lines.Add(new List<char>(80));
        }

        private string DecodeText(byte[] data, int offset = 0)
        {
            return _encoding.GetString(data, offset, data.Length - offset);
        }

        internal void Append(DataArrivedEventArgs e, StringBuilder builder)
        {
            if (e.Type == DataBlockType.Data)
            {
                string text = DecodeText(e.Data);
                AppendText(text, builder);

            }

        }

        private List<char> CurrentLine => _lines[_y];

        private void ProcessEscapeSeuence(EscapeSequence sequence)
        {
            if (sequence.Command == EscapeCommand.CursorBackward)
            {
                _x -= sequence.GetParameterInt32();
                if (_x < 0)
                    _x = 0;
            }
            else if (sequence.Command == EscapeCommand.ClearLineFromCursor)
            {
                var line = CurrentLine;
                var length = line.Count;

                if (_x < length)
                    line.RemoveRange(_x, length - _x);
            }
            else if (sequence.Command == EscapeCommand.ClearScreen)
            {
                Clear();
            }
            else
                Trace.WriteLine($"Escape sequence {sequence.Command} ignored.");
        }

        private void AppendText(string text, StringBuilder builder)
        {
            lock (_lines)
            {
                var chars = text.ToArray();
                var length = chars.Length;

#if DEBUG
                _all.AddRange(chars);
#endif

                for (int i = 0; i < length; i++)
                {
                    var append = true;
                    var ch = chars[i];

                    if (ch == '\r')
                    {
                        DoCarriageReturn();
                    }
                    else if (ch == '\n')
                    {
                        DoLineFeed();
                    }
                    else if (ch == '\x8')
                    {
                        DoBackspace();

                        if (builder != null && builder.Length > 0)
                        {
                            builder.Remove(builder.Length - 1, 1);
                        }

                        append = false;
                    }
                    else if (ch == '\x1b')
                    {
                        i += DoEscape(text.Substring(++i));
                        append = false;
                    }
                    else if (ch >= ' ')
                    {
                        DoCharacter(ch);
                    }
                    else
                        append = false;

                    if (append && builder != null)
                        builder.Append(ch);
                }
            }
        }

        private int DoEscape(string text)
        {
            var esc = EscapeSequenceDefinition.Match(text, out int length);

            if (esc != null)
            {
                ProcessEscapeSeuence(esc);
                return length;
            }

                return 0;
        }

        private void DoCharacter(char ch)
        {
            var line = _lines[_y];

            while (_x >= line.Count)
                line.Add(' ');

            line[_x++] = ch;
        }

        private void DoBackspace()
        {
            if (_x > 0)
            {
                var line = _lines[_y];

                line.RemoveAt(_x-- - 1);
            }
        }

        private void DoLineFeed()
        {
            _y++;

            while (_y >= _lines.Count)
            {
                NewLine();
            }

            var line = _lines[_y];
            if (_x > line.Count)
                _x = line.Count;

            //int y = _y++;

            //List<char> line1 = new List<char>(80), line0 = _lines[y];
            //_lines.Insert(_y, line1);

            //if (_x < line0.Count)
            //{
            //    line1.AddRange(line0.Skip(_x));
            //    line0.RemoveRange(_x, line0.Count - _x);
            //}

            //if (_x > line1.Count)
            //    _x = line1.Count;

        }

        private void DoCarriageReturn()
        {
            _x = 0;
        }

        public void Clear()
        {
            lock (_lines)
            {
                _lines.Clear();
                _x = _y = 0;
                NewLine();
            }
        }

        public string GetText()
        {
            StringBuilder builder = new StringBuilder();

            foreach (var line in _lines)
            {
                if (builder.Length > 0) builder.AppendLine();
                builder.Append(line.ToArray());
            }

            return builder.ToString();
        }

        internal KeywordFilter Find(ICollection<KeywordFilter> filters)
        {
            lock (_lines)
            {
                foreach (var line in Enumerable.Reverse(_lines))
                {
                    string lineText = new string(line.ToArray());

                    foreach (var filter in filters)
                    {
                        if (filter.IsMatch(lineText))
                            return filter;
                    }
                }
            }

            return null;
        }

        CaptureState _capture;

        public void BeginCapture()
        {
            if (_capture != null)
                throw new NotSupportedException("Multiple capture is not supported.");

            _capture = new CaptureState { X = _x, Y = _y };
        }

        public string EndCapture()
        {
            if (_capture == null)
                throw new InvalidOperationException("Capture has not started.");

            StringBuilder text = new StringBuilder();

            lock (_lines)
            {

                for (int y = _capture.Y; y <= _y; y++)
                {
                    char[] line = _lines[y].ToArray();

                    if (text.Length > 0)
                        text.AppendLine();

                    if (y == _capture.Y)
                    {
                        if (_capture.X < line.Length)
                            text.Append(line.Skip(_capture.X).ToArray());
                    }
                    else if (y == _y)
                    {
                        if (_x > 0)
                            text.Append(line.Take(_x).ToArray());
                    }
                    else
                    {
                        text.Append(line);
                    }
                    
                }
            }

            _capture = null;

            return text.ToString();
        }

        public char[] GetAllText()
        {
#if DEBUG
            return _all.ToArray();
#else
            return null;
#endif
        }

        class CaptureState
        {
            public int X { get; set; }

            public int Y { get; set; }
        }
    }
}
