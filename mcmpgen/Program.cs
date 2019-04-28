using System;
using System.Linq;
using Newtonsoft.Json;
using cmpdl_wrapper;

namespace mcmpgen
{
    class Program
    {
        public static bool IsLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

        const string tmpLocation = "./cspdltmp";
        const string modpackTempZipName = "modpack.zip";
        const string modpackTempZipLocation = tmpLocation + "/" + modpackTempZipName;
        const string launchScriptName = "launch";
        const string javaArgs = "-d64 -server -XX:+AggressiveOpts -XX:+UseConcMarkSweepGC -XX:+UnlockExperimentalVMOptions -XX:+UseParNewGC -XX:+ExplicitGCInvokesConcurrent -XX:+UseFastAccessorMethods -XX:+OptimizeStringConcat -XX:+UseAdaptiveGCBoundary";


        static ArgsObject ParseArgs(string[] args)
        {
            return JsonConvert.DeserializeObject<ArgsObject>(args[0]);
        }


        static void Main(string[] args)
        {
            if(args.Length < 1 || args.Length > 1)
            {
                Console.WriteLine("Usage: mcmpgen.exe \"<escaped json here>\"");
                return;
            }
            ArgsObject aobj = ParseArgs(args);


            switch (aobj.ServerType)
            {
                case ServerType.Modded:
                    switch(aobj.InstallType)
                    {
                        case InstallType.Update:
                            ModpackUpdateProcess(aobj.InstancesPath, aobj.InstanceName, aobj.ModpackUrl, aobj.ExcludedFiles);
                            break;
                        default:
                            ModpackInstallProcess(
                                aobj.InstancesPath,
                                aobj.InstanceName,
                                aobj.ModpackUrl,
                                aobj.RamMB,
                                aobj.Ops,
                                aobj.Whitelisted,
                                aobj.Port,
                                aobj.LevelName,
                                aobj.OnlineMode,
                                aobj.CommandBlocksEnabled,
                                aobj.Difficulty,
                                aobj.WhiteList,
                                aobj.LevelType);
                            break;
                    }
                    break;
                case ServerType.Bukkit:
                    switch(aobj.InstallType)
                    {
                        case InstallType.Update:
                            switch(aobj.SpigotType)
                            {
                                case SpigotType.BuildTools:
                                    SpigotBuildToolsUpdateProcess(aobj.InstanceName, aobj.InstancesPath, "https://hub.spigotmc.org/jenkins/job/BuildTools/lastSuccessfulBuild/artifact/target/BuildTools.jar", aobj.Version);
                                    break;
                                case SpigotType.DirectDownload:
                                    SpigotDirectDownloadUpdateProcess(aobj.InstanceName, aobj.InstancesPath, aobj.BukkitUrl);
                                    break;
                            }
                            break;
                        default:
                            switch(aobj.SpigotType)
                            {
                                case SpigotType.BuildTools:
                                    SpigotBuildToolsInstallProcess(
                                        aobj.InstancesPath,
                                        aobj.InstanceName,
                                        "https://hub.spigotmc.org/jenkins/job/BuildTools/lastSuccessfulBuild/artifact/target/BuildTools.jar",
                                        aobj.Version,
                                        aobj.RamMB,
                                        aobj.Ops,
                                        aobj.Whitelisted,
                                        aobj.Port,
                                        aobj.LevelName,
                                        aobj.OnlineMode,
                                        aobj.CommandBlocksEnabled,
                                        aobj.Difficulty,
                                        aobj.WhiteList,
                                        aobj.LevelType);
                                    break;
                                case SpigotType.DirectDownload:
                                    SpigotDirectDownloadInstallProcess(
                                        aobj.InstancesPath,
                                        aobj.InstanceName,
                                        aobj.BukkitUrl,
                                        aobj.RamMB,
                                        aobj.Ops,
                                        aobj.Whitelisted,
                                        aobj.Port,
                                        aobj.LevelName,
                                        aobj.OnlineMode,
                                        aobj.CommandBlocksEnabled,
                                        aobj.Difficulty,
                                        aobj.WhiteList,
                                        aobj.LevelType);
                                    break;
                            }
                            break;
                    }
                    break;
                case ServerType.Bedrock:
                    switch(aobj.InstallType)
                    {
                        case InstallType.Update:
                            BedrockUpdateProcess(aobj.InstancesPath, aobj.InstanceName, aobj.BedrockUrl);
                            break;
                        default:
                            BedrockInstallProcess(
                                aobj.InstancesPath,
                                aobj.InstanceName,
                                aobj.BedrockUrl,
                                aobj.Whitelisted,
                                aobj.Port,
                                aobj.LevelName,
                                aobj.OnlineMode,
                                aobj.CommandBlocksEnabled,
                                aobj.Difficulty,
                                aobj.WhiteList);
                            break;
                    }
                    break;
            }



            

        }
        
