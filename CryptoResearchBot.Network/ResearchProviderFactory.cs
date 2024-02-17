using CryptoResearchBot.Core.Interfaces;

namespace CryptoResearchBot.Core.Network
{
    public static class ResearchProviderFactory
    {
        public static IResearchProvider GetResearchProvider(long chatId)
        {
            return new EthResearchProvider();
        }
    }
}
