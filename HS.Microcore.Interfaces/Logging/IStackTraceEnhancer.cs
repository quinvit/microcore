using System;
using Newtonsoft.Json.Linq;

namespace HS.Microcore.Interfaces.Logging
{
    public interface IStackTraceEnhancer
    {
        string Clean(string stackTrace);
        JObject ToJObjectWithBreadcrumb(Exception exception);
    }
}