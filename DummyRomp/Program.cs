using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace DummyRomp
{
    class Program
    {
        private static readonly UTF8Encoding UTF8 = new UTF8Encoding(false);

        static void Main(string[] args)
        {
            if (args.Length == 2 && args[0] == "pack" && args[1] == ".upack")
            {
                DummyPackCommand();
            }
            else
            {
                throw new NotImplementedException("This version of romp.exe is a minimal implementation for demonstrating the Visual Studio Romp plugin.");
            }
        }

        private static void DummyPackCommand()
        {
            var package = JsonConvert.DeserializeAnonymousType(File.ReadAllText(".upack/upack.json", UTF8), new { name = "", version = "" });
            var archiveName = $"{package.name}-{package.version}.upack";
            using (var zip = new ZipArchive(File.OpenWrite(archiveName), ZipArchiveMode.Create, false))
            {
                string baseDir = Path.GetFullPath(".upack");
                foreach (var dir in Directory.EnumerateDirectories(baseDir, "*", SearchOption.AllDirectories))
                {
                    zip.CreateEntry(dir.Substring(baseDir.Length).Replace('\\', '/').Trim('/') + "/");
                }
                foreach (var name in Directory.EnumerateFiles(baseDir, "*", SearchOption.AllDirectories))
                {
                    var entry = zip.CreateEntry(name.Substring(baseDir.Length).Replace('\\', '/').Trim('/'));
                    entry.LastWriteTime = new DateTimeOffset(File.GetLastWriteTimeUtc(name), TimeSpan.Zero);
                    using (var stream = entry.Open())
                    using (var file = File.OpenRead(name))
                    {
                        file.CopyTo(stream);
                    }
                }
            }
            Console.WriteLine($"Successfully created Universal Package {archiveName} in {Directory.GetCurrentDirectory()}");
        }
    }
}
