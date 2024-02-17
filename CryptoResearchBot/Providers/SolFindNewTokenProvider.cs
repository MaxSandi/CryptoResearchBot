using CryptoResearchBot.Core.Interfaces;
using CryptoResearchBot.Core.Providers;
using CryptoResearchBot.Core.TelegramAPI;
using CryptoResearchBot.SOL.Data;
using CryptoResearchBot.SOL.Helper;
using System.Collections.Concurrent;
using TL;

namespace CryptoResearchBot.SOL
{
    internal class SolFindNewTokenProvider : IFindNewTokenProvider
    {
        private ConcurrentBag<SolTokenData> _readyTokens = new();

        public SolFindNewTokenProvider()
        {
            ChatListener.AddWatchingChat(2014070659, ParseTokenMessage);
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public Task<IEnumerable<ITokenData>> GetNewTokensAsync()
        {
            var list = new List<ITokenData>();
            while (_readyTokens.TryTake(out var tokenData))
                list.Add(tokenData);

            return Task.FromResult((IEnumerable<ITokenData>)list);
        }

        private void ParseTokenMessage(long chatId, Message messageEntity)
        {
            Task.Run(async () =>
            {
                try
                {
                    SolTokenInfo? solTokenInfo;
                    switch (chatId)
                    {
                        case 2014070659:
                            solTokenInfo = TokenParser.ParseTokenInformationFrom_solanaburns(messageEntity.message);
                            break;
                        case 1983454858:
                            solTokenInfo = TokenParser.ParseTokenInformationFrom_solana_tracker(messageEntity.message);
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    if (solTokenInfo is not null)
                    {
                        var ownerId = await SolanaRPCHelper.GetOwnerAsync(solTokenInfo.Id);
                        if (ownerId is null)
                        {
                            Console.WriteLine($"GetOwnerAsync return null Token: {solTokenInfo.Id}");
                            return;
                        }

                        _readyTokens.Add(new SolTokenData(solTokenInfo, ownerId));
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine($"ParseTokenMessage ERROR: {e.Message}");
                }
            });

        }
    }
}
