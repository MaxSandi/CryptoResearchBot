using CryptoResearchBot.SOL.Data;
using System.Text.RegularExpressions;

namespace CryptoResearchBot.SOL.Helper
{
    internal class TokenParser
    {
        #region https://t.me/solanaburns
        public static SolTokenInfo? ParseTokenInformationFrom_solanaburns(string inputText)
        {
            //PEPE AI(PEPEAI)

            //🔥 Burn Percentage: 100 %
            //🕒 Trading Start Time: 59 seconds ago

            //📊 Marketcap: $1.64K
            //💧 Liquidity: $2.60K(159.01 %)
            //💵 Price: $0.000001638

            //🚀 Launch MC: $1.03K(x2)
            //📦 Total Supply: 1.00B

            //🌐 Socials:
            //・https://www.pepeaisol.xyz
            //・https://twitter.com/PepeAIonSOL
            //・https://t.me/PEPEAISol
            //⚙️ Security:
            //├ Mutable Metadata: Yes ❌
            //├ Mint Authority: Yes ❌
            //└ Freeze Authority: No ✅

            //🏦 Top Holders:
            //├ Raydium AMM | 775.66M | 77.57 %
            //├ Di1s...VYo8 | 35.57M | 3.56 %
            //├ 8sA4...dg6m | 21.07M | 2.11 %
            //├ 7tP2...wGEw | 19.41M | 1.94 %
            //└ 2zGJ...498w | 17.97M | 1.80 %

            //🧠 Score: Bad(2 issues) 🔴🔴🔴
            //🟥 Mint Authority still enabled
            //🟧 Mutable metadata

            //Solscan | Birdeye | Dexscreener | Photon

            //M4BRxogujzkxvrGY9dA3VGo1vnoTkfMzD27YUWCEwUH
            //📣 AD: Your ad here? Contact us

            SolTokenInfo tokenInfo = new SolTokenInfo();

            // Разделяем текст на строки
            string[] groupsArray = inputText.Split(new string[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);

            tokenInfo.Name = groupsArray[0];
            Console.WriteLine($"Accept {tokenInfo.Name}");

            {
                string[] lines = groupsArray[8].Split('\n');
                tokenInfo.Id = lines[0];
            }

            {
                string[] lines = groupsArray[2].Split('\n');
                var liquidityLine = lines[1];

                tokenInfo.Liquidity = liquidityLine.Split(':')[1].Trim();
            }

            {
                string[] lines = groupsArray[1].Split('\n');
                var burnLine = lines[0];
                var timeLine = lines[1];

                tokenInfo.BurnPercentage = burnLine.Split(':')[1].Trim();
                tokenInfo.TradingStartTime = ParseTimeString(timeLine.Split(':')[1].Trim());
            }

            {
                string[] lines = groupsArray[4].Split('\n');
                if(!lines[0].Contains("🌐 Socials: ⚙️ Security:"))
                {
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (lines[i].Contains("🌐 Socials:"))
                            continue;
                        if (lines[i].Contains("⚙️ Security:"))
                            break;

                        tokenInfo.SocialBlock += lines[i] + "\n";
                    }
                }
                tokenInfo.SocialBlock = tokenInfo.SocialBlock.TrimEnd('\n');
                if (string.IsNullOrEmpty(tokenInfo.SocialBlock) || tokenInfo.SocialBlock.Contains("No links found"))
                    return null;

                var mintLine = lines.FirstOrDefault(line => line.Contains("Mint Authority"));
                if (mintLine is null)
                    return null;

                string[] parts = mintLine.Split(':');
                tokenInfo.MintAuthority = parts[1].Trim().Contains("Yes") ? true : false;
            }

            tokenInfo.TopHoldersBlock = groupsArray[5];

            {
                string[] lines = groupsArray[6].Split('\n').Where(x => x.Contains("Creator owns") || x.Contains("Single holder") || x.Contains("Creator sent")).ToArray();

                if(lines.Length > 0)
                {
                    tokenInfo.WarningsBlock = "🧠 Warnings:";
                    foreach (var line in lines)
                        tokenInfo.WarningsBlock += "\n" + line;
                }
            }

            return tokenInfo;
        }

        #endregion

