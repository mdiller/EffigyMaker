using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EffigyMaker.Core
{
    /// <summary>
    /// The information about a hero cosmetic
    /// </summary>
    public class HeroItemInfo
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public string Slot { get; private set; }
        public string ModelPath { get; private set; }
        public string HeroName { get; private set; }
        public bool IsDefault { get; private set; }

        /// <summary>
        /// Parses a the info about a hero cosmetic from json
        /// </summary>
        /// <param name="id">The id of the cosmetic</param>
        /// <param name="data">The json data about the cosmetic</param>
        /// <returns>The constructed information about the cosmetic</returns>
        public static HeroItemInfo FromJson(string id, JObject data)
        {
            var usedByDict = data.Value<JObject>("used_by_heroes");
            string heroName = null;
            if (usedByDict != null && usedByDict.Count > 0)
            {
                heroName = usedByDict.Properties().Select(p => p.Name).First();
            }
            return new HeroItemInfo
            {
                Id = int.Parse(id),
                Name = data.Value<string>("name"),
                Slot = data.Value<string>("item_slot"),
                ModelPath = data.Value<string>("model_player"),
                HeroName = heroName,
                IsDefault = data.Value<string>("prefab") == "default_item"
            };
        }
    }
}
