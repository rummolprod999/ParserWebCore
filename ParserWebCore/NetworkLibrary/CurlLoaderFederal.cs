using System.Diagnostics;

namespace ParserWebCore.NetworkLibrary
{
    public static class CurlLoaderFederal
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
            var cliOut = cliProcess.StandardOutput.ReadToEnd();
            cliProcess.WaitForExit();
            cliProcess.Close();

            return cliOut;
        }
    }
}