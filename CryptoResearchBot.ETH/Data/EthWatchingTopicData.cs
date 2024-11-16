using CryptoResearchBot.Core.Data;

namespace CryptoResearchBot.ETH.Data
{
    internal class EthWatchingTopicData : BaseWatchingTopicData
    {
        public EthWatchingTopicData(int id, ChannelInformation channelInformation, BaseTokenData? tokenData) : base(id, channelInformation, tokenData)
        {
        }

        protected override string GetBottomInformation()
        {
            return Token is null ?
                "📈DexTools \\| 📈Dexscreen \\| ⚖️Owner"
                :
                $"[📈DexTools](https://www.dextools.io/app/en/ether/pair-explorer/{Token.Id}) \\| [📈Dexscreen](https://dexscreener.com/ethereum/{Token.Id})";
        }
    }
}