        static void BedrockUpdateProcess(string instancesPath, string instanceName, string zipDownloadLink/*, string[] excluded*/)
        {
            Bedrocker bedrocker = new Bedrocker(tmpLocation, "bedrock.zip", launchScriptName, instanceName, instancesPath, new ConsoleFileDownloader());
            bedrocker.UpdateBedrock(zipDownloadLink, new[] {
                "permissions.json",
                "server.properties",
                "whitelist.json",
                "worlds"
            });
        }

        static void BedrockInstallProcess(
            string instancesPath,
            string instanceName,
            string zipDownloadLink,
            string[] whitelisted,
            int port,
            string level_name,
            bool online_mode,
            bool command_block,
            int difficulty,
            bool white_list
            )
        {
            Bedrocker bedrocker = new Bedrocker(tmpLocation, "bedrock.zip", launchScriptName, instanceName, instancesPath, new ConsoleFileDownloader());
            bedrocker.Bedrock(
                zipDownloadLink, 
                whitelisted.Select(x => new BedrockWhitelisted(x)).ToArray(), 
                port, 
                level_name, 
                online_mode, 
                command_block, 
                difficulty, 
                white_list
            );
        }

        static void SpigotDirectDownloadUpdateProcess(string instanceName, string instancesPath, string jarDownloadLink)
        {
            Spigoter spigoter = new Spigoter(launchScriptName, instanceName, instancesPath, new ConsoleFileDownloader());
            spigoter.UpdateSpigot(jarDownloadLink);
        }

        static void SpigotBuildToolsUpdateProcess(string instanceName, string instancesPath, string buildToolsDownloadLink, string version)
        {
            Spigoter spigoter = new Spigoter(tmpLocation, "tmpinst", launchScriptName, instanceName, instancesPath, new ConsoleFileDownloader());
            spigoter.UpdateSpigotBuildTools(buildToolsDownloadLink, version);
        }

        static void SpigotDirectDownloadInstallProcess(
            string instancesPath, 
            string instanceName, 
            string jarDownloadLink, 
            int ramMB, 
            string[] ops, 
            string[] whitelisted,
            int port, 
            string level_name, 
            bool online_mode, 
            bool command_block, 
            int difficulty, 
            bool white_list,
            string level_type)
        {
            var tup = MCTools.GetOppedAndWhitelisted(ops, whitelisted);
            Spigoter spigoter = new Spigoter(
                tmpLocation,
                "tmpinst",
                launchScriptName,
                instanceName,
                instancesPath,
                new ConsoleFileDownloader()
            );

            spigoter.SpigotDirectDownload(
                jarDownloadLink, 
                ramMB, 
                tup.Item1, 
                tup.Item2, 
                port, 
                level_name,
                online_mode, 
                command_block, 
                difficulty, 
                white_list,
                level_type
            );

        }

        static void SpigotBuildToolsInstallProcess(
            string instancesPath,
            string instanceName,
            string buildToolsDownloadLink,
            string version,
            int ramMB,
            string[] ops,
            string[] whitelisted,
            int port,
            string level_name,
            bool online_mode,
            bool command_block,
            int difficulty,
            bool white_list,
            string level_type)
        {
            var tup = MCTools.GetOppedAndWhitelisted(ops, whitelisted);
            Spigoter spigoter = new Spigoter(
                tmpLocation,
                "tmpinst",
                launchScriptName,
                instanceName,
                instancesPath,
                new ConsoleFileDownloader()
            );

            spigoter.SpigotBuildTools(
                buildToolsDownloadLink,
                version,
                ramMB,
                tup.Item1,
                tup.Item2,
                port,
                level_name,
                online_mode,
                command_block,
                difficulty,
                white_list,
                level_type
            );
        }


