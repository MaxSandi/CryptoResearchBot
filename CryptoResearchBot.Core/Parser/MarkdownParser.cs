using System.Text.RegularExpressions;

namespace CryptoResearchBot.Core.Parser
{
    public static class MarkdownParser
    {
        public static string GetConvertedText(string input)
        {
            char[] specialChars = { '_', '*', '[', ']', '(', ')', '~', '`', '>', '#', '+', '-', '=', '|', '{', '}', '.', '!' };

            foreach (char c in specialChars)
            {
                string pattern = Regex.Escape(c.ToString());
                input = Regex.Replace(input, pattern, "\\" + c);
            }

            return input;
        }
    }
}
