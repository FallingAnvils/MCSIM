using Newtonsoft.Json;
using System;

namespace bungeecordhandler
{
    class Program
    {
        static ArgsObject ParseArgs(string[] args)
        {
            return JsonConvert.DeserializeObject<ArgsObject>(args[0]);
        }

        static void Main(string[] args)
        {
            if (args.Length < 1 || args.Length > 1)
            {
                Console.WriteLine("Usage: bungeecordhandler.exe \"<escaped json here>\"");
                return;
            }
            ArgsObject aobj = ParseArgs(args);

            switch(aobj.BungeeType)
            {
                case BungeeType.Spigot:
                    Spigotify(aobj.BungeeName, aobj.InstancePath, aobj.BungeePath, aobj.Port);
                    break;
                case BungeeType.SpongeForge:
                    SpongeForgeify(aobj.BungeeName, aobj.InstancePath, aobj.BungeePath, aobj.Port);
                    break;
                case BungeeType.SpongeVanilla:
                    break;
                case BungeeType.VanillaCord:
                    break;
            }
        }

        static void Spigotify(string instanceName, string instancePath, string bungeePath, int port)
        {
            Spigoter spigoter = new Spigoter(instancePath, bungeePath);
            spigoter.Spigot(instanceName, port);
        }

        static void SpongeForgeify(string instanceName, string instancePath, string bungeePath, int port)
        {
            ForgeSponger sponger = new ForgeSponger(instancePath, bungeePath);
            Console.WriteLine("Mc version (TEMP TEMP TEMP)");
            sponger.Sponge(instanceName, port, Console.ReadLine());
        }
    }
}
