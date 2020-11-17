using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ValveResourceFormat.IO;
using ValveResourceFormat.Serialization.KeyValues;

namespace EffigyMaker.Core
{
    /// <summary>
    /// Represents information about a single hero
    /// </summary>
    public class HeroInfo
    {
        private static readonly string npcScriptsPath = "scripts/npc/npc_heroes.txt";
        private static readonly string itemsScriptsPath = "scripts/items/items_game.txt";
        private static readonly string dotaEnglishPath = "resource/localization/dota_english.txt";

        public HeroInfo()
        {

        }

        public int Id { get; private set; }
        public string Name { get; private set; }
        public string LocalizedName { get; private set; }
        public string ModelPath { get; private set; }
        public List<JObject> ItemSlots { get; private set; }
        public List<HeroItemInfo> Cosmetics { get; private set; } = new List<HeroItemInfo>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        private static HeroInfo FromJson(string name, JObject json, JObject langFile)
        {
            return new HeroInfo
            {
                Id = int.Parse(json.Value<string>("HeroID")),
                Name = name,
                LocalizedName = langFile.ContainsKey(name) ? langFile.Value<string>(name) : name,
                ModelPath = json.Value<string>("Model"),
                ItemSlots = (json["ItemSlots"] as JObject).Values().Cast<JObject>().ToList(),
            };
        }

        /// <summary>
        /// Loads the list of hero infos from the vpk
        /// </summary>
        /// <param name="vpkLoader">The vpk file loader</param>
        /// <returns>The loaded hero information</returns>
        public static List<HeroInfo> LoadFromVpk(BasicVpkFileLoader vpkLoader)
        {
            var kvLoader = new KvFileLoader(vpkLoader);
            //var heroScripts = vpkLoader.LoadFile(npcScriptsPath);
            var langJson = (kvLoader.LoadFile(dotaEnglishPath)["lang"]["Tokens"] as JObject);
            var heroJson = kvLoader.LoadFile(npcScriptsPath);

            var heroInfos = new List<HeroInfo>();
            foreach (var heroData in (heroJson["DOTAHeroes"] as JObject))
            {
                var ignoreKeys = new List<string>
                {
                    "Version",
                    "npc_dota_hero_target_dummy",
                    "npc_dota_hero_base"
                };
                if (ignoreKeys.Contains(heroData.Key))
                {
                    continue;
                }
                heroInfos.Add(FromJson(heroData.Key, (JObject)heroData.Value, langJson));
            }

            var itemsJson = kvLoader.LoadFile(itemsScriptsPath);

            foreach (var kv in (itemsJson["items_game"]["items"] as JObject))
            {
                var data = kv.Value as JObject;
                if (data.Value<string>("prefab") == "wearable" || data.Value<string>("prefab") == "default_item")
                {
                    var item = HeroItemInfo.FromJson(kv.Key, data);
                    if (item.HeroName == null || !heroInfos.Any(h => h.Name == item.HeroName))
                    { // this one not assigned to a hero, so skip
                        continue;
                    }
                    if (item.ModelPath == null)
                    { // skip anything missing a model path (this project only cares about models)
                        continue;
                    }
                    heroInfos.FirstOrDefault(h => h.Name == item.HeroName).Cosmetics.Add(item);
                }
            }

            return heroInfos;
        }
    }
}
