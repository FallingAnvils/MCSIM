using cmpdl_wrapper;
using mcmpgen;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.VisualBasic.FileIO;

namespace bungeecordhandler
{
    public class ForgeSponger
    {
        static Dictionary<string, string> dllinks = new Dictionary<string, string>()
            {
                { "1.12.2", "https://repo.spongepowered.org/maven/org/spongepowered/spongeforge/1.12.2-2768-7.1.5/spongeforge-1.12.2-2768-7.1.5.jar" },
                { "1.11.2", "https://repo.spongepowered.org/maven/org/spongepowered/spongeforge/1.11.2-2476-6.1.0-BETA-2792/spongeforge-1.11.2-2476-6.1.0-BETA-2792.jar" },
                { "1.10.2", "https://repo.spongepowered.org/maven/org/spongepowered/spongeforge/1.10.2-2477-5.2.0-BETA-2793/spongeforge-1.10.2-2477-5.2.0-BETA-2793.jar" },
                { "1.7.10", "https://yivesmirror.com/files/thermos/Thermos-1.7.10-1614-57.zip" } // special zip file
            };

        string instancePath;
        string bungeePath;

        public ForgeSponger(string instancePath, string bungeePath)
        {
            this.instancePath = instancePath;
            this.bungeePath = bungeePath;
        }

        public void Sponge(string instanceName, int port, string mcversion)
        {
            string modsFolder = Path.Combine(instancePath, "mods");
            var bungeeConfigFile = Path.Combine(bungeePath, "config.yml");
            var serverDotProperties = Path.Combine(instancePath, "server.properties");
            ConsoleFileDownloader dl = new ConsoleFileDownloader();

            // sponge
            if (mcversion.Equals("1.12.2") || mcversion.Equals("1.11.2") || mcversion.Equals("1.10.2"))
            {
                Directory.CreateDirectory(Path.Combine(instancePath, "config", "sponge"));
                var spongeConfigFile = Path.Combine(instancePath, "config", "sponge", "global.conf");

                Console.WriteLine($"Sponge config file: {spongeConfigFile}");
                Console.WriteLine($"Bungee config file: {bungeeConfigFile}");
                Console.WriteLine($"Server.properties: {serverDotProperties}");

                // download and install spongeforge
                // aaa so it loads first I guess, trust me you need it
                dl.DownloadWithName("", dllinks[mcversion], modsFolder, "aaasponge.jar");

                // not compatible with foamfix, remove if there
                var foam = Directory.GetFiles(modsFolder).Where(x => x.ToLower().Contains("foamfix")).FirstOrDefault();
                if (!string.IsNullOrEmpty(foam)) File.Delete(foam);

                // write base config
                File.WriteAllText(spongeConfigFile,
    @"sponge {
    bungeecord {
        ip-forwarding=true
    }
    modules {
        bungeecord=true
    }
}
");
            }
            // thermos (aka cauldron aka "spigotforge")
            else if (mcversion.Equals("1.7.10"))
            {
                // download thermos
                dl.DownloadWithName("", dllinks["1.7.10"], instancePath, "thermoszip.zip");

                // extract it
                var ziploc = Path.Combine(instancePath, "thermoszip.zip");
                var tmploc = Path.Combine(instancePath, "thermostmp");
                var libs = Path.Combine(tmploc, "libraries");
                ZipFile.ExtractToDirectory(ziploc, tmploc);
                FileSystem.MoveDirectory(libs, Path.Combine(instancePath, "libraries"), true);
                File.Move(Directory.GetFiles(tmploc).FirstOrDefault(), Path.Combine(instancePath, new FileInfo(Directory.GetFiles(tmploc).FirstOrDefault()).Name));
                var oldjar = File.ReadAllText(Path.Combine(instancePath, "launch.bat")).Split(' ').Where(x => x.EndsWith(".jar")).FirstOrDefault();
                MCTools.RegenScripts(instancePath, "launch", oldjar, new FileInfo(Directory.GetFiles(instancePath).Where(x => x.Contains("Thermos")).FirstOrDefault()).Name);
                File.Delete(ziploc);
                Directory.Delete(tmploc, true);
                File.WriteAllText(Path.Combine(instancePath, "spigot.yml"), @"
settings:
  bungeecord: true
");
            }



            // update bungeecord config
            var lines = new List<string>(File.ReadAllLines(bungeeConfigFile));

            int serversIndex = lines.IndexOf("servers:") + 1;
            lines.Insert(serversIndex, "    restricted: false");
            lines.Insert(serversIndex, $"    address: localhost:{port}");
            lines.Insert(serversIndex, "    motd: 'motd'");
            lines.Insert(serversIndex, $"  {instanceName}:");

            File.WriteAllLines(bungeeConfigFile, lines);
        }
    }
}
