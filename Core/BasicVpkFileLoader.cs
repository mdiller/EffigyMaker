using System;
using System.IO;
using SteamDatabase.ValvePak;
using ValveResourceFormat;
using ValveResourceFormat.IO;

namespace EffigyMaker.Core
{
    /// <summary>
    /// Note, this file taken directly from https://github.com/SteamDatabase/ValveResourceFormat, with some slight modifications
    /// </summary>
    public class BasicVpkFileLoader : IFileLoader
    {
        private readonly Package CurrentPackage;

        public BasicVpkFileLoader(Package package)
        {
            CurrentPackage = package ?? throw new ArgumentNullException(nameof(package));
        }

        public Resource LoadFile(string file)
        {
            file = FixPathSlashes(file);
            var entry = CurrentPackage.FindEntry(file);

            if (entry == null)
            {
                return null;
            }

            CurrentPackage.ReadEntry(entry, out var output, false);

            var resource = new Resource();
            resource.Read(new MemoryStream(output));

            return resource;
        }

        public static BasicVpkFileLoader FromVpk(string path)
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var package = new Package();
            package.SetFileName(path);

            package.Read(stream);

            return new BasicVpkFileLoader(package);
        }

        private static string FixPathSlashes(string path)
        {
            path = path.Replace('\\', '/');

            if (Path.DirectorySeparatorChar != '/')
            {
                path = path.Replace('/', Path.DirectorySeparatorChar);
            }

            return path;
        }
    }
}
