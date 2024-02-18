namespace CryptoResearchBot.SOL.Data
{
    internal class SolTokenInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime TradingStartTime { get; set; }
        public string Liquidity { get; set; } = string.Empty;
        public string BurnPercentage { get; set; } = string.Empty;
        public bool MintAuthority { get; set; }
        public string SocialBlock { get; set; } = string.Empty;
        public string TopHoldersBlock { get; set; } = string.Empty;
        public string WarningsBlock { get; set; } = string.Empty;
    }
}
