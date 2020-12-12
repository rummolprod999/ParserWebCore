using System.Diagnostics;

namespace ParserWebCore.NetworkLibrary
{
    public class CurlLoader
    {
        public static string DownL(string url, string cookie)
        {
            var cliProcess = new Process()
            {
                StartInfo = new ProcessStartInfo("curl", $"\"{url}\" -H \"Cookie: PHPSESSID={cookie}\"")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
            };
            cliProcess.Start();
            string cliOut = cliProcess.StandardOutput.ReadToEnd();
            cliProcess.WaitForExit();
            cliProcess.Close();

            return cliOut;
        }
    }
}