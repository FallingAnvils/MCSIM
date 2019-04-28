using System.IO;

namespace mcmpgen
{
    public class MCTools
    {
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
    }
}
