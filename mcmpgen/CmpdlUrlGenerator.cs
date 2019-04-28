using System.Net;

namespace Cmpdl.Tools
{
    public class CmpdlUrlGenerator
    {
        public static string ModFile(int projectID, int fileID)
        {
            return $"https://minecraft.curseforge.com/projects/{projectID}/files/{fileID}/download";
        }

        /// <summary>
        /// returns the zip filename from the url, or null if it errored
        /// </summary>
        /// <param name="url">the url</param>
        /// <returns>the zip filename from the url</returns>
        public static string FileName(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.AllowAutoRedirect = false;
                //request.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Ubuntu Chromium/53.0.2785.143 Chrome/53.0.2785.143 Safari/537.36";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                var header = response.GetResponseHeader("Location");

                // idk
                // if it breaks this is *probably* why
                if (!header.StartsWith("https://"))
                {
                    return FileName("https://" + request.Host + header);
                }
                else return header;
            }
            catch
            {
                return null;
            }

        }

        public static string RealUrl(int projectID, int fileID)
        {
            return FileName(ModFile(projectID, fileID));
        }
    }
}
