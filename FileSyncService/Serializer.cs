using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace FileSyncService
{
    public class Serializer
    {
        public ConfigData Configdata { get; set; }

        public Serializer()
        {
            using (StreamReader file = File.OpenText(@"config.json"))
            {
                string json = file.ReadToEnd();
                Configdata = JsonConvert.DeserializeObject<ConfigData>(json);
            }
        }
    }

    public class ConfigData
    {
        public string folder { get; set; }
        public string source { get; set; }
        public string destination { get; set; }
        public string logfile { get; set; }
    }
}
