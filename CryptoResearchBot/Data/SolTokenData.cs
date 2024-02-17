using CryptoResearchBot.Core.Data;
using CryptoResearchBot.Core.Parser;
using Telegram.Bot.Types;

namespace CryptoResearchBot.SOL.Data
{
    internal class SolTokenData : BaseTokenData
    {
        public SolTokenInfo TokenInformation { get; private set; }

        public bool IsReady => TokenInformation.BurnPercentage.Contains("100");
        public bool IsTokenStarted => TokenInformation.TradingStartTime < DateTime.Now;

        public string Age => IsTokenStarted ? $"{(int)((DateTime.Now - TokenInformation.TradingStartTime).TotalMinutes)} minutes" : $"Not started: {TokenInformation.TradingStartTime.ToString("HH:mm")}";

        public SolTokenData(SolTokenInfo tokenInformation, string ownerId) : base(tokenInformation.Id, tokenInformation.Name, ownerId)
        {
            TokenInformation = tokenInformation;
        }

        public string GetInformation()
        {
            var baseInformation = $"""
                {MarkdownParser.GetConvertedText(Name)} 

                `{Id}`

                Age: {MarkdownParser.GetConvertedText(Age)}
                Liquidity: {MarkdownParser.GetConvertedText(TokenInformation.Liquidity)}🔥\({TokenInformation.BurnPercentage}\)
                Mint: {(TokenInformation.MintAuthority ? "✅" : "❌")}

                {MarkdownParser.GetConvertedText(TokenInformation.SocialBlock)}

                {MarkdownParser.GetConvertedText(TokenInformation.TopHoldersBlock)}

                {MarkdownParser.GetConvertedText(TokenInformation.WarningsBlock)}

                [📈DexTools](https://www.dextools.io/app/en/solana/pair-explorer/{TokenInformation.Id}) \| [📈Dexscreen](https://dexscreener.com/solana/{TokenInformation.Id}) \| [⚖️Owner](https://solscan.io/account/{Owner})
                """;

            return baseInformation;
        }

        internal static BaseTokenData? CreateTokenData(Message message)
        {
            var messageText = message.Text;
            if (messageText is null)
                return null;
            if (message.Entities is null)
                return null;

            string[] groupsArray = messageText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var solscanEntity = message.Entities.FirstOrDefault(x => x.Url is not null && x.Url.Contains("solscan"));
            if (solscanEntity is null || solscanEntity.Url is null)
                return null;

            //https://solscan.io/account/
            var name = groupsArray[0];
            var id = groupsArray[1];
            var owner = solscanEntity.Url.Substring(27);
            return new SolTokenData(new SolTokenInfo() { Id = id, Name = name}, owner);
        }
    }
}
