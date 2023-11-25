using System;
using System.Diagnostics;

namespace ParserWebCore.NetworkLibrary
{
    public class CurlDownloadSportMaster
    {
        public static string DownL(string arguments)
        {
            var cliProcess = new Process()
            {
                StartInfo = new ProcessStartInfo("curl", arguments)
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WorkingDirectory = Environment.SystemDirectory
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