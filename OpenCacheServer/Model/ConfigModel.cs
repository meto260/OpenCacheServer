using System;
using System.Collections.Generic;
using System.Text;

namespace OpenCacheServer.Model
{
    public class ConfigModel
    {
        public Configs configs { get; set; }
    }

    public class Configs
    {
        public string AuthKey { get; set; }
        public string AdminKey { get; set; }
    }
}
