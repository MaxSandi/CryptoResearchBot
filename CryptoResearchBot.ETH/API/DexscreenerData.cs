using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoResearchBot.ETH.API
{
    internal class TokenPairs
    {
        public string schemaVersion { get; set; }
        public List<TokenPair> pairs { get; set; }
    }

    internal class TokenPair
    {
        public string chainId { get; set; }
        public string dexId { get; set; }
        public string url { get; set; }
        public string pairAddress { get; set; }
        public List<string> labels { get; set; }
        public Token baseToken { get; set; }
        public Token quoteToken { get; set; }
        public string priceNative { get; set; }
        public string priceUsd { get; set; }
        public TransactionStats txns { get; set; }
        public VolumeData volume { get; set; }
        public PriceChangeData priceChange { get; set; }
        public LiquidityData liquidity { get; set; }
        public int fdv { get; set; }
        public long pairCreatedAt { get; set; }
        public InfoData info { get; set; }
    }

    internal class Token
    {
        public string address { get; set; }
        public string name { get; set; }
        public string symbol { get; set; }
    }

    internal class TransactionStats
    {
        public int buys { get; set; }
        public int sells { get; set; }
    }

    internal class VolumeData
    {
        public double h24 { get; set; }
        public double h6 { get; set; }
        public double h1 { get; set; }
        public double m5 { get; set; }
    }

    internal class PriceChangeData
    {
        public double m5 { get; set; }
        public double h1 { get; set; }
        public double h6 { get; set; }
        public double h24 { get; set; }
    }

    internal class LiquidityData
    {
        public double usd { get; set; }
        public long @base { get; set; }
        public double quote { get; set; }
    }

    internal class InfoData
    {
        public string imageUrl { get; set; }
        public List<WebsiteData> websites { get; set; }
        public List<SocialData> socials { get; set; }
    }

    internal class WebsiteData
    {
        public string label { get; set; }
        public string url { get; set; }
    }

    internal class SocialData
    {
        public string type { get; set; }
        public string url { get; set; }
    }
}
