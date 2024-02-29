namespace CryptoResearchBot.ETH.API
{
    internal static class DexscreenerAPI
    {
        public static async Task<TokenPair?> GetTokenInformationAsync(string tokenID)
        {
            string apiEndpoint = $"https://api.dexscreener.com/latest/dex/tokens/{tokenID}";
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(apiEndpoint);
                    if (!response.IsSuccessStatusCode)
                        throw new Exception($"Response status code {response.StatusCode}");

                    string responseData = await response.Content.ReadAsStringAsync();
                    var tokenPairs = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenPairs>(responseData);
                    if (tokenPairs is null)
                        throw new Exception($"Can't json parse - {responseData}");

                    return tokenPairs.pairs.Where(x => x.quoteToken.symbol.Equals("WETH")).First();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DexscreenerAPI ERROR: {ex.Message}");
                    return null;
                }
            }
        }
    }
}
