using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace VsRomp
{
    [DefaultValue(text)]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum VariableType
    {
        text,
        boolean,
        list,
        map
    }

    public sealed class RompVariables : Dictionary<string, RompVariables.Variable>
    {
        public const string SchemaUrl = "https://gist.githubusercontent.com/BenLubar/c00bc6513b5020acbea1b74a3b7a9155/raw/c3963caa7b4296e395ab2d406793d94bc7190dc3/gistfile3.txt";

        [JsonConverter(typeof(Converter))]
        public sealed class Variable
        {
            /// <summary>
            /// A string of the variable's value itself.
            /// </summary>
            [JsonProperty(PropertyName = "value")]
            public string Value { get; set; }

            /// <summary>
            /// A string to document the variable's intended usage.
            /// </summary>
            [JsonProperty(PropertyName = "description", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
            public string Description { get; set; }

            /// <summary>
            /// A boolean to indicate that a value is required; when a required variable does not have value (i.e. not null or empty), the installation will not proceed; defaults to false if not specified.
            /// </summary>
            [JsonProperty(PropertyName = "required")]
            public bool Required { get; set; }

            /// <summary>
            /// A boolean to indicate that the value should not be displayed in logs; default is false Note: an installation script may still Log the value of the variable to the console.
            /// </summary>
            [JsonProperty(PropertyName = "sensitive", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
            public bool Sensitive { get; set; }

            /// <summary>
            /// A string of the variable’s type; text (default), boolean (indicates only true or false string literals are allowed, list, or map.
            /// </summary>
            [JsonProperty(PropertyName = "type")]
            public VariableType Type { get; set; }

            /// <summary>
            /// An array of strings of values that will constrain the value when listRestrict is true.
            /// </summary>
            [JsonProperty(PropertyName = "listValues")]
            public ObservableCollection<StringValue> ListValues { get; } = new ObservableCollection<StringValue>();

            [EditorBrowsable(EditorBrowsableState.Never)]
            public bool ShouldSerializeListValues() => this.ListValues.Count > 0;

            /// <summary>
            /// A boolean to indicate that the value must contain one or the strings (when type is text) from listValues,
            /// or must be an array containing only strings from listValues (when type is list).
            /// </summary>
            [JsonProperty(PropertyName = "listRestrict", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
            public bool ListRestrict { get; set; }

            private sealed class Converter : JsonConverter
            {
                public override bool CanWrite => false;

                public override bool CanConvert(Type objectType)
                {
                    return objectType == typeof(Variable);
                }

                public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
                {
                    switch (reader.TokenType)
                    {
                        case JsonToken.Null:
                            return new Variable { Value = null, Required = true };
                        case JsonToken.String:
                            return new Variable { Value = (string)reader.Value, Required = true };
                        case JsonToken.StartObject:
                            var value = new Variable();
                            serializer.Populate(reader, value);
                            return value;
                        default:
                            throw new JsonSerializationException();
                    }
                }

                public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
                {
                    throw new NotSupportedException();
                }
            }
        }
    }

    [JsonConverter(typeof(Converter))]
    public sealed class StringValue : Commands.Configure.IHasName
    {
        public static implicit operator StringValue(string value) => new StringValue { Value = value };
        public static implicit operator string(StringValue value) => value.Value;

        public string Value { get; set; }

        string Commands.Configure.IHasName.Name
        {
            get => this.Value;
            set => this.Value = value;
        }

        private sealed class Converter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(StringValue);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return new StringValue { Value = (string)reader.Value };
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteValue((string)(StringValue)value);
            }
        }
    }
}
