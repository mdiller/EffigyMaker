using EffigyMaker.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using ValveResourceFormat;
using ValveResourceFormat.ResourceTypes;

namespace EffigyMaker.CLI
{
    class Program
    {
        public static int Main(string[] args)
        {
            Console.WriteLine("Exporting!");
            var vpkPath = @"C:\Program Files (x86)\Steam\steamapps\common\dota 2 beta\game\dota\pak01_dir.vpk";

            //var heroName = "npc_dota_hero_void_spirit";
            var vpkLoader = BasicVpkFileLoader.FromVpk(vpkPath);

            var modelPaths = new List<string>();

            //var heroInfos = HeroInfo.LoadFromVpk(vpkLoader);
            //var heroInfo = heroInfos.FirstOrDefault(h => h.LocalizedName == heroName);
            //modelPaths.Add(heroInfo.ModelPath);
            //modelPaths.AddRange(heroInfo.Cosmetics.Where(item => item.IsDefault && !item.ModelPath.Contains("persona")).Select(item => item.ModelPath));

            modelPaths = new List<string>
            {
                "models/heroes/bristleback/bristleback.vmdl",
                "models/heroes/bristleback/bristleback_back.vmdl",
                "models/heroes/bristleback/bristleback_head.vmdl",
                "models/heroes/bristleback/bristleback_weapon.vmdl",
                "models/heroes/bristleback/bristleback_bracer.vmdl",
                "models/heroes/bristleback/bristleback_necklace.vmdl",
            };

            var exporter = new ObjExporter(vpkLoader);
            exporter.ExportModelsAsObj(modelPaths.Select(path =>
            {
                var resource = vpkLoader.LoadFile(path);

                if (resource.ResourceType != ResourceType.Model)
                {
                    throw new ArgumentException("Passed in path must be a path to a model");
                }

                return (Model)resource.DataBlock;
            }).ToList());
            return 0;
        }
    }
}
