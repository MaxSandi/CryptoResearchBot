namespace CryptoResearchBot.SOL.Data
{
    internal class SolTokenInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime TradingStartTime { get; set; }
        public string Liquidity { get; set; }
        public string BurnPercentage { get; set; }
        public bool MintAuthority { get; set; }
        public string SocialBlock { get; set; }
        public string TopHoldersBlock { get; set; }
        public string WarningsBlock { get; set; }
    }
}
