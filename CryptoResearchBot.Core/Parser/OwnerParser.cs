using System.Text.RegularExpressions;

namespace CryptoResearchBot.Core.Parser
{
    public static class OwnerParser
    {
        public static bool IsExistedOwner(string ownerName)
        {
            return ownerName.Contains("админ") || ownerName.Contains("овнер");
        }

        public static string CreateNewOwnerName(string preName, string ownerName, bool flag)
        {
            if (IsExistedOwner(ownerName))
            {
                string pattern = @"\(([-+]?\d+(\s*[-+]\s*\d+)*)\)";
                Regex regex = new Regex(pattern);

                Match match = regex.Match(ownerName);
                if (!match.Success)
                    return $"{ownerName} {GetRank(flag)}";

                var rank = match.Value;

                string pattern2 = @"\(([-+]?\d+)\s*([+-]?\s*\d*)\)";
                Regex regex2 = new Regex(pattern2);
                Match match2 = regex2.Match(rank);
                if (!match2.Success)
                    return $"{ownerName} {GetRank(flag)}";

                var positiveRank = 0;
                var negativeRank = 0;

                var value1 = int.Parse(match2.Groups[1].Value);
                if (int.TryParse(match2.Groups[2].Value, out int value2))
                {
                    // два значения
                    positiveRank = value1;
                    negativeRank = Math.Abs(value2);
                }
                else
                {
                    if (value1 > 0)
                        positiveRank = value1;
                    else
                        negativeRank = Math.Abs(value1);
                }

                if (flag)
                    positiveRank++;
                else
                    negativeRank++;

                string newRank = GetRank(positiveRank, negativeRank);

                return ownerName.Replace(rank, newRank);
            }
            else
            {
                return $"{preName} {ownerName} {GetRank(flag)}";
            }
        }

        private static string GetRank(bool flag)
        {
            var positiveRank = flag ? 1 : 0;
            var negativeRank = flag ? 0 : 1;

            return $"({(positiveRank != 0 ? $"+{positiveRank}" : "")} {(negativeRank != 0 ? $"-{negativeRank}" : "")})";
        }
        private static string GetRank(int positive, int negative)
        {
            return $"({(positive != 0 ? $"+{positive}" : "")}{(negative != 0 ? $" -{negative}" : "")})";
        }
    }
}
