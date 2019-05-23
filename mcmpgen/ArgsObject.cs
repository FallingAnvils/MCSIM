namespace mcmpgen
{
    public class ArgsObject
    {
        public string InstancesPath;
        public string InstanceName;
        public int RamMB;
        public string JavaArgs;
        public ServerType ServerType;
        public InstallType InstallType;
        public string Url;
        public string[] Ops;
        public string[] Whitelisted;
        public int Port = 25565;
        public string LevelName = "world";
        public bool OnlineMode = true;
        public bool CommandBlocksEnabled = true;
        public int Difficulty = 1;
        public bool WhiteList = true;
        public string[] ExcludedFiles;
        public string LevelType;
        public SpigotType SpigotType;
        public string Version;
    }
}
