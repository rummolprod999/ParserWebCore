#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

#endregion

namespace ParserWebCore.NetworkLibrary
{
    public class Curl
    {
        public static string ExecuteCurl(string curlCommand, int timeoutInSeconds = 60)
        {
            if (string.IsNullOrEmpty(curlCommand))
            {
                return "";
            }

            curlCommand = curlCommand.Trim();

            // remove the curl keworkd
            if (curlCommand.StartsWith("curl"))
            {
                curlCommand = curlCommand.Substring("curl".Length).Trim();
            }

            // this code only works on windows 10 or higher
            {
                curlCommand = curlCommand.Replace("--compressed", "");

                var fullPath = Path.Combine(Environment.SystemDirectory, "curl");
                // on windows ' are not supported. For example: curl 'http://ublux.com' does not work and it needs to be replaced to curl "http://ublux.com"
                var parameters = new List<string>();


                // separate parameters to escape quotes
                try
                {
                    var q = new Queue<char>();

                    foreach (var c in curlCommand.ToCharArray())
                    {
                        q.Enqueue(c);
                    }

                    var currentParameter = new StringBuilder();

                    void insertParameter()
                    {
                        var temp = currentParameter.ToString().Trim();
                        if (string.IsNullOrEmpty(temp) == false)
                        {
                            parameters.Add(temp);
                        }

                        currentParameter.Clear();
                    }

                    while (true)
                    {
                        if (q.Count == 0)
                        {
                            insertParameter();
                            break;
                        }

                        var x = q.Dequeue();

                        if (x == '\'')
                        {
                            insertParameter();

                            // add until we find last '
                            while (true)
                            {
                                x = q.Dequeue();

                                // if next 2 characetrs are \' 
                                if (x == '\\' && q.Count > 0 && q.Peek() == '\'')
                                {
                                    currentParameter.Append('\'');
                                    q.Dequeue();
                                    continue;
                                }

                                if (x == '\'')
                                {
                                    insertParameter();
                                    break;
                                }

                                currentParameter.Append(x);
                            }
                        }
                        else if (x == '"')
                        {
                            insertParameter();

                            // add until we find last "
                            while (true)
                            {
                                x = q.Dequeue();

                                // if next 2 characetrs are \"
                                if (x == '\\' && q.Count > 0 && q.Peek() == '"')
                                {
                                    currentParameter.Append('"');
                                    q.Dequeue();
                                    continue;
                                }

                                if (x == '"')
                                {
                                    insertParameter();
                                    break;
                                }

                                currentParameter.Append(x);
                            }
                        }
                        else
                        {
                            currentParameter.Append(x);
                        }
                    }
                }
                catch
                {
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }

                    throw new Exception("Invalid curl command");
                }

                var finalCommand = new StringBuilder();

                foreach (var p in parameters)
                {
                    if (p.StartsWith("-"))
                    {
                        finalCommand.Append(p);
                        finalCommand.Append(" ");
                        continue;
                    }

                    var temp = p;

                    if (temp.Contains("\""))
                    {
                        temp = temp.Replace("\"", "\\\"");
                    }

                    if (temp.Contains("'"))
                    {
                        temp = temp.Replace("'", "\\'");
                    }

                    finalCommand.Append($"\"{temp}\"");
                    finalCommand.Append(" ");
                }


                using (var proc = new Process
                       {
                           StartInfo = new ProcessStartInfo
                           {
                               FileName = "curl",
                               Arguments = finalCommand.ToString(),
                               UseShellExecute = false,
                               RedirectStandardOutput = true,
                               RedirectStandardError = true,
                               CreateNoWindow = true,
                               WorkingDirectory = Environment.SystemDirectory
                           }
                       })
                {
                    proc.Start();

                    proc.WaitForExit(timeoutInSeconds * 1000);

                    return proc.StandardOutput.ReadToEnd();
                }
            }
        }
    }
}