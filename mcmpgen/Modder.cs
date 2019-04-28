using Cmpdl.Tools;
using cmpdl_wrapper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mcmpgen
{
    public class Modder
    {
        string tmpLocation;
        const string javaArgs = "-d64 -server -XX:+AggressiveOpts -XX:+UseConcMarkSweepGC -XX:+UnlockExperimentalVMOptions -XX:+UseParNewGC -XX:+ExplicitGCInvokesConcurrent -XX:+UseFastAccessorMethods -XX:+OptimizeStringConcat -XX:+UseAdaptiveGCBoundary";
        string modpackTempFolderLocation;
        string modpackTempZipName;
        string modpackTempZipLocation;
        string launchScriptName;
        string packsPath;
        string nameOfTheInstance;
        ConsoleFileDownloader fd;

        string forgejarname;
        string forgeInstaller;
        string filesInModpackDirectory;
        string serverPackOriginalDownloadLink;
        string serverPackDownloadLink;

        string updateExcludePath;

        public Modder(
            string tmpLocation,
            string tempZipName,
            string launchScriptName,
            string instanceName,
            string packsPath,
            ConsoleFileDownloader fd)
        {
            this.tmpLocation = tmpLocation;
            modpackTempFolderLocation = Path.Combine(tmpLocation, "modpack");
            modpackTempZipName = tempZipName;
            modpackTempZipLocation = Path.Combine(tmpLocation, tempZipName);
            this.launchScriptName = launchScriptName;
            this.packsPath = packsPath;
            nameOfTheInstance = instanceName;
            this.fd = fd;
        }

        private void DownloadModpackZip(string zipLink)
        {
            serverPackDownloadLink = CmpdlUrlGenerator.FileName(zipLink);
            if (Directory.Exists(tmpLocation)) Directory.Delete(tmpLocation, true);
            Directory.CreateDirectory(tmpLocation);
            fd.DownloadWithName("", serverPackDownloadLink, tmpLocation, modpackTempZipName);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>location of the files we want</returns>
        private string ExtractModpackZip()
        {
            ZipFile.ExtractToDirectory(modpackTempZipLocation, modpackTempFolderLocation);
            if (Directory.GetFiles(modpackTempFolderLocation).Length == 0 && Directory.GetDirectories(modpackTempFolderLocation).Length == 1)
            {
                return Directory.GetDirectories(modpackTempFolderLocation).First();
            }
            return modpackTempFolderLocation;
        }

        private void DownloadAndExtractZip(string zipLink)
        {
            DownloadModpackZip(zipLink);
            modpackTempFolderLocation = ExtractModpackZip();
        }
        

        public void Mod(
            string zipDownloadLink, 
            int ramMB, 
            MCOp[] ops, 
            MCWhitelisted[] whitelisted,
            int port,
            string level_name,
            bool online_mode,
            bool command_block,
            int difficulty,
            bool white_list,
            string level_type
            )
        {
            Directory.CreateDirectory(tmpLocation);
            DownloadAndExtractZip(zipDownloadLink);
            ForgeDetectionAndInstallation();
            MCTools.GenScripts(modpackTempFolderLocation, launchScriptName, forgejarname, ramMB, javaArgs);
            MCTools.AgreeToEula(modpackTempFolderLocation);
            MCTools.WriteWhitelistsAndOps(modpackTempFolderLocation, ops, whitelisted);
            MCTools.WriteServerDotProperties(modpackTempFolderLocation, 
                port, 
                level_name,
                online_mode ? "true" : "false",
                command_block ? "true" : "false", 
                difficulty,
                white_list ? "true" : "false",
                level_type);
            MCTools.MoveAndDeleteDirectory(modpackTempFolderLocation, Path.Combine(packsPath, nameOfTheInstance), tmpLocation);
        }

        public void UpdateMod(string zipDownloadLink, string[] exclude)
        {
            var instancePath = Path.Combine(packsPath, nameOfTheInstance);
            if(!Directory.Exists(instancePath))
            {
                throw new ArgumentException("Update called; modpack is not installed yet.");
            }
            List<string> excluded = new List<string>(exclude);
            excluded.Add(launchScriptName + ".sh");
            excluded.Add(launchScriptName + ".bat");
            var oldjar = File.ReadAllText(Path.Combine(instancePath, "launch.bat")).Split(' ').Where(x => x.EndsWith(".jar")).FirstOrDefault();
            if (oldjar.Contains("Thermos")) excluded.Add(oldjar);
            MCTools.DeleteDirectory(instancePath, excluded.ToArray());

            Directory.CreateDirectory(tmpLocation);
            DownloadAndExtractZip(zipDownloadLink);
            ForgeDetectionAndInstallation();
            var mods = Directory.GetFiles(Path.Combine(modpackTempFolderLocation, "mods"));
            if (string.IsNullOrEmpty(mods.Where(x => x.EndsWith("aaasponge.jar")).FirstOrDefault()))
            {
                var foam = mods.Where(x => x.ToLower().Contains("foamfix")).FirstOrDefault();
                if (!string.IsNullOrEmpty(foam)) File.Delete(foam);
            }

            MCTools.MoveDirectory(modpackTempFolderLocation, instancePath, exclude);
            if(!oldjar.Contains("Thermos")) MCTools.RegenScripts(instancePath, launchScriptName, new FileInfo(oldjar).Name, forgejarname);
            Directory.Delete(tmpLocation, true);
        }

        private void ForgeDetectionAndInstallation()
        {
            string[] filesInModpackDirectory = Directory.GetFiles(modpackTempFolderLocation);
            if (!string.IsNullOrEmpty(filesInModpackDirectory.Where(f => f.Contains("universal.jar") || (f.Contains("forge")&&f.Contains(".jar")&&!f.Contains("installer"))).FirstOrDefault()))
            {
                forgejarname = new FileInfo(filesInModpackDirectory.Where(f => f.Contains("universal.jar") || (f.Contains("forge") && f.Contains(".jar")&& !f.Contains("installer"))).FirstOrDefault()).Name;
                if(forgejarname.Contains("FTBserver")) // ftb custom junk
                {
                    string forgeversion = forgejarname.Replace("-universal.jar", "").Replace("FTBserver-", "");
                    forgeInstaller = forgejarname.Replace("-universal.jar", "-installer.jar").Replace("FTBserver-", "forge-");
                    string forgedlurl = $"https://files.minecraftforge.net/maven/net/minecraftforge/forge/{forgeversion}/{forgeInstaller}";
                    fd.DownloadWithName("", forgedlurl, modpackTempFolderLocation, forgeInstaller);
                    ForgeInstallProcess(modpackTempFolderLocation, forgeInstaller);
                    forgejarname = forgeInstaller.Replace("installer", "universal");
                }
            }
            else if(!string.IsNullOrEmpty((forgeInstaller = filesInModpackDirectory.Where(f => f.Contains("installer.jar")).FirstOrDefault())))
            {
                Console.WriteLine("Forge installer");
                forgeInstaller = new FileInfo(forgeInstaller).Name;
                ForgeInstallProcess(modpackTempFolderLocation, forgeInstaller);
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
                // another special FTB case, this one is mainly for infinity evolved
                else if(!string.IsNullOrEmpty(forgejarname = new FileInfo(filesInModpackDirectory.Where(x => x.EndsWith("FTBserver.jar")).FirstOrDefault()).Name))
                {
                    // just to make sure
                    if(!string.IsNullOrEmpty(filesInModpackDirectory.Where(x => x.Contains("FTBInstall")).FirstOrDefault()))
                    {
                        string script = "FTBInstall" + (Program.IsLinux ? ".sh" : ".bat");
                        Process p = new Process();
                        if(Program.IsLinux)
                        {
                            p.StartInfo = new ProcessStartInfo()
                            {
                                UseShellExecute = false,
                                FileName = "/bin/bash",
                                WorkingDirectory = modpackTempFolderLocation,
                                Arguments = new FileInfo(script).Name
                            };
                        }
                        else
                        {
                            p.StartInfo = new ProcessStartInfo()
                            {
                                UseShellExecute = false,
                                FileName = script,
                                WorkingDirectory = modpackTempFolderLocation
                            };
                        }
                        p.Start();
                        p.WaitForExit();
                    }
                }
                else
                {
                    var bats = fileInfosInModpackDir
                        .Where(f => f.Extension.Equals(Program.IsLinux ? ".sh" : ".bat"))
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
                        if (Program.IsLinux)
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
                Console.WriteLine("Uhhh");
                Console.WriteLine();
            }
            if (string.IsNullOrEmpty(forgejarname)) forgejarname = new FileInfo(forgeInstaller.Replace("installer", "universal")).Name;
        }

        void ForgeInstallProcess(string workingDir, string fileName)
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
        }
    }
}
