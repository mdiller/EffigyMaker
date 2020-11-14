using EffigyMaker.Core;
using System;

namespace EffigyMaker.CLI
{
    class Program
    {
        public static int Main(string[] args)
        {
            Console.WriteLine("Exporting!");
            var vpkPath = @"C:\Program Files (x86)\Steam\steamapps\common\dota 2 beta\game\dota\pak01_dir.vpk";

            var modelPath = "models/heroes/bristleback/bristleback.vmdl_c";
            var exporter = new ObjExporter(BasicVpkFileLoader.FromVpk(vpkPath));
            exporter.ExportModelAsObj(modelPath);
            return 0;
        }
    }
}
