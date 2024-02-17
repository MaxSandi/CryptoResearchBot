using CryptoResearchBot.Core.Interfaces;
using CryptoResearchBot.Core.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace CryptoResearchBot.Core.Data
{
    [Serializable]
    public abstract class BaseTokenData : ITokenData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }

        public BaseTokenData(string id, string name, string owner)
        {
            Id = id;
            Name = name;
            Owner = owner;
        }
    }
}
