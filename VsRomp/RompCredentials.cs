using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace VsRomp
{
    public sealed class RompCredentials : List<RompCredentials.ResourceCredential>
    {
        public const string SchemaUrl = "https://gist.githubusercontent.com/BenLubar/c00bc6513b5020acbea1b74a3b7a9155/raw/c3963caa7b4296e395ab2d406793d94bc7190dc3/gistfile4.txt";

        public sealed class ResourceCredential : Commands.Configure.IHasName
        {
            [JsonProperty(PropertyName = "type")]
            public string Type { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "description", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
            public string Description { get; set; }

            [JsonProperty(PropertyName = "restricted", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
            public bool Restricted { get; set; }

            [JsonProperty(PropertyName = "defaults")]
            [JsonConverter(typeof(KeyValueCollectionConverter))]
            public ObservableCollection<NameValue> Defaults { get; } = new ObservableCollection<NameValue>();

            public sealed class NameValue : Commands.Configure.IHasName
            {
                public string Name { get; set; }
                public string Value { get; set; }
            }

            private sealed class KeyValueCollectionConverter : JsonConverter
            {
                public override bool CanConvert(Type objectType)
                {
                    return objectType == typeof(ObservableCollection<NameValue>);
                }

                public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
                {
                    var obj = (JObject)JToken.ReadFrom(reader);
                    var collection = (ObservableCollection<NameValue>)existingValue ?? new ObservableCollection<NameValue>();

                    collection.Clear();
                    foreach (var kv in obj)
                    {
                        collection.Add(new NameValue { Name = kv.Key, Value = kv.Value.Value<string>() });
                    }

                    return collection;
                }

                public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
                {
                    writer.WriteStartObject();
                    foreach (var kv in (ObservableCollection<NameValue>)value)
                    {
                        writer.WritePropertyName(kv.Name);
                        if (string.IsNullOrEmpty(kv.Value))
                        {
                            writer.WriteNull();
                        }
                        else
                        {
                            writer.WriteValue(kv.Value);
                        }
                    }
                    writer.WriteEndObject();
                }
            }
        }
    }
}
