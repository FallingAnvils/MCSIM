using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cmpdl.Tools;
using cmpdl_wrapper;

namespace mcmpgen
{
    public class Spigoter
    {
        string tmpLocation;
        const string javaArgs = "-DIReallyKnowWhatIAmDoingISwear -d64 -server -XX:+AggressiveOpts -XX:+UseConcMarkSweepGC -XX:+UnlockExperimentalVMOptions -XX:+UseParNewGC -XX:+ExplicitGCInvokesConcurrent -XX:+UseFastAccessorMethods -XX:+OptimizeStringConcat -XX:+UseAdaptiveGCBoundary";
        string launchScriptName;
        string instancesPath;
        string nameOfTheInstance;
        ConsoleFileDownloader fd;

        string spigotjarname;

        /// <summary>
        /// Use this one for updating
        /// </summary>
        /// <param name="launchScriptName"></param>
        /// <param name="instanceName"></param>
        /// <param name="instancesPath"></param>
        /// <param name="fd"></param>
        public Spigoter(string launchScriptName, string instanceName, string instancesPath, ConsoleFileDownloader fd)
        {
            this.launchScriptName = launchScriptName;
            this.nameOfTheInstance = instanceName;
            this.instancesPath = instancesPath;
            this.fd = fd;
        }
        /// <summary>
        /// Use this one for installing
        /// </summary>
        /// <param name="tmpLocation"></param>
        /// <param name="tempFolderName"></param>
        /// <param name="launchScriptName"></param>
        /// <param name="instanceName"></param>
        /// <param name="instancesPath"></param>
        /// <param name="fd"></param>
        public Spigoter(
            string tmpLocation,
            string tempFolderName,
            string launchScriptName,
            string instanceName,
            string instancesPath,
            ConsoleFileDownloader fd)
        {
            this.tmpLocation = tmpLocation;
            this.launchScriptName = launchScriptName;
            this.instancesPath = instancesPath;
            nameOfTheInstance = instanceName;
            this.fd = fd;
        }

        public void UpdateSpigot(string jarDownloadLink)
        {
            var instancepath = instancesPath + "/" + nameOfTheInstance;
            var filesInModpackDirectory = Directory.GetFiles(instancepath).Where(x => x.Contains(".jar"));
            if(filesInModpackDirectory.Count() == 1)
            {
                var oldjarfile = filesInModpackDirectory.First();
                File.Delete(oldjarfile);
                fd.DownloadWithName("", jarDownloadLink, instancepath, jarDownloadLink.AsUri().LastElement());
                MCTools.RegenScripts(instancepath, launchScriptName, new FileInfo(oldjarfile).Name, jarDownloadLink.AsUri().LastElement());
            }
            else
            {
                Console.WriteLine("There's not a single jar in the folder??? what???");
            }
        }

        public void SpigotBuildTools(
            string buildToolsDownloadLink,
            string version,
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

            var buildtoolsfolder = Path.Combine(tmpLocation, "bt_TEMP");
            spigotjarname = $"spigot-{version}.jar";
            Directory.CreateDirectory(buildtoolsfolder);

            fd.DownloadWithName("", buildToolsDownloadLink, buildtoolsfolder, "buildtools.jar");
            Process p = new Process();
            p.StartInfo = new ProcessStartInfo()
            {
                UseShellExecute = false,
                WorkingDirectory = buildtoolsfolder,
                FileName = "java",
                Arguments = $"-jar buildtools.jar --rev {version}"
            };
            p.Start();
            p.WaitForExit();

            File.Move(Path.Combine(buildtoolsfolder, spigotjarname), Path.Combine(tmpLocation, spigotjarname));
            Directory.Delete(buildtoolsfolder, true);

            MCTools.GenScripts(tmpLocation, launchScriptName, spigotjarname, ramMB, javaArgs);
            MCTools.AgreeToEula(tmpLocation);
            MCTools.WriteWhitelistsAndOps(tmpLocation, ops, whitelisted);
            MCTools.WriteServerDotProperties(tmpLocation,
                port,
                level_name,
                online_mode ? "true" : "false",
                command_block ? "true" : "false",
                difficulty,
                white_list ? "true" : "false",
                level_type);
            MCTools.MoveDirectory(tmpLocation, Path.Combine(instancesPath, nameOfTheInstance), new string[] { });
        }

        public void UpdateSpigotBuildTools(string buildToolsDownloadLink, string version)
        {
            var instancepath = instancesPath + "/" + nameOfTheInstance; // Path.Combine returns the second one if it's "valid", and in this case an instance name might be like
                                                                  // "/java/vanilla/private" and since it starts with a / it's "valid" (even though it doesn't exist)
                                                                  // BUT we add a slash just in case
            Console.WriteLine("######################");
            Console.WriteLine("InstancePath: " + instancepath);
            Console.WriteLine("InstancesPath: " + instancesPath);
            Console.WriteLine("NameOfTheInstance: " + nameOfTheInstance);
            Console.WriteLine("######################");
            var filesInModpackDirectory = Directory.GetFiles(instancepath).Where(x => x.Contains(".jar"));
            string oldjarfile;
            if (filesInModpackDirectory.Count() == 1)
            {
                oldjarfile = filesInModpackDirectory.First();
                File.Delete(oldjarfile);
            }
            else
            {
                // *Creates 4 new objects per iteration* EFFICIENCY!
                oldjarfile = filesInModpackDirectory.Where(x => (
                    new FileInfo(x).Name.Contains("spigot") || 
                    new FileInfo(x).Name.Contains("bukkit") || 
                    new FileInfo(x).Name.Contains("server") || 
                    new FileInfo(x).Name.Contains("1.")
                )).FirstOrDefault();
            }

            Directory.CreateDirectory(tmpLocation);

            var buildtoolsfolder = Path.Combine(tmpLocation, "bt_TEMP");
            spigotjarname = $"spigot-{version}.jar";
            Directory.CreateDirectory(buildtoolsfolder);

            fd.DownloadWithName("", buildToolsDownloadLink, buildtoolsfolder, "buildtools.jar");
            Process p = new Process();
            p.StartInfo = new ProcessStartInfo()
            {
                UseShellExecute = false,
                WorkingDirectory = buildtoolsfolder,
                FileName = "java",
                Arguments = $"-jar buildtools.jar --rev {version}"
            };
            p.Start();
            p.WaitForExit();

            File.Move(Path.Combine(buildtoolsfolder, spigotjarname), Path.Combine(instancepath, spigotjarname));


            MCTools.RegenScripts(instancepath, launchScriptName, new FileInfo(oldjarfile).Name, spigotjarname);

            Directory.Delete(tmpLocation, true);

        }

        public void SpigotDirectDownload(
            string jarDownloadLink,
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
            fd.DownloadWithName("", jarDownloadLink, tmpLocation, spigotjarname = jarDownloadLink.AsUri().LastElement());
            MCTools.GenScripts(tmpLocation, launchScriptName, spigotjarname, ramMB, javaArgs);
            MCTools.AgreeToEula(tmpLocation);
            MCTools.WriteWhitelistsAndOps(tmpLocation, ops, whitelisted);
            MCTools.WriteServerDotProperties(tmpLocation,
                port,
                level_name,
                online_mode ? "true" : "false",
                command_block ? "true" : "false",
                difficulty,
                white_list ? "true" : "false",
                level_type);
            MCTools.MoveDirectory(tmpLocation, Path.Combine(instancesPath, nameOfTheInstance), new string[] { });
        }
    }
}
