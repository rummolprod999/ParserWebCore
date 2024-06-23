using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ParserWebCore.Logger;

namespace ParserWebCore.NetworkLibrary
{
    public static class DownloadString
    {
        public static int MaxDownload;

        static DownloadString()
        {
            MaxDownload = 0;
        }

        public static string DownLUserAgentEis(string url)
        {
            MaxDownload++;
            var tmp = "";
            var count = 0;
            while (true)
            {
                try
                {
                    var task = Task.Run(() => (new TimedWebClientUaEis()).DownloadString(url));
                    if (task.Wait(TimeSpan.FromSeconds(99)))
                    {
                        tmp = task.Result;
                        break;
                    }

                    throw new TimeoutException();
                    //tmp = new TimedWebClient().DownloadString(url);
                }
                catch (WebException ex)
                {
                    if (ex.Response is HttpWebResponse r) Log.Logger("Response code: ", r.StatusCode);
                    if (ex.Response is HttpWebResponse errorResponse &&
                        errorResponse.StatusCode == HttpStatusCode.Forbidden)
                    {
                        Log.Logger("Error 403 or 434");
                        return tmp;
                    }

                    if (count >= 2)
                    {
                        Log.Logger($"Не удалось скачать xml за {count} попыток", url);
                        break;
                    }

                    Log.Logger("Не удалось получить строку xml", ex.Message, url);
                    count++;
                    Thread.Sleep(5000);
                }
                catch (Exception e)
                {
                    if (count >= 2)
                    {
                        Log.Logger($"Не удалось скачать xml за {count} попыток", url);
                        break;
                    }

                    switch (e)
                    {
                        case AggregateException a
                            when a.InnerException != null && a.InnerException.Message.Contains("(404) Not Found"):
                            Log.Logger("404 Exception", a.InnerException.Message, url);
                            goto Finish;
                        case AggregateException a
                            when a.InnerException != null && a.InnerException.Message.Contains("(403) Forbidden"):
                            Log.Logger("403 Exception", a.InnerException.Message, url);
                            goto Finish;
                        case AggregateException a when a.InnerException != null &&
                                                       a.InnerException.Message.Contains(
                                                           "The remote server returned an error: (434)"):
                            Log.Logger("434 Exception", a.InnerException.Message, url);
                            goto Finish;
                        /*case TimeoutException a:
                            Log.Logger("Timeout exception");
                            goto Finish;*/
                    }

                    Log.Logger("Не удалось получить строку xml", e, url);
                    count++;
                    Thread.Sleep(5000);
                }
            }

            Finish:
            return tmp;
        }

        public static string DownL(string url, int tryCount = 2)
        {
            var tmp = "";
            var count = 0;
            while (true)
            {
                try
                {
                    var task = Task.Run(() => (new TimedWebClient()).DownloadString(url));
                    if (!task.Wait(TimeSpan.FromSeconds(60))) throw new TimeoutException();
                    tmp = task.Result;
                    break;
                }

                catch (Exception e)
                {
                    if (count >= tryCount)
                    {
                        Log.Logger($"Не удалось скачать за {count} попыток", url);
                        break;
                    }

                    switch (e)
                    {
                        case AggregateException a
                            when a.InnerException != null && a.InnerException.Message.Contains("(404) Not Found"):
                            Log.Logger("404 Exception", a.InnerException.Message, url);
                            return tmp;
                        case AggregateException a
                            when a.InnerException != null && a.InnerException.Message.Contains("(403) Forbidden"):
                            Log.Logger("403 Exception", a.InnerException.Message, url);
                            return tmp;
                        case AggregateException a when a.InnerException != null &&
                                                       a.InnerException.Message.Contains(
                                                           "The remote server returned an error: (434)"):
                            Log.Logger("434 Exception", a.InnerException.Message, url);
                            return tmp;
                    }

                    Log.Logger("Не удалось получить строку", e, url);
                    count++;
                    Thread.Sleep(5000 * count);
                }
            }

            return tmp;
        }

