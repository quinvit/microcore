using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hyperscale.Microcore.ServiceDiscovery.Rewrite;
using Newtonsoft.Json;

namespace Hyperscale.Microcore.ServiceDiscovery
{
    public class ConsulQueryExecuteResponse
    {
        public ServiceEntry[] Nodes { get; set; }
    }

    public class ServiceEntry
    {
        public NodeEntry Node { get; set; }

        public AgentService Service { get; set; }

    }

    public class NodeEntry
    {
        [JsonProperty(PropertyName = "Node")]
        public string Name { get; set; }
    }

    public class AgentService
    {
        public string[] Tags { get; set; }

        public int Port { get; set; }

    }


    public class KeyValueResponse
    {
        public string Value { get; set; }

        public T DecodeValue<T>() where T : class
        {
            var serialized = Encoding.UTF8.GetString(Convert.FromBase64String(Value));
            return JsonConvert.DeserializeObject<T>(serialized);
        }
    }

    public class ServiceKeyValue
    {
        [JsonProperty("version")]
        public string Version { get; set; }
    }
}
