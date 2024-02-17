using CryptoResearchBot.Core.Data;

namespace CryptoResearchBot.Network.SOL.Data
{
    public class SolWatchingTopicData : BaseWatchingTopicData
    {
        public SolWatchingTopicData(int id, ChannelInformation channelInformation, BaseTokenData? tokenData) : base(id, channelInformation, tokenData)
        {
        }

        protected override string GetBottomInformation()
        {
            return Token is null ?
                "📈DexTools \\| 📈Dexscreen \\| ⚖️Owner"
                :
                $"[📈DexTools](https://www.dextools.io/app/en/solana/pair-explorer/{Token.Id}) \\| [📈Dexscreen](https://dexscreener.com/solana/{Token.Id}) \\| [⚖️Owner](https://solscan.io/account/{Token.Owner})";
        }
    }
}
