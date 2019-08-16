using System;
using System.Net.Http;
using Hyperscale.Common.Contracts.Exceptions;
using Hyperscale.Microcore.SharedLogic.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hyperscale.Microcore.SharedLogic.Exceptions
{
    internal class StripHttpRequestExceptionConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(HttpRequestException);
        }


        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var httpException = (HttpRequestException)value;

            var innerException = httpException?.InnerException ??
                                 new EnvironmentException("[HttpRequestException] " + httpException?.RawMessage(),
                                     unencrypted: new Tags { { "originalStackTrace", httpException?.StackTrace } });
            
            JObject.FromObject(innerException, serializer).WriteTo(writer);
        }


        public override bool CanRead => false;
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}