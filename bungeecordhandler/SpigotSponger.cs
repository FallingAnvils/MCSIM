using System.Collections.Generic;
using System.IO;

namespace bungeecordhandler
{
    public class Spigoter
    {
        string instancePath;
        string bungeePath;

        public Spigoter(string instancePath, string bungeePath)
        {
            this.instancePath = instancePath;
            this.bungeePath = bungeePath;
        }

        public void Spigot(string instanceName, int port)
        {
            var bungeeConfigFile = Path.Combine(bungeePath, "config.yml");
            File.WriteAllText(Path.Combine(instancePath, "spigot.yml"), @"
settings:
  bungeecord: true
");



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