        public static string DownLTektorg(string url)
        {
            var tmp = "";
            var count = 0;
            while (true)
            {
                try
                {
                    var task = Task.Run(() => (new TimedWebClientTektorg()).DownloadString(url));
                    if (!task.Wait(TimeSpan.FromSeconds(30))) throw new TimeoutException();
                    tmp = task.Result;
                    break;
                }

                catch (Exception e)
                {
                    if (count >= 2)
                    {
                        Log.Logger($"Не удалось скачать за {count} попыток", url);
                        break;
                    }

                    switch (e)
                    {
                        case AggregateException a
                            when a.InnerException != null && a.InnerException.Message.Contains("(404) Not Found"):
                            Log.Logger("404 Exception", a.InnerException.Message, url);
                            return tmp;
                        case AggregateException a
                            when a.InnerException != null && a.InnerException.Message.Contains("(403) Forbidden"):
                            Log.Logger("403 Exception", a.InnerException.Message, url);
                            return tmp;
                        case AggregateException a when a.InnerException != null &&
                                                       a.InnerException.Message.Contains(
                                                           "The remote server returned an error: (434)"):
                            Log.Logger("434 Exception", a.InnerException.Message, url);
                            return tmp;
                    }

                    Log.Logger("Не удалось получить строку", e, url);
                    count++;
                    Thread.Sleep(5000);
                }
            }

            return tmp;
        }

        public static string DownLUserAgent(string url, bool randomUa = false,
            Dictionary<string, string> headers = null)
        {
            var tmp = "";
            var count = 0;
            while (true)
            {
                try
                {
                    var task = Task.Run(() => (new TimedWebClientUa(randomUa, headers)).DownloadString(url));
                    if (!task.Wait(TimeSpan.FromSeconds(60))) throw new TimeoutException();
                    tmp = task.Result;
                    break;
                }
                catch (WebException ex)
                {
                    if (ex.Response is HttpWebResponse r) Log.Logger("Response code: ", r.StatusCode);
                    if (ex.Response is HttpWebResponse errorResponse &&
                        errorResponse.StatusCode == HttpStatusCode.Forbidden)
                    {
                        Log.Logger("Error 403 or 434");
                        return tmp;
                    }

                    if (count >= 2)
                    {
                        Log.Logger($"Не удалось скачать за {count} попыток", url);
                        break;
                    }

                    Log.Logger("Не удалось получить строку", ex.Message, url);
                    count++;
                    Thread.Sleep(5000);
                }
                catch (Exception e)
                {
                    if (count >= 2)
                    {
                        Log.Logger($"Не удалось скачать за {count} попыток", url);
                        break;
                    }

                    switch (e)
                    {
                        case AggregateException a
                            when a.InnerException != null && a.InnerException.Message.Contains("(404) Not Found"):
                            Log.Logger("404 Exception", a.InnerException.Message, url);
                            goto Finish;
                        case AggregateException a
                            when a.InnerException != null && a.InnerException.Message.Contains("(403) Forbidden"):
                            Log.Logger("403 Exception", a.InnerException.Message, url);
                            goto Finish;
                        case AggregateException a when a.InnerException != null &&
                                                       a.InnerException.Message.Contains(
                                                           "The remote server returned an error: (434)"):
                            Log.Logger("434 Exception", a.InnerException.Message, url);
                            goto Finish;
                    }

                    Log.Logger("Не удалось получить строку", e, url);
                    count++;
                    Thread.Sleep(5000);
                }
            }

            Finish:
            return tmp;
        }

        public static string DownL1251(string url)
        {
            var tmp = "";
            var count = 0;
            while (true)
            {
                try
                {
                    var task = Task.Run(() =>
                    {
                        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                        var v = new TimedWebClient { Encoding = Encoding.GetEncoding("windows-1251") };
                        return v.DownloadString(url);
                    });
                    if (!task.Wait(TimeSpan.FromSeconds(60))) throw new TimeoutException();
                    tmp = task.Result;
                    break;
                }
                catch (Exception e)
                {
                    if (count >= 2)
                    {
                        Log.Logger($"Не удалось скачать за {count} попыток", url);
                        break;
                    }

                    Log.Logger("Не удалось получить строку", e, url);
                    count++;
                    Thread.Sleep(5000);
                }
            }

            return tmp;
        }

