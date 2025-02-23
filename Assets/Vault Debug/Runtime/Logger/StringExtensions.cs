using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace VaultDebug.Runtime.Logger
{
    /// <summary>
    /// Provides extension methods for string matching and JSON conversion.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Determines whether the string matches the given regular expression pattern.
        /// </summary>
        /// <param name="str">The input string.</param>
        /// <param name="pattern">The regular expression pattern.</param>
        /// <returns><c>true</c> if the string matches the pattern; otherwise, <c>false</c>.</returns>
        public static bool IsMatch(this string str, string pattern)
        {
            return Regex.IsMatch(str, pattern);
        }

        /// <summary>
        /// Matches the string against a regular expression pattern once.
        /// </summary>
        /// <param name="str">The input string.</param>
        /// <param name="pattern">The regular expression pattern.</param>
        /// <returns>
        /// A <see cref="GroupCollection"/> containing the match groups if a match is found; otherwise, <c>null</c>.
        /// </returns>
        public static GroupCollection MatchOnce(this string str, string pattern)
        {
            var match = Regex.Match(str, pattern);

            if (match.Success)
            {
                return match.Groups;
            }

            return null;
        }

        /// <summary>
        /// Matches the string against a regular expression pattern and returns all matches.
        /// </summary>
        /// <param name="str">The input string.</param>
        /// <param name="pattern">The regular expression pattern.</param>
        /// <returns>A <see cref="MatchCollection"/> containing all matches.</returns>
        public static MatchCollection MatchAll(this string str, string pattern)
        {
            var matchCollection = Regex.Matches(str, pattern);
            return matchCollection;
        }

        /// <summary>
        /// Converts an object to its JSON string representation.
        /// </summary>
        /// <param name="o">The object to convert.</param>
        /// <returns>A JSON string representing the object.</returns>
        public static string GetJsonString(this object o)
        {
            return JsonConvert.SerializeObject(o, Formatting.None);
        }
    }
}
