using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mcmpgen
{
    public class MCTools
    {
        public static void AgreeToEula(string folder)
        {
            FileStream eula = File.Create(folder + "/eula.txt");    // we
            eula.Write(Encoding.ASCII.GetBytes("eula=true"), 0, 9); // gotta
            eula.Close();                                           // agree

        }
        public static void MoveAndDeleteDirectory(string source, string target, string delete)
        {
            Directory.Move(source, target);
            Directory.Delete(delete, true);
        }

        public static void DeleteDirectory(string dir, string[] exclude)
        {
            void processDirectory(string startLocation)
            {
                foreach (var directory in Directory.GetDirectories(startLocation))
                {
                    processDirectory(directory);
                    if (Directory.GetFiles(directory).Length == 0 &&
                        Directory.GetDirectories(directory).Length == 0)
                    {
                        Directory.Delete(directory, false);
                    }
                }
            }
            // do not touch this is magic that works I guess
            var d = Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories)
                .Select(x => x.Substring(dir.Length + 1).Replace('\\', '/')).ToList();
            List<string> toRemove = new List<string>();
            foreach(string s in exclude)
            {
                foreach(string sd in d)
                {
                    if (sd.StartsWith(s)) toRemove.Add(sd);
                }
            }
            toRemove.ForEach(x => d.Remove(x));

            d.ForEach(x => {
                File.Delete(dir + "/" + x);
            });
            List<string> toRemove1 = new List<string>();
            var d1 = Directory.EnumerateDirectories(dir, "*.*", SearchOption.AllDirectories)
                .Select(x => x.Substring(dir.Length + 1).Replace('\\', '/')).ToList();
            foreach (string s in exclude)
            {
                foreach (string sd in d1)
                {
                    if (sd.StartsWith(s)) toRemove1.Add(sd);
                }
            }
            toRemove1.ForEach(x => d1.Remove(x));
            d1.ForEach(x => {
                if(Directory.GetFiles(dir+"/"+x).Length < 1 && Directory.GetDirectories(dir+"/"+x).Length < 1) Directory.Delete(dir + "/" + x);
            });
            processDirectory(dir);
        }
        /// <summary>
        /// Format for exclude: assume it starts with "./". So to exclude "test.txt" in the root, exclude "test.txt".
        /// To exclude "test.txt" from a folder called "world" inside a folder called "hello" which sits in the root,
        /// exclude "hello/world/test.txt".
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="exclude"></param>
        public static void MoveDirectory(string source, string target, string[] exclude)
        {
            var stack = new Stack<Folders>();
            stack.Push(new Folders(source, target));

            while (stack.Count > 0)
            {
                var folders = stack.Pop();
                if (target != folders.Target && exclude.Contains(folders.Target.Substring(target.Length + 1).Replace('\\', '/'))) continue;
                Directory.CreateDirectory(folders.Target);
                foreach (var file in Directory.GetFiles(folders.Source, "*.*"))
                {
                    string targetFile = Path.Combine(folders.Target, Path.GetFileName(file));
                    var excludefile = targetFile.Substring(target.Length+1).Replace('\\','/');
                    if (exclude.Contains(excludefile)) continue;
                    if (File.Exists(targetFile))
                    {
                        File.Delete(targetFile);
                    }

                    File.Move(file, targetFile);
                }

                foreach (var folder in Directory.GetDirectories(folders.Source))
                {
                    stack.Push(new Folders(folder, Path.Combine(folders.Target, Path.GetFileName(folder))));
                }
            }
            Directory.Delete(source, true);
        }
        public static void ChmodPlusXScript(string folder, string filenameWithoutExtension)
        {
            ChmodPlusX(folder, filenameWithoutExtension + ".sh");
        }
        public static void ChmodPlusX(string folder, string filename)
        {
            if (Program.IsLinux)
            {
                Process p = new Process();
                p.StartInfo = new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    FileName = "/bin/chmod",
                    WorkingDirectory = folder,
                    Arguments = "+x " + filename,
                };
                p.Start();
                p.WaitForExit();
            }
        }
        public static void GenScripts(string folder, string scriptName, string jarName, int ramMB, string javaArgs)
        {
            Command mcStartCmd = new Command();
            mcStartCmd.FileName = "java";
            mcStartCmd.Args.Add(javaArgs);
            mcStartCmd.Args.Add("-Xmx" + ramMB + "M");
            mcStartCmd.Args.Add("-jar " + jarName);
            mcStartCmd.Args.Add("nogui");

            var bbg = new BatchBashGenerator();
            bbg.AddCommand(mcStartCmd);

            var launchScriptPath = folder + "/" + scriptName;
            File.WriteAllText(launchScriptPath + ".bat", bbg.GenerateBatch());
            File.WriteAllText(launchScriptPath + ".sh", bbg.GenerateBash());
            ChmodPlusXScript(folder, scriptName);
        }
        public static void GenBedrockScriptLinux(string folder, string scriptName)
        {
            File.WriteAllText(folder + "/" + scriptName + ".sh", "#!/bin/bash\nLD_LIBRARY_PATH=. ./bedrock_server");
            ChmodPlusXScript(folder, scriptName);
        }
        public static void RegenScripts(string folder, string scriptname, string oldJarName, string newJarName)
        {
            var launchScriptPath = folder + "/" + scriptname;

            var bashtxt = File.ReadAllText(launchScriptPath + ".sh");
            var batchtxt = File.ReadAllText(launchScriptPath + ".bat");
            bashtxt = bashtxt.Replace(oldJarName, newJarName);
            batchtxt = batchtxt.Replace(oldJarName, newJarName);
            File.WriteAllText(launchScriptPath + ".sh", bashtxt);
            File.WriteAllText(launchScriptPath + ".bat", batchtxt);
        }
        public static void WriteServerDotProperties(
            string folder, 
            int port, 
            string level_name, 
            string online_mode,
            string command_block,
            int difficulty,
            string white_list,
            string level_type
            )
        {
            string[] serverdotproperties = {
                "server-port=" + port,
                "level-name=" + level_name,
                "online-mode=" + online_mode,
                "enable-command-block=" + command_block,
                "difficulty=" + difficulty,
                "white-list=" + white_list,
                "level-type=" + level_type
            };

            File.WriteAllLines(Path.Combine(folder, "server.properties"), serverdotproperties);
        }
        public static void BedrockWriteServerDotProperties(
            string folder,
            int port,
            string level_name,
            bool online_mode,
            bool command_block,
            int difficulty,
            bool white_list)
        {
            string[] serverdotproperties = {
                "difficulty=" + (difficulty == 0 ? "peaceful" : (difficulty == 1 ? "easy" : (difficulty == 2 ? "normal" : (difficulty == 3 ? "hard" : "")))),
                "allow-cheats=true",
                "online-mode=" + (online_mode ? "true" : "false"),
                "white-list=" + (white_list ? "true" : "false"),
                "server-port=" + port,
                "server-portv6=" + (port + 1),
                "level-name=" + level_name
            };
            File.WriteAllLines(folder + "/server.properties", serverdotproperties);
        }
        public static void WriteWhitelistsAndOps(
            string folder,
            MCOp[] ops,
            MCWhitelisted[] whitelisted)
        {
            //List<MCWhitelisted> tmpWhitelisteds = new List<MCWhitelisted>(additionalWhitelisteds);      
            //foreach(MCOp x in ops) tmpWhitelisteds.Add(new MCWhitelisted(x.name, x.uuid));
            //MCWhitelisted[] finalwhitelisteds = tmpWhitelisteds.ToArray();

            File.WriteAllText(Path.Combine(folder, "ops.json"), JsonConvert.SerializeObject(ops));
            Console.WriteLine(JsonConvert.SerializeObject(ops));
            Console.WriteLine();
            File.WriteAllText(Path.Combine(folder, "whitelist.json"), JsonConvert.SerializeObject(whitelisted));
            Console.WriteLine(JsonConvert.SerializeObject(whitelisted));
        }
        public static void BedrockWriteWhitelists(string folder, BedrockWhitelisted[] whitelisted)
        {
            File.WriteAllText(Path.Combine(folder, "whitelist.json"), JsonConvert.SerializeObject(whitelisted));
        }
        public static void WriteWhitelistsAndOps(string folder, Tuple<MCOp[], MCWhitelisted[]> tup)
        {
            WriteWhitelistsAndOps(folder, tup.Item1, tup.Item2);
        }

        public static Tuple<MCOp[], MCWhitelisted[]> GetOppedAndWhitelisted(string[] ops, string[] whitelisted)
        {
            List<MCOp> mcops = ops
                .Select(
                x => 
                new MCOp
                (x, 
                new Guid(JsonConvert.DeserializeObject<MCUser>(InputTools.UrlGet("https://api.mojang.com/users/profiles/minecraft/" + x)).id).ToString()
                )).ToList();
            List<MCWhitelisted> whitelistedl = whitelisted.Select(x => new MCWhitelisted(x, new Guid(JsonConvert.DeserializeObject<MCUser>(InputTools.UrlGet("https://api.mojang.com/users/profiles/minecraft/" + x)).id).ToString())).ToList();

            List<MCWhitelisted> tmpWhitelisteds = new List<MCWhitelisted>(whitelistedl);
            foreach (MCOp x in mcops) tmpWhitelisteds.Add(new MCWhitelisted(x.name, x.uuid));
            MCWhitelisted[] finalwhitelisteds = tmpWhitelisteds.ToArray();
            return Tuple.Create(mcops.ToArray(), finalwhitelisteds);
        }






    }
    public class Folders
    {
        public string Source { get; private set; }
        public string Target { get; private set; }

        public Folders(string source, string target)
        {
            Source = source;
            Target = target;
        }
    }
}
