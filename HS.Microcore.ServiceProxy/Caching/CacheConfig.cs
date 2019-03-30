using System;
using System.Collections.Generic;
using HS.Microcore.Interfaces.Configuration;

namespace HS.Microcore.ServiceProxy.Caching
{
    [ConfigurationRoot("Cache", RootStrategy.ReplaceClassNameWithPath)]
    public class CacheConfig: IConfigObject
    {
        public bool LogRevokes { get; set; } = false;
        public Dictionary<string, CacheGroupConfig> Groups { get; } = new Dictionary<string, CacheGroupConfig>(StringComparer.InvariantCultureIgnoreCase);
    }

    public class CacheGroupConfig
    {
        public bool WriteExtraLogs { get; set; } = false;
    }
}
