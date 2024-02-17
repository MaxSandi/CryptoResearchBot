using TL;

namespace CryptoResearchBot.Core.Data
{
    [Serializable]
    public class UserInformation
    {
        public string Name { get; set; }

        public long Id { get; set; }
        public long AccessHash { get; set; }

        public UserInformation()
        {
            
        }
        public UserInformation(string name, long id, long accessHash)
        {
            Name = name;
            Id = id;
            AccessHash = accessHash;
        }
        public UserInformation(string name, InputUser data)
        {
            Name = name;
            Id = data.user_id;
            AccessHash = data.access_hash;
        }

        public override bool Equals(object? other)
        {
            if (other is not UserInformation userInformation)
                return false;

            return Name.Equals(userInformation.Name) && Id == userInformation.Id && AccessHash == userInformation.AccessHash;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, AccessHash);
        }

        public static implicit operator InputUser(UserInformation userInformation)
        {
            return new InputUser(userInformation.Id, userInformation.AccessHash);
        }

    }

    [Serializable]
    public class ChannelInformation
    {
        public long Id { get; set; }
        public long AccessHash { get; set; }

        public string Name { get; set; }
        public string TelegramGroupLink { get; set; }

        public int InitialUserCount { get; set; }
        public int CurrentUserCount { get; set; }

        public UserInformation? Owner { get; set; }
        public IEnumerable<UserInformation> Admins { get; set; }
        public IEnumerable<UserInformation> Contacts { get; set; }

        public ChannelInformation(long channelId, long acessHash, string name, int userCount, UserInformation? owner, IEnumerable<UserInformation> admins, IEnumerable<UserInformation> contacts)
        {
            Id = channelId;
            AccessHash = acessHash;

            Name = name;
            InitialUserCount = userCount;
            Owner = owner;
            CurrentUserCount = userCount;
            Admins = admins;
            Contacts = contacts;
        }

        public override string ToString()
        {
            // Telegram link
            // Initial user count
            // Admins/owner
            var adminsBlock = string.Empty;
            foreach (var admin in Admins)
                adminsBlock += $"\n - {admin.Name}";

            var contactsBlock = string.Empty;
            foreach (var contact in Contacts)
                contactsBlock += $"\n - {contact.Name}";

            var text = $"""
                👥 Initial user count: {InitialUserCount}. 
                👥 Current user count: {CurrentUserCount}.
                🔑 Owner: {(Owner is null ? "-" : Owner.Name)}
                🔑 Admins: {(string.IsNullOrEmpty(adminsBlock) ? "-" : adminsBlock)}
                🔗 Contacts: {(string.IsNullOrEmpty(contactsBlock) ? "-" : contactsBlock)}                
                """;

            return text;
        }

        public static implicit operator InputChannel(ChannelInformation channelInformation)
        {
            return new InputChannel(channelInformation.Id, channelInformation.AccessHash);
        }

        public ChannelInformation Clone()
        {
            return new ChannelInformation(Id, AccessHash, Name,  InitialUserCount, Owner, Admins, Contacts);
        }
    }
}