        static void ModpackUpdateProcess(string instancesPath, string instanceName, string zipDownloadLink, string[] exclude)
        {
            Modder modder = new Modder(
                tmpLocation, 
                "modpack.zip", 
                launchScriptName, 
                instanceName, 
                instancesPath, 
                new ConsoleFileDownloader()
            );

            modder.UpdateMod(zipDownloadLink, exclude);
        }

        static void ModpackInstallProcess(
            string instancesPath, 
            string instanceName, 
            string zipDownloadLink, 
            int ramMB, 
            string[] ops, 
            string[] whitelisted, 
            int port, 
            string level_name, 
            bool online_mode,
            bool command_block,
            int difficulty,
            bool white_list,
            string level_type)
        {
            var tup = MCTools.GetOppedAndWhitelisted(ops, whitelisted);

            Modder modder = new Modder(
                tmpLocation,
                "modpack.zip",
                launchScriptName,
                instanceName,
                instancesPath,
                new ConsoleFileDownloader()
            );

            modder.Mod(
                zipDownloadLink, 
                ramMB, 
                tup.Item1, 
                tup.Item2, 
                port, 
                level_name, 
                online_mode, 
                command_block, 
                difficulty, 
                white_list,
                level_type);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filesInModpackDirectory"></param>
        /// <returns>forge jar name</returns>
        /*static string ForgeInstallerProcess(string[] filesInModpackDirectory, ConsoleFileDownloader fd)
        {
            string forgeInstaller = "";
            string forgejarname;
            Console.WriteLine(filesInModpackDirectory.Where(f => f.Contains("universal.jar")).FirstOrDefault());
            if (!string.IsNullOrEmpty((forgejarname = filesInModpackDirectory.Where(f => f.Contains("universal.jar")).FirstOrDefault())))
            {
                Console.WriteLine("Forge is installed, continuing");
            }
            else if (string.IsNullOrEmpty((forgeInstaller = filesInModpackDirectory.Where(f => f.Contains("installer.jar")).FirstOrDefault())))
            {
                Console.WriteLine("No forge installer");
                var fileInfosInModpackDir = filesInModpackDirectory.Select(f => new FileInfo(f));
                var settingsCfg = fileInfosInModpackDir.Where(f => f.Name.Equals("settings.cfg"));
                if (settingsCfg.Count() > 0) // ATM-style settings for installer
                {
                    string[] settingscfglines = File.ReadAllLines(settingsCfg.First().FullName);
                    string forgeurl = settingscfglines.Where(l => l.StartsWith("FORGEURL=")).FirstOrDefault();
                    forgeurl = forgeurl.Substring(0, forgeurl.Length - 1);
                    forgeurl = forgeurl.Substring("FORGEURL=".Length, forgeurl.Length - "FORGEURL=".Length);
                    forgeInstaller = forgeurl.AsUri().LastElement();
                    fd.DownloadWithName("", forgeurl, modpackTempFolderLocation, forgeInstaller);
                    ForgeInstallProcess(modpackTempFolderLocation, forgeInstaller);
                }
                else
                {
                    var bats = fileInfosInModpackDir
                        .Where(f => f.Extension.Equals(IsLinux ? ".sh" : ".bat"))
                        .ToList();

                    if (bats.Count == 0)     // 0 bats
                    {

                    }
                    else if (bats.Count > 1) // > 1 bats
                    {
                        Console.WriteLine("Multiple script files found:");
                        for (int i = 0; i < bats.Count; i++)
                        {
                            Console.WriteLine((i + 1) + ". " + bats[i].Name);
                        }
                        int result; bool success = false;
                        do
                        {
                            Console.Write("Which one?:");
                            success = int.TryParse(Console.ReadLine(), out result) && result <= bats.Count;
                        } while (!success);
                        FileInfo choice = bats[result - 1];
                        Console.WriteLine("You chose " + choice.Name);
                    }
                    else                    // 1 bat
                    {
                        var bat = bats.First();
                        Console.WriteLine("Using " + bat.Name);
                        Process p = new Process();
                        if (IsLinux)
                        {
                            p.StartInfo = new ProcessStartInfo()
                            {
                                UseShellExecute = false,
                                FileName = "/bin/bash",
                                WorkingDirectory = modpackTempFolderLocation,
                                Arguments = bat.Name
                            };
                        }
                        else
                        {
                            p.StartInfo = new ProcessStartInfo()
                            {
                                UseShellExecute = false,
                                FileName = bat.FullName,
                                WorkingDirectory = modpackTempFolderLocation
                            };
                        }

                        p.Start();
                        p.WaitForExit();
                        Console.WriteLine("installed");

                    }
                }
            }
            else
            {
                Console.WriteLine("Forge installer, launching");
                ForgeInstallProcess(modpackTempFolderLocation, new FileInfo(forgeInstaller).Name);
                Console.WriteLine("Forge installed");
            }

            if (string.IsNullOrEmpty(forgejarname)) forgejarname = forgeInstaller.Replace("installer", "universal");

            return new FileInfo(forgejarname).Name;
        }*/
        /*static void GetServerDotProperties(string folder)
        {
            int port = PromptInt("Server port?: ");
            string level_name = PromptDefault("Level name? (world): ", "world");
            string level_type = PromptDefault("Level type? (DEFAULT): ", "DEFAULT");
            string online_mode = PromptDefault("Online mode? (true): ", "true");
            string command_block = PromptDefault("Enable command blocks? (false): ", "false");
            int difficulty = PromptDefaultInt("Difficulty?: ", 1);
            string white_list = PromptDefault("Whitelist? (true): ", "true");

            string[] serverdotproperties = {
                "server-port=" + port,
                "level-name=" + level_name,
                "level-type=" + level_type,
                "online-modoe=" + online_mode,
                "enable-command-block=" + command_block,
                "difficulty=" + difficulty,
                "white-list=" + white_list
            };

            File.WriteAllLines(folder + "/server.properties", serverdotproperties);
        }*/

        /*static void GetWhitelistsAndOps(string folder)
        {
            Console.WriteLine("Type operators, then type \"!done\"");
            List<MCOp> ops = new List<MCOp>();

            string val;
            while ((val = Console.ReadLine()) != "!done")
            {
                MCOp op = new MCOp();
                op.name = val;
                op.uuid = JsonConvert.DeserializeObject<MCUser>(UrlGet("https://api.mojang.com/users/profiles/minecraft/" + val)).id;
                ops.Add(op);
            }

            MCOp[] opsarr = ops.ToArray();

            File.WriteAllText(folder + "/ops.json", JsonConvert.SerializeObject(opsarr));


            Console.WriteLine("Type additional whitelisted players, then type \"!done\". Operators are whitelisted automatically.");
            List<MCWhitelisted> whitelisteds = new List<MCWhitelisted>();
            ops.ForEach(x => whitelisteds.Add(new MCWhitelisted(x.name, x.uuid)));

            string valw;
            while ((valw = Console.ReadLine()) != "!done")
            {
                MCWhitelisted whitelisted = new MCWhitelisted();
                whitelisted.name = valw;
                whitelisted.uuid = JsonConvert.DeserializeObject<MCUser>(UrlGet("https://api.mojang.com/users/profiles/minecraft/" + valw)).id;
                whitelisteds.Add(whitelisted);
            }

            MCWhitelisted[] whitelistedsarr = whitelisteds.ToArray();

            File.WriteAllText(folder + "/whitelist.json", JsonConvert.SerializeObject(whitelistedsarr));

        }*/

        /*static void ForgeInstallProcess(string workingDir, string fileName)
        {
            Process p = new Process();
            p.StartInfo = new ProcessStartInfo()
            {
                UseShellExecute = false,
                WorkingDirectory = workingDir,
                FileName = "java",
                Arguments = $"-jar \"{fileName}\" --installServer"
            };
            p.Start();
            p.WaitForExit();
        }*/

        /*static int GetMaxRamUserInput()
        {
            bool success = false; int maxRam;
            do
            {
                Console.Write("How much ram? (in MB): ");
                success = int.TryParse(Console.ReadLine(), out maxRam);
            } while (!success);
            return maxRam;
        }*/

    }
}