        public static string DownLSber(string url, int num)
        {
            var tmp = "";
            var count = 0;
            while (true)
            {
                try
                {
                    var task = Task.Run(() => (new HttpPostSberB2B(num)).DownloadString(url));
                    if (!task.Wait(TimeSpan.FromSeconds(60))) throw new TimeoutException();
                    tmp = task.Result;
                    break;
                }
                catch (WebException ex)
                {
                    if (ex.Response is HttpWebResponse r) Log.Logger("Response code: ", r.StatusCode);
                    if (ex.Response is HttpWebResponse errorResponse &&
                        errorResponse.StatusCode == HttpStatusCode.Forbidden)
                    {
                        Log.Logger("Error 403 or 434");
                        return tmp;
                    }

                    if (count >= 2)
                    {
                        Log.Logger($"Не удалось скачать за {count} попыток", url);
                        break;
                    }

                    Log.Logger("Не удалось получить строку", ex.Message, url);
                    count++;
                    Thread.Sleep(5000);
                }
                catch (Exception e)
                {
                    if (count >= 2)
                    {
                        Log.Logger($"Не удалось скачать за {count} попыток", url);
                        break;
                    }

                    switch (e)
                    {
                        case AggregateException a
                            when a.InnerException != null && a.InnerException.Message.Contains("(404) Not Found"):
                            Log.Logger("404 Exception", a.InnerException.Message, url);
                            goto Finish;
                        case AggregateException a
                            when a.InnerException != null && a.InnerException.Message.Contains("(403) Forbidden"):
                            Log.Logger("403 Exception", a.InnerException.Message, url);
                            goto Finish;
                        case AggregateException a when a.InnerException != null &&
                                                       a.InnerException.Message.Contains(
                                                           "The remote server returned an error: (434)"):
                            Log.Logger("434 Exception", a.InnerException.Message, url);
                            goto Finish;
                    }

                    Log.Logger("Не удалось получить строку", e, url);
                    count++;
                    Thread.Sleep(5000);
                }
            }

            Finish:
            return tmp;
        }

        public static string DownLZakMos(string url, string data)
        {
            var tmp = "";
            var count = 0;
            while (true)
            {
                try
                {
                    var task = Task.Run(() => (new HttpPostZakMos()).DownloadString(url, data));
                    if (!task.Wait(TimeSpan.FromSeconds(60))) throw new TimeoutException();
                    tmp = task.Result;
                    break;
                }
                catch (WebException ex)
                {
                    if (ex.Response is HttpWebResponse r) Log.Logger("Response code: ", r.StatusCode);
                    if (ex.Response is HttpWebResponse errorResponse &&
                        errorResponse.StatusCode == HttpStatusCode.Forbidden)
                    {
                        Log.Logger("Error 403 or 434");
                        return tmp;
                    }

                    if (count >= 2)
                    {
                        Log.Logger($"Не удалось скачать за {count} попыток", url);
                        break;
                    }

                    Log.Logger("Не удалось получить строку", ex.Message, url);
                    count++;
                    Thread.Sleep(5000);
                }
                catch (Exception e)
                {
                    if (count >= 2)
                    {
                        Log.Logger($"Не удалось скачать за {count} попыток", url);
                        break;
                    }

                    switch (e)
                    {
                        case AggregateException a
                            when a.InnerException != null && a.InnerException.Message.Contains("(404) Not Found"):
                            Log.Logger("404 Exception", a.InnerException.Message, url);
                            goto Finish;
                        case AggregateException a
                            when a.InnerException != null && a.InnerException.Message.Contains("(403) Forbidden"):
                            Log.Logger("403 Exception", a.InnerException.Message, url);
                            goto Finish;
                        case AggregateException a when a.InnerException != null &&
                                                       a.InnerException.Message.Contains(
                                                           "The remote server returned an error: (434)"):
                            Log.Logger("434 Exception", a.InnerException.Message, url);
                            goto Finish;
                    }

                    Log.Logger("Не удалось получить строку", e, url);
                    count++;
                    Thread.Sleep(5000);
                }
            }

            Finish:
            return tmp;
        }

