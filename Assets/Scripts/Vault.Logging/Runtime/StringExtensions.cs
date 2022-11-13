using System.Text.RegularExpressions;

namespace Vault.Logging.Runtime
{
    public static class StringExtensions
    {
        public static bool IsMatch(this string str, string pattern)
        {
            return Regex.IsMatch(str, pattern);
        }

        public static GroupCollection MatchPattern(this string str, string pattern)
        {
            var match = Regex.Match(str, pattern);

            if (match.Success)
            {
                return match.Groups;
            }

            return null;
        }

        public static MatchCollection MatchAllPatterns(this string str, string pattern)
        {
            var matchCollection = Regex.Matches(str, pattern);
            return matchCollection;
        }
    }
}