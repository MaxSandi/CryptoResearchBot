using Solnet.Metaplex.NFT.Library;
using Solnet.Rpc;
using Solnet.Wallet;

namespace CryptoResearchBot.SOL.Helper
{
    internal class SolanaRPCHelper
    {
        private static IRpcClient _rpcClient = ClientFactory.GetClient(Cluster.MainNet);

        public static async Task<string?> GetOwnerAsync(string tokenId)
        {
            var accountMetadata = await MetadataAccount.GetAccount(_rpcClient, new PublicKey(tokenId));
            if (accountMetadata is null)
                return null;

            return accountMetadata.updateAuthority.ToString();
        }
    }
}
