using EffigyMaker.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ValveResourceFormat;
using ValveResourceFormat.ResourceTypes;

namespace EffigyMaker.CLI
{
    class Program
    {
        public static async Task<int> Main(string[] args)
        {
            Console.WriteLine("Exporting!");
            var vpkPath = @"C:\Program Files (x86)\Steam\steamapps\common\dota 2 beta\game\dota\pak01_dir.vpk";

            var vpkLoader = BasicVpkFileLoader.FromVpk(vpkPath);

            var modelPaths = new List<string>();

            var autoGetHeroModels = false;
            var useDefaultCostmetics = false;
            var useOpendota = false;

            if (autoGetHeroModels)
            {
                //var heroName = "Tinker";
                var heroName = "Bristleback";

                var heroInfos = HeroInfo.LoadFromVpk(vpkLoader);
                var heroInfo = heroInfos.FirstOrDefault(h => h.LocalizedName == heroName);
                modelPaths.Add(heroInfo.ModelPath);

                if (useDefaultCostmetics)
                {
                    modelPaths.AddRange(heroInfo.Cosmetics.Where(item => item.IsDefault && !item.ModelPath.Contains("persona")).Select(item => item.ModelPath));
                }
                if (useOpendota)
                {
                    var cosmeticIds = await DotaMatchCosmetics.GetCostmeticIds(95211699, heroInfo.Id);
                    modelPaths.AddRange(heroInfo.Cosmetics.Where(item => cosmeticIds.Contains(item.Id)).Select(item => item.ModelPath));
                }
                else
                {
                    var cosmeticIds = new int[]
                    {
                        8391,
                        9150,
                        9787,
                        9789,
                        9790,
                    };
                    modelPaths.AddRange(heroInfo.Cosmetics.Where(item => cosmeticIds.Contains(item.Id)).Select(item => item.ModelPath));
                }
            }
            else
            {
                modelPaths = new List<string>
                {
                    "models/heroes/bristleback/bristleback.vmdl",
                    "models/heroes/bristleback/bristleback_necklace.vmdl",
                    //"models/heroes/bristleback/bristleback_head.vmdl",
                    //"models/heroes/bristleback/bristleback_back.vmdl",
                    //"models/heroes/bristleback/bristleback_weapon.vmdl",
                    //"models/heroes/bristleback/bristleback_bracer.vmdl",
                };
            }

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

            using Process myProcess = new Process();
            myProcess.StartInfo.FileName = "explorer.exe";
            myProcess.StartInfo.Arguments = "out.obj";
            myProcess.Start();
            return 0;
        }
    }
}
