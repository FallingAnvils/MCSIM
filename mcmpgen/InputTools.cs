using System;
using System.IO;
using System.Net;
using System.Linq;

namespace mcmpgen
{
    public class InputTools
    {
        public static string Prompt(string msg)
        {
            Console.Write(msg);
            return Console.ReadLine();
        }
        public static int PromptInt(string msg)
        {
            Console.Write(msg);
            int num; bool success = false;
            do
            {
                success = int.TryParse(Console.ReadLine(), out num);
            } while (!success);
            return num;
        }
        public static string PromptDefault(string msg, string @default = "")
        {
            Console.Write(msg);
            string ret = string.Empty;
            if (string.IsNullOrWhiteSpace(ret = Console.ReadLine()))
                return @default;
            else
                return ret;
        }
        public static int PromptDefaultInt(string msg, int @default = 0)
        {
            Console.Write(msg);
            int num; bool success = false;
            do
            {
                string input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) return @default;
                success = int.TryParse(input, out num);
            } while (!success);
            return num;
        }
        public static string UrlGet(string url)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            var response = request.GetResponse();
            var stream = response.GetResponseStream();
            var reader = new StreamReader(stream);
            var responsetxt = reader.ReadToEnd();
            reader.Close();
            stream.Close();
            response.Close();
            return responsetxt;
        }
        public static string GetWithBanned(string msg, string bannedMsg, string[] banned, bool showBanned = true)
        {
            if(showBanned)
            {
                Console.WriteLine("Banned inputs are " + string.Join(", ", banned) + ".");
            }
            bool success = false; string response;
            do
            {
                Console.Write(msg);
                response = Console.ReadLine();
                if(banned.Contains(response))
                {
                    Console.WriteLine(bannedMsg);
                }
                else
                {
                    success = true;
                }
            } while (!success);
            return response;
        }
    }
}