        #region https://t.me/solana_tracker
        static public SolTokenInfo? ParseTokenInformationFrom_solana_tracker(string inputText)
        {
            //🔥🔥🔥 BURN 🔥🔥🔥 

            //Harambe the Gorilla($HARAMBE)

            //🪙 CA: 7w2sLLuetGFGGtFkYtJePHdyaNsRNzXQrfFsTYqWzz6z
            //💡 Market Cap: $7.64K
            //💧 Liquidity: $2.34K
            //⛽ Pooled SOL: 26.5 SOL
            //🔥 Burn: 100 %
            // ✅ Mint Disabled


            //📖 Description:
            //            Harambe is the ultimate Gorilla, Missed by Andrew Tate and the rest of the Apes, he was a compassionate Leader who showed mercy upon the human species.

            //            Website: https://harambesolana.com
            //        Telegram: https://t.me/HarambeCoinSOL
            //        Twitter: https://twitter.com/HarambeCoinSOL

            //📈 DexScreen | 📈 Dextools | 📈 Birdeye | 🔥 Raydium |  ⚖️ Owner |  ⚖️ Pair 

            SolTokenInfo tokenInfo = new SolTokenInfo();

            // Разделяем текст на строки


            string[] groupsArray = inputText.Split(new string[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (!groupsArray[0].Contains("BURN"))
                return null;

            tokenInfo.Name = groupsArray[1];

            {
                string[] lines = groupsArray[2].Split('\n');

                tokenInfo.Id = lines[0].Split(':')[1].Trim();
                tokenInfo.Liquidity = lines[2].Split(':')[1].Trim();
                tokenInfo.BurnPercentage = lines[4].Split(':')[1].Trim();
                tokenInfo.TradingStartTime = DateTime.Now;
                tokenInfo.MintAuthority = lines[5].Trim().Contains("Disabled") ? true : false;
            }

            tokenInfo.TopHoldersBlock = string.Empty;
            tokenInfo.SocialBlock = string.Empty;

            var descriptionBlock = groupsArray.SkipWhile(x => !x.Contains("📖 Description:"));
            if(descriptionBlock is not null)
            {
                descriptionBlock = descriptionBlock.TakeWhile(x => !x.Contains("📈 DexScreen"));
                if(descriptionBlock is not null)
                {
                    var description = string.Join("", descriptionBlock);
                    var socials = ParseMetadataSocials(description);
                    if(socials.Any())
                    {
                        tokenInfo.SocialBlock = string.Join("\n", socials);
                    }
                }
            }
            
            if (string.IsNullOrEmpty(tokenInfo.SocialBlock))
                return null;

            return tokenInfo;
        }

        static private IEnumerable<string> ParseMetadataSocials(string description)
        {
            var telegramLinks = ParseTelegram(description);
            var twitterLinks = ParseTwitter(description);
            var websiteLinks = ParseWeb(description, telegramLinks, twitterLinks);

            return telegramLinks.Concat(twitterLinks).Concat(websiteLinks).Distinct();
        }

        static private IEnumerable<string> ParseTelegram(string v)
        {
            string pattern = @"(?:t\.me/|@)(\w+)";

            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection matches = regex.Matches(v);

            var telegramLinks = new List<string>();
            foreach (Match match in matches)
            {
                var link = match.Groups[1].Value;
                if (!link.Contains("t.me/"))
                {
                    link = $"t.me/{link}";
                }

                if (!link.Contains("https://"))
                {
                    link = $"https://{link}";
                }

                telegramLinks.Add(link);
            }

            return telegramLinks;
        }

        static private IEnumerable<string> ParseTwitter(string v)
        {
            string pattern = @"(?:x\.com/|twitter\.com/)(\w+)";

            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection matches = regex.Matches(v);

            var telegramLinks = new List<string>();
            foreach (Match match in matches)
            {
                var link = match.Groups[1].Value;
                if (!link.Contains("t.me/"))
                {
                    link = $"t.me/{link}";
                }

                if (!link.Contains("https://"))
                {
                    link = $"https://{link}";
                }

                telegramLinks.Add(link);
            }

            return telegramLinks;
        }

        static private IEnumerable<string> ParseWeb(string description, IEnumerable<string> telegram, IEnumerable<string> twitter)
        {
            string pattern = @"(?:https?://[^\s]+)";

            // Создаем объект регулярного выражения
            Regex regex = new Regex(pattern);

            // Ищем соответствия в строке
            MatchCollection matches = regex.Matches(description);

            return matches.Where(x => !telegram.Any(telegramLink => telegramLink.Contains(x.Value)) && !twitter.Any(twitterLink => twitterLink.Contains(x.Value))).Select(x => x.Value);
        }

        #endregion

        static private DateTime ParseTimeString(string input)
        {
            // Приводим все к нижнему регистру для удобства
            string lowerInput = input.ToLower();

            // Проверяем, содержит ли строка "ago"
            bool isAgo = lowerInput.Contains("ago");

            // Извлекаем числовые значения из строки
            int minutes = ExtractMinutes(lowerInput);
            int hours = ExtractHours(lowerInput);

            // Вычисляем смещение времени
            TimeSpan offset = isAgo ? TimeSpan.FromMinutes(-minutes - hours * 60) : TimeSpan.FromMinutes(minutes + hours * 60);

            // Возвращаем текущее время плюс смещение
            return DateTime.Now.Add(offset);
        }
        static private int ExtractMinutes(string input)
        {
            int minutes = 0;
            string minutesPattern = @"(\d+) minute";
            Match match = Regex.Match(input, minutesPattern);

            if (match.Success)
            {
                minutes = int.Parse(match.Groups[1].Value);
            }

            return minutes;
        }
        static private int ExtractHours(string input)
        {
            int hours = 0;
            string hoursPattern = @"(\d+) hour";
            Match match = Regex.Match(input, hoursPattern);

            if (match.Success)
            {
                hours = int.Parse(match.Groups[1].Value);
            }

            return hours;
        }
    }
}
