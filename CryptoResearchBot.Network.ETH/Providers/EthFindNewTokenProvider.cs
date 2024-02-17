using CryptoResearchBot.Core.Interfaces;
using CryptoResearchBot.Core.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace CryptoResearchBot.Network.ETH.Providers
{
    internal class EthFindNewTokenProvider : IFindNewTokenProvider
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public Task<IEnumerable<ITokenData>> GetNewTokensAsync()
        {
            throw new NotImplementedException();
        }
    }
}
