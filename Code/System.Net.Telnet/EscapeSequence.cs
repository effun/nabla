using System.Text.RegularExpressions;

namespace System.Net.Telnet
{
    class EscapeSequence
    {
        private readonly Match _match;

        public EscapeSequence(EscapeSequenceDefinition definition, Match match)
        {
            Definition = definition;
            _match = match;
        }

        public EscapeSequenceDefinition Definition { get; }

        public EscapeCommand Command => Definition.Command;

        public string GetParameter(int index)
        {
            if (index < 0 || index >= _match.Groups.Count)
                throw new ArgumentOutOfRangeException();

            string value = _match.Groups[index + 1].Value;

            if (string.IsNullOrEmpty(value))
                value = Definition.DefaultValues[index];

            return value;
        }

        public int GetParameterInt32(int index = 0)
        {
            string value = GetParameter(index);

            return int.Parse(value);
        }

        public string Text => _match.Value;
    }

}