        public static string DownLRtsZmo(string url, string data, int section)
        {
            var tmp = "";
            var count = 0;
            while (true)
            {
                try
                {
                    var task = Task.Run(() => (new HttpZmoRts()).DownloadString(url, data, section));
                    if (!task.Wait(TimeSpan.FromSeconds(60))) throw new TimeoutException();
                    tmp = task.Result;
                    break;
                }
                catch (WebException ex)
                {
                    if (ex.Response is HttpWebResponse r) Log.Logger("Response code: ", r.StatusCode);
                    if (ex.Response is HttpWebResponse errorResponse &&
                        errorResponse.StatusCode == HttpStatusCode.Forbidden)
                    {
                        Log.Logger("Error 403 or 434");
                        return tmp;
                    }

                    if (count >= 2)
                    {
                        Log.Logger($"Не удалось скачать за {count} попыток", url);
                        break;
                    }

                    Log.Logger("Не удалось получить строку", ex.Message, url);
                    count++;
                    Thread.Sleep(5000);
                }
                catch (Exception e)
                {
                    if (count >= 2)
                    {
                        Log.Logger($"Не удалось скачать за {count} попыток", url);
                        break;
                    }

                    switch (e)
                    {
                        case AggregateException a
                            when a.InnerException != null && a.InnerException.Message.Contains("(404) Not Found"):
                            Log.Logger("404 Exception", a.InnerException.Message, url);
                            goto Finish;
                        case AggregateException a
                            when a.InnerException != null && a.InnerException.Message.Contains("(403) Forbidden"):
                            Log.Logger("403 Exception", a.InnerException.Message, url);
                            goto Finish;
                        case AggregateException a when a.InnerException != null &&
                                                       a.InnerException.Message.Contains(
                                                           "The remote server returned an error: (434)"):
                            Log.Logger("434 Exception", a.InnerException.Message, url);
                            goto Finish;
                    }

                    Log.Logger("Не удалось получить строку", e, url);
                    count++;
                    Thread.Sleep(5000);
                }
            }

            Finish:
            return tmp;
        }


        public static string DownLHttpPost(string url)
        {
            var tmp = "";
            var count = 0;
            while (true)
            {
                try
                {
                    var task = Task.Run(() => (HttpPostAll.CreateInstance()).DownloadString(url));
                    if (!task.Wait(TimeSpan.FromSeconds(60))) throw new TimeoutException();
                    tmp = task.Result;
                    break;
                }
                catch (WebException ex)
                {
                    if (ex.Response is HttpWebResponse r) Log.Logger("Response code: ", r.StatusCode);
                    if (ex.Response is HttpWebResponse errorResponse &&
                        errorResponse.StatusCode == HttpStatusCode.Forbidden)
                    {
                        Log.Logger("Error 403 or 434");
                        return tmp;
                    }

                    if (count >= 2)
                    {
                        Log.Logger($"Не удалось скачать за {count} попыток", url);
                        break;
                    }

                    Log.Logger("Не удалось получить строку", ex.Message, url);
                    count++;
                    Thread.Sleep(5000);
                }
                catch (Exception e)
                {
                    if (count >= 2)
                    {
                        Log.Logger($"Не удалось скачать за {count} попыток", url);
                        break;
                    }

                    switch (e)
                    {
                        case AggregateException a
                            when a.InnerException != null && a.InnerException.Message.Contains("(404) Not Found"):
                            Log.Logger("404 Exception", a.InnerException.Message, url);
                            goto Finish;
                        case AggregateException a
                            when a.InnerException != null && a.InnerException.Message.Contains("(403) Forbidden"):
                            Log.Logger("403 Exception", a.InnerException.Message, url);
                            goto Finish;
                        case AggregateException a when a.InnerException != null &&
                                                       a.InnerException.Message.Contains(
                                                           "The remote server returned an error: (434)"):
                            Log.Logger("434 Exception", a.InnerException.Message, url);
                            goto Finish;
                    }

                    Log.Logger("Не удалось получить строку", e, url);
                    count++;
                    Thread.Sleep(5000);
                }
            }

            Finish:
            return tmp;
        }


        public static string DownLHttpPostWithCookies(string url, string baseUrl, Cookie cookie,
            FormUrlEncodedContent postContent = null)
        {
            var tmp = "";
            var count = 0;
            while (true)
            {
                try
                {
                    var task = Task.Run(() =>
                        (HttpPostCookies.CreateInstance()).DownloadString(url, baseUrl, cookie, postContent));
                    if (!task.Wait(TimeSpan.FromSeconds(60))) throw new TimeoutException();
                    tmp = task.Result;
                    break;
                }
                catch (WebException ex)
                {
                    if (ex.Response is HttpWebResponse r) Log.Logger("Response code: ", r.StatusCode);
                    if (ex.Response is HttpWebResponse errorResponse &&
                        errorResponse.StatusCode == HttpStatusCode.Forbidden)
                    {
                        Log.Logger("Error 403 or 434");
                        return tmp;
                    }

                    if (count >= 2)
                    {
                        Log.Logger($"Не удалось скачать за {count} попыток", url);
                        break;
                    }

                    Log.Logger("Не удалось получить строку", ex.Message, url);
                    count++;
                    Thread.Sleep(5000);
                }
                catch (Exception e)
                {
                    if (count >= 2)
                    {
                        Log.Logger($"Не удалось скачать за {count} попыток", url);
                        break;
                    }

                    switch (e)
                    {
                        case AggregateException a
                            when a.InnerException != null && a.InnerException.Message.Contains("(404) Not Found"):
                            Log.Logger("404 Exception", a.InnerException.Message, url);
                            goto Finish;
                        case AggregateException a
                            when a.InnerException != null && a.InnerException.Message.Contains("(403) Forbidden"):
                            Log.Logger("403 Exception", a.InnerException.Message, url);
                            goto Finish;
                        case AggregateException a when a.InnerException != null &&
                                                       a.InnerException.Message.Contains(
                                                           "The remote server returned an error: (434)"):
                            Log.Logger("434 Exception", a.InnerException.Message, url);
                            goto Finish;
                    }

                    Log.Logger("Не удалось получить строку", e, url);
                    count++;
                    Thread.Sleep(5000);
                }
            }

            Finish:
            return tmp;
        }

