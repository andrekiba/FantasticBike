using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FantasticBike.Assembler.Infrastructure
{
    public static class EventSerialization
    {
        public static JsonSerializerSettings Settings
        {
            get
            {
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    ContractResolver = new MessageResolver(),
                    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
                };
                return settings;
            }
        }
    }
    public class MessageResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);
            
            if (prop.Writable)
                return prop;
            
            var property = member as PropertyInfo;
            var hasPrivateSetter = property?.GetSetMethod(true) != null;
            prop.Writable = hasPrivateSetter;
            return prop;
        }
    }
}