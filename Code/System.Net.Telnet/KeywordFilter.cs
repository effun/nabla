using System.Diagnostics;
using System.Text.RegularExpressions;

namespace System.Net.Telnet
{
    [DebuggerDisplay("{Keyword}, IsRegex = {IsRegex}")]
    public class KeywordFilter
    {
        public KeywordFilter(string keyword, bool isRegex)
        {
            Keyword = keyword;
            IsRegex = isRegex;
        }

        public KeywordFilter(string keyword)
            : this(keyword, false)
        {

        }

        public KeywordFilter(Regex regex)
        {
            Keyword = regex.ToString();
            IsRegex = true;
            _regex = regex;
        }

        public string Keyword { get; }

        public bool IsRegex { get; }

        public bool IgnoreCase { get; set; }

        public Match Match { get; private set; }

        public string MatchedInput { get; private set; }

        public bool IsMatch(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            bool isMatch;

            if (IsRegex)
                isMatch = MatchRegex(input);
            else
                isMatch = MatchPlainText(input);

            if (isMatch)
                MatchedInput = input;

            return isMatch;
        }

        private bool MatchPlainText(string input)
        {
            StringComparison sc = IgnoreCase ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;

            return input.IndexOf(Keyword, sc) >= 0;
        }

        private Regex _regex;

        private bool MatchRegex(string input)
        {
            if (_regex == null)
            {
                _regex = new Regex(Keyword, IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
            }

            var match = _regex.Match(input);

            if (match.Success)
                Match = match;

            return match.Success;
        }
    }
}
