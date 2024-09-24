using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EffigyMaker.CLI
{
    public class DotaMatchCosmetics
    {
        public static async Task<int[]> GetCostmeticIds(int playerId, int heroId)
        {
            var httpClient = new HttpClient();

            var match_id = await GetLastMatchIdAsHero(playerId, heroId);

            var url = $"https://api.opendota.com/api/matches/{match_id}";

            var response = await httpClient.GetAsync(url);

            var match = JObject.Parse(await response.Content.ReadAsStringAsync());

            var player = match["players"].First(player => player.Value<int>("hero_id") == heroId);

            var cosmetics = ((JObject)match["cosmetics"])
                .ToObject<Dictionary<int, int>>()
                .Where(kv => kv.Value == player.Value<int>("player_slot"))
                .Select(kv => kv.Key)
                .ToArray();

            return cosmetics;
        }

        public static async Task<string> GetLastMatchIdAsHero(int playerId, int heroId)
        {
            var httpClient = new HttpClient();

            var url = $"https://api.opendota.com/api/players/{playerId}/matches?limit=1&hero_id={heroId}";

            var response = await httpClient.GetAsync(url);
            var matches = JArray.Parse(await response.Content.ReadAsStringAsync());
            if (matches.Count != 1)
            {
                throw new Exception("Couldn't find a match where that player played that hero");
            }
            return matches[0].Value<string>("match_id");
        }
    }
}
