using System.Text.RegularExpressions;

namespace System.Net.Telnet
{
    class EscapeSequenceDefinition
    {
        private EscapeSequenceDefinition(EscapeCommand command, string pattern, params string[] defaultValues)
        {
            Pattern = pattern;
            Command = command;
            DefaultValues = defaultValues;
        }

        public string[] DefaultValues { get; private set; }

        public string Pattern { get; private set; }

        public EscapeCommand Command { get; private set; }

        Regex _regex;

        private Match Match(string input)
        {
            if (_regex == null)
                _regex = new Regex("^" + Pattern, RegexOptions.Compiled);

            return _regex.Match(input);
        }

        static EscapeSequenceDefinition[] Sequences = new EscapeSequenceDefinition[]
        {
            new EscapeSequenceDefinition(EscapeCommand.CursorHome, @"\[(?:(\d+);(\d+))?H"),
            new EscapeSequenceDefinition(EscapeCommand.CursorUp, @"\[(\d+)?A", "1"),
            new EscapeSequenceDefinition(EscapeCommand.CursorDown, @"\[(\d+)?B", "1"),
            new EscapeSequenceDefinition(EscapeCommand.CursorForward, @"\[(\d+)?C", "1"),
            new EscapeSequenceDefinition(EscapeCommand.CursorBackward, @"\[(\d+)?D", "1"),
            new EscapeSequenceDefinition(EscapeCommand.ScrollScreen, @"\[(?:(\d+);(\d+))?r"),
            new EscapeSequenceDefinition(EscapeCommand.ScrollDown, @"D"),
            new EscapeSequenceDefinition(EscapeCommand.ScrollUp, @"M"),
            new EscapeSequenceDefinition(EscapeCommand.ClearLineFromCursor, @"\[M"),       // Not found in ANSI definition
            new EscapeSequenceDefinition(EscapeCommand.ClearLineFromCursor, @"\[K"),
            new EscapeSequenceDefinition(EscapeCommand.ClearLineFromCursor, @"\[0K"),
            new EscapeSequenceDefinition(EscapeCommand.ClearLineToCursor, @"\[1K"),
            new EscapeSequenceDefinition(EscapeCommand.ClearEntireLine, @"\[2K"),
            new EscapeSequenceDefinition(EscapeCommand.ClearScreenFromCursor, @"\[J"),
            new EscapeSequenceDefinition(EscapeCommand.ClearScreenFromCursor, @"\[0J"),
            new EscapeSequenceDefinition(EscapeCommand.ClearScreenToCursor, @"\[1J"),
            new EscapeSequenceDefinition(EscapeCommand.ClearScreen, @"\[2J"),
        };

        internal static EscapeSequence Match(string text, out int length)
        {
            foreach (var seq in Sequences)
            {
                var match = seq.Match(text);

                if (match.Success)
                {
                    length = match.Length;
                    return new EscapeSequence(seq, match);
                }
            }

            length = 0;
            return null;
        }
    }

}
