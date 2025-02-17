using System.Text.RegularExpressions;

namespace VaultDebug.Runtime.Logger
{
    public static class StringExtensions
    {
        public static bool IsMatch(this string str, string pattern)
        {
            return Regex.IsMatch(str, pattern);
        }

        public static GroupCollection MatchOnce(this string str, string pattern)
        {
            var match = Regex.Match(str, pattern);

            if (match.Success)
            {
                return match.Groups;
            }

            return null;
        }

        public static MatchCollection MatchAll(this string str, string pattern)
        {
            var matchCollection = Regex.Matches(str, pattern);
            return matchCollection;
        }
    }
}