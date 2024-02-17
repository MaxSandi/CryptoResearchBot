using CryptoResearchBot.Core.Providers;
using CryptoResearchBot.Network;

namespace CryptoResearchBot.Core.Network
{
    public static class ResearchProviderFactory
    {
        public static IResearchProvider CreateResearchProvider(NetworkType networkType)
        {
            return networkType switch
            {
                NetworkType.ETH => new EthResearchProvider(),
                NetworkType.SOL => new SolResearchProvider(),
                _ => throw new NotImplementedException(),
            };
        }
    }
}
