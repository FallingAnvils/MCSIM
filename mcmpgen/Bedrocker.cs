using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cmpdl_wrapper;

namespace mcmpgen
{
    public class Bedrocker
    {
        string tmpLocation;
        string tmpZipName;
        string tmpZipLocation;
        string tmpFolderLocation;
        string launchScriptName;
        string instancesPath;
        string nameOfTheInstance;
        ConsoleFileDownloader fd;

        string spigotjarname;

        public Bedrocker(
            string tmpLocation,
            string tempZipName,
            string launchScriptName,
            string instanceName,
            string instancesPath,
            ConsoleFileDownloader fd)
        {
            this.tmpLocation = tmpLocation;
            this.tmpZipName = tempZipName;
            this.launchScriptName = launchScriptName;
            this.nameOfTheInstance = instanceName;
            this.instancesPath = instancesPath;
            this.fd = fd;
            this.tmpFolderLocation = Path.Combine(tmpLocation, "bedrock");
            this.tmpZipLocation = Path.Combine(tmpLocation, tmpZipName);
        }
        public void Bedrock(
            string zipDownloadLink, 
            BedrockWhitelisted[] whitelisteds, 
            int port, 
            string level_name,
            bool online_mode,
            bool command_block,
            int difficulty,
            bool white_list
        )
        {
            Directory.CreateDirectory(tmpLocation);
            DownloadAndExtractZip(zipDownloadLink);
            MCTools.GenBedrockScriptLinux(tmpFolderLocation, launchScriptName);
            MCTools.BedrockWriteWhitelists(tmpFolderLocation, whitelisteds);
            MCTools.BedrockWriteServerDotProperties(tmpFolderLocation, port, level_name, online_mode, command_block, difficulty, white_list);
            MCTools.ChmodPlusX(tmpFolderLocation, "bedrock_server");
            MCTools.MoveAndDeleteDirectory(tmpFolderLocation, Path.Combine(instancesPath, nameOfTheInstance), tmpLocation);
        }
        public void UpdateBedrock(string zipDownloadLink, string[] excluded)
        {
            Directory.CreateDirectory(tmpLocation);
            DownloadAndExtractZip(zipDownloadLink);
            MCTools.ChmodPlusX(tmpFolderLocation, "bedrock_server");
            MCTools.MoveDirectory(tmpFolderLocation, Path.Combine(instancesPath, nameOfTheInstance), excluded);
            Directory.Delete(tmpLocation, true);
        }
        private void DownloadAndExtractZip(string zipLink)
        {
            DownloadZip(zipLink);
            tmpFolderLocation = ExtractZip();
        }
        private void DownloadZip(string zipLink)
        {
            if (Directory.Exists(tmpLocation)) Directory.Delete(tmpLocation, true);
            Directory.CreateDirectory(tmpLocation);
            fd.DownloadWithName("", zipLink, tmpLocation, tmpZipName);
        }
        private string ExtractZip()
        {
            ZipFile.ExtractToDirectory(tmpZipLocation, tmpFolderLocation);
            return tmpFolderLocation;
        }
    }
}