        public static string DownLHttpPostWithCookiesAll(string url, string baseUrl, CookieCollection cookie,
            FormUrlEncodedContent postContent = null, bool useProxy = false)
        {
            var tmp = "";
            var count = 0;
            while (true)
            {
                try
                {
                    var task = Task.Run(() =>
                        (HttpPostCookiesAll.CreateInstance()).DownloadString(url, baseUrl, cookie, postContent,
                            useProxy: useProxy));
                    if (!task.Wait(TimeSpan.FromSeconds(60))) throw new TimeoutException();
                    tmp = task.Result;
                    break;
                }
                catch (WebException ex)
                {
                    if (ex.Response is HttpWebResponse r) Log.Logger("Response code: ", r.StatusCode);
                    if (ex.Response is HttpWebResponse errorResponse &&
                        errorResponse.StatusCode == HttpStatusCode.Forbidden)
                    {
                        Log.Logger("Error 403 or 434");
                        return tmp;
                    }

                    if (count >= 2)
                    {
                        Log.Logger($"Не удалось скачать за {count} попыток", url);
                        break;
                    }

                    Log.Logger("Не удалось получить строку", ex.Message, url);
                    count++;
                    Thread.Sleep(5000);
                }
                catch (Exception e)
                {
                    if (count >= 2)
                    {
                        Log.Logger($"Не удалось скачать за {count} попыток", url);
                        break;
                    }

                    switch (e)
                    {
                        case AggregateException a
                            when a.InnerException != null && a.InnerException.Message.Contains("(404) Not Found"):
                            Log.Logger("404 Exception", a.InnerException.Message, url);
                            goto Finish;
                        case AggregateException a
                            when a.InnerException != null && a.InnerException.Message.Contains("(403) Forbidden"):
                            Log.Logger("403 Exception", a.InnerException.Message, url);
                            goto Finish;
                        case AggregateException a when a.InnerException != null &&
                                                       a.InnerException.Message.Contains(
                                                           "The remote server returned an error: (434)"):
                            Log.Logger("434 Exception", a.InnerException.Message, url);
                            goto Finish;
                    }

                    Log.Logger("Не удалось получить строку", e, url);
                    count++;
                    Thread.Sleep(5000);
                }
            }

            Finish:
            return tmp;
        }

        public static string DownLHttpPostWithCookiesAll(string url)
        {
            var tmp = "";
            var count = 0;
            while (true)
            {
                try
                {
                    var task = Task.Run(() =>
                        (HttpPostSpg.CreateInstance()).DownloadString(url));
                    if (!task.Wait(TimeSpan.FromSeconds(60))) throw new TimeoutException();
                    tmp = task.Result;
                    break;
                }
                catch (WebException ex)
                {
                    if (ex.Response is HttpWebResponse r) Log.Logger("Response code: ", r.StatusCode);
                    if (ex.Response is HttpWebResponse errorResponse &&
                        errorResponse.StatusCode == HttpStatusCode.Forbidden)
                    {
                        Log.Logger("Error 403 or 434");
                        return tmp;
                    }

                    if (count >= 2)
                    {
                        Log.Logger($"Не удалось скачать за {count} попыток", url);
                        break;
                    }

                    Log.Logger("Не удалось получить строку", ex.Message, url);
                    count++;
                    Thread.Sleep(5000);
                }
                catch (Exception e)
                {
                    if (count >= 2)
                    {
                        Log.Logger($"Не удалось скачать за {count} попыток", url);
                        break;
                    }

                    switch (e)
                    {
                        case AggregateException a
                            when a.InnerException != null && a.InnerException.Message.Contains("(404) Not Found"):
                            Log.Logger("404 Exception", a.InnerException.Message, url);
                            goto Finish;
                        case AggregateException a
                            when a.InnerException != null && a.InnerException.Message.Contains("(403) Forbidden"):
                            Log.Logger("403 Exception", a.InnerException.Message, url);
                            goto Finish;
                        case AggregateException a when a.InnerException != null &&
                                                       a.InnerException.Message.Contains(
                                                           "The remote server returned an error: (434)"):
                            Log.Logger("434 Exception", a.InnerException.Message, url);
                            goto Finish;
                    }

                    Log.Logger("Не удалось получить строку", e, url);
                    count++;
                    Thread.Sleep(5000);
                }
            }

            Finish:
            return tmp;
        }

        public static string DownLFederal(string url)
        {
            var tmp = "";
            var count = 0;
            while (true)
            {
                try
                {
                    var task = Task.Run(() => (new TimedWebClientFederal()).DownloadString(url));
                    if (!task.Wait(TimeSpan.FromSeconds(30))) throw new TimeoutException();
                    tmp = task.Result;
                    break;
                }

                catch (Exception e)
                {
                    if (count >= 2)
                    {
                        Log.Logger($"Не удалось скачать за {count} попыток", url);
                        break;
                    }

                    switch (e)
                    {
                        case AggregateException a
                            when a.InnerException != null && a.InnerException.Message.Contains("(404) Not Found"):
                            Log.Logger("404 Exception", a.InnerException.Message, url);
                            return tmp;
                        case AggregateException a
                            when a.InnerException != null && a.InnerException.Message.Contains("(403) Forbidden"):
                            Log.Logger("403 Exception", a.InnerException.Message, url);
                            return tmp;
                        case AggregateException a when a.InnerException != null &&
                                                       a.InnerException.Message.Contains(
                                                           "The remote server returned an error: (434)"):
                            Log.Logger("434 Exception", a.InnerException.Message, url);
                            return tmp;
                    }

                    Log.Logger("Не удалось получить строку", e, url);
                    count++;
                    Thread.Sleep(5000);
                }
            }

            return tmp;
        }

        public static string DownLUserAgentB2B(string url, bool randomUa = false)
        {
            var tmp = "";
            var count = 0;
            while (true)
            {
                try
                {
                    var task = Task.Run(() => (new TimedWebClientUaB2B(randomUa)).DownloadString(url));
                    if (!task.Wait(TimeSpan.FromSeconds(60))) throw new TimeoutException();
                    tmp = task.Result;
                    break;
                }
                catch (WebException ex)
                {
                    if (ex.Response is HttpWebResponse r) Log.Logger("Response code: ", r.StatusCode);
                    if (ex.Response is HttpWebResponse errorResponse &&
                        errorResponse.StatusCode == HttpStatusCode.Forbidden)
                    {
                        Log.Logger("Error 403 or 434");
                        return tmp;
                    }

                    if (count >= 2)
                    {
                        Log.Logger($"Не удалось скачать за {count} попыток", url);
                        break;
                    }

                    Log.Logger("Не удалось получить строку", ex.Message, url);
                    count++;
                    Thread.Sleep(5000);
                }
                catch (Exception e)
                {
                    if (count >= 2)
                    {
                        Log.Logger($"Не удалось скачать за {count} попыток", url);
                        break;
                    }

                    switch (e)
                    {
                        case AggregateException a
                            when a.InnerException != null && a.InnerException.Message.Contains("(404) Not Found"):
                            Log.Logger("404 Exception", a.InnerException.Message, url);
                            goto Finish;
                        case AggregateException a
                            when a.InnerException != null && a.InnerException.Message.Contains("(403) Forbidden"):
                            Log.Logger("403 Exception", a.InnerException.Message, url);
                            goto Finish;
                        case AggregateException a when a.InnerException != null &&
                                                       a.InnerException.Message.Contains(
                                                           "The remote server returned an error: (434)"):
                            Log.Logger("434 Exception", a.InnerException.Message, url);
                            goto Finish;
                    }

                    Log.Logger("Не удалось получить строку", e, url);
                    count++;
                    Thread.Sleep(5000);
                }
            }

            Finish:
            return tmp;
        }

