#region

using System;
using System.Collections.Generic;
using System.IO;
using ParserWebCore.BuilderApp;

#endregion

namespace ParserWebCore.NetworkLibrary
{
    public class ProxyLoader
    {
        private static readonly List<ProxyEntity> proxyList = new List<ProxyEntity>();

        static ProxyLoader()
        {
            var proxyPath = $"{AppBuilder.Path}{Path.DirectorySeparatorChar}{AppBuilder.ProxyFile}";
            using (var reader = File.OpenText(proxyPath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var proxyArr = line.Split(":");
                    proxyList.Add(new ProxyEntity(proxyArr[0], int.Parse(proxyArr[1]), proxyArr[2], proxyArr[3]));
                }
            }
        }

        public static ProxyEntity getRandomProxy()
        {
            var rand = new Random();
            var index = rand.Next(0, proxyList.Count);
            return proxyList[index];
        }
    }
}