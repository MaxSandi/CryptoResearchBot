using CryptoResearchBot.Core.Parser;

namespace CryptoResearchBot.Core.Data
{
    [Serializable]
    public abstract class BaseWatchingTopicData
    {
        public int Id { get; set; }
        public int MainMessageId { get; set; }
        public int? TotalCallMessageId { get; set; }

        public BaseTokenData? Token { get; set; }
        public ChannelInformation ChannelInformation { get; set; }

        public BaseWatchingTopicData(int id, ChannelInformation channelInformation, BaseTokenData? tokenData)
        {
            Id = id;
            ChannelInformation = channelInformation;
            Token = tokenData;
        }

        public string GetMainInformation()
        {
            return Token is null ?
                $"""
                CA undifined
                {MarkdownParser.GetConvertedText(ChannelInformation.ToString())}
                📈DexTools \| 📈Dexscreen \| ⚖️Owner
                """
                :
                $"""
                {MarkdownParser.GetConvertedText(Token.Name)} 
                {MarkdownParser.GetConvertedText(ChannelInformation.ToString())}
                {GetBottomInformation()}
                """;
        }

        protected abstract string GetBottomInformation();
    }
}