        public static string DownLHttpPostWithCookiesB2b(string url, CookieCollection cookie,
            FormUrlEncodedContent postContent = null, bool useProxy = false, Dictionary<string, string> headers = null)
        {
            var tmp = "";
            var count = 0;
            while (true)
            {
                try
                {
                    var task = Task.Run(() =>
                        (HttpPostCookiesB2b.CreateInstance()).DownloadString(url, cookie, postContent,
                            useProxy: useProxy, headers));
                    if (!task.Wait(TimeSpan.FromSeconds(60))) throw new TimeoutException();
                    tmp = task.Result;
                    break;
                }
                catch (WebException ex)
                {
                    if (ex.Response is HttpWebResponse r) Log.Logger("Response code: ", r.StatusCode);
                    if (ex.Response is HttpWebResponse errorResponse &&
                        errorResponse.StatusCode == HttpStatusCode.Forbidden)
                    {
                        Log.Logger("Error 403 or 434");
                        return tmp;
                    }

                    if (count >= 2)
                    {
                        Log.Logger($"Не удалось скачать за {count} попыток", url);
                        break;
                    }

                    Log.Logger("Не удалось получить строку", ex.Message, url);
                    count++;
                    Thread.Sleep(5000);
                }
                catch (Exception e)
                {
                    if (count >= 2)
                    {
                        Log.Logger($"Не удалось скачать за {count} попыток", url);
                        break;
                    }

                    switch (e)
                    {
                        case AggregateException a
                            when a.InnerException != null && a.InnerException.Message.Contains("(404) Not Found"):
                            Log.Logger("404 Exception", a.InnerException.Message, url);
                            goto Finish;
                        case AggregateException a
                            when a.InnerException != null && a.InnerException.Message.Contains("(403) Forbidden"):
                            Log.Logger("403 Exception", a.InnerException.Message, url);
                            goto Finish;
                        case AggregateException a when a.InnerException != null &&
                                                       a.InnerException.Message.Contains(
                                                           "The remote server returned an error: (434)"):
                            Log.Logger("434 Exception", a.InnerException.Message, url);
                            goto Finish;
                    }

                    Log.Logger("Не удалось получить строку", e, url);
                    count++;
                    Thread.Sleep(5000);
                }
            }

            Finish:
            return tmp;
        }

        public static string DownLMedsi(string url, int num)
        {
            var tmp = "";
            var count = 0;
            while (true)
            {
                try
                {
                    var task = Task.Run(() => (new HttpPostMedsi(num)).DownloadString(url));
                    if (!task.Wait(TimeSpan.FromSeconds(60))) throw new TimeoutException();
                    tmp = task.Result;
                    break;
                }
                catch (WebException ex)
                {
                    if (ex.Response is HttpWebResponse r) Log.Logger("Response code: ", r.StatusCode);
                    if (ex.Response is HttpWebResponse errorResponse &&
                        errorResponse.StatusCode == HttpStatusCode.Forbidden)
                    {
                        Log.Logger("Error 403 or 434");
                        return tmp;
                    }

                    if (count >= 2)
                    {
                        Log.Logger($"Не удалось скачать за {count} попыток", url);
                        break;
                    }

                    Log.Logger("Не удалось получить строку", ex.Message, url);
                    count++;
                    Thread.Sleep(5000);
                }
                catch (Exception e)
                {
                    if (count >= 2)
                    {
                        Log.Logger($"Не удалось скачать за {count} попыток", url);
                        break;
                    }

                    switch (e)
                    {
                        case AggregateException a
                            when a.InnerException != null && a.InnerException.Message.Contains("(404) Not Found"):
                            Log.Logger("404 Exception", a.InnerException.Message, url);
                            goto Finish;
                        case AggregateException a
                            when a.InnerException != null && a.InnerException.Message.Contains("(403) Forbidden"):
                            Log.Logger("403 Exception", a.InnerException.Message, url);
                            goto Finish;
                        case AggregateException a when a.InnerException != null &&
                                                       a.InnerException.Message.Contains(
                                                           "The remote server returned an error: (434)"):
                            Log.Logger("434 Exception", a.InnerException.Message, url);
                            goto Finish;
                    }

                    Log.Logger("Не удалось получить строку", e, url);
                    count++;
                    Thread.Sleep(5000);
                }
            }

            Finish:
            return tmp;
        }
    }
}