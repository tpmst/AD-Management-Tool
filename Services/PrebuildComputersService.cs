using ADTool.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ADTool.Services
{

    public static class PrebuildComputersManager
    {
        private static string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Prebuild.json");

        public static void SavePrebuildComputers(List<PrebuildComputers> prebuildComputers)
        {
            string serializedPrebuildComputers = JsonConvert.SerializeObject(prebuildComputers);

            File.WriteAllText(filePath, serializedPrebuildComputers);
        }
    }
}
