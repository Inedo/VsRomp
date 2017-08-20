using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;

namespace VsRomp
{
    public sealed class UPackMetadata
    {
        public const string SchemaUrl = "https://gist.githubusercontent.com/BenLubar/c00bc6513b5020acbea1b74a3b7a9155/raw/c3963caa7b4296e395ab2d406793d94bc7190dc3/gistfile2.txt";

        /// <summary>
        /// A string of zero to fifty characters: numbers (0-9), upper- and lower-case letters (a-Z), dashes (-), periods (.),
        /// forward-slashes (/), and underscores (_); may not start or end with a forward-slash; if not specified, the group
        /// name will be considered an empty string.
        /// </summary>
        [JsonProperty(PropertyName = "group", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Group { get; set; }

        /// <summary>
        /// A string of one to fifty characters: numbers (0-9), upper- and lower-case letters (a-Z), dashes (-), periods (.),
        /// and underscores (_)
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// A string representing a Semantic Version; this is a three-part, dot-specification.
        /// </summary>
        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        /// <summary>
        /// A string of no more than fifty characters.
        /// </summary>
        [JsonProperty(PropertyName = "title", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Title { get; set; }

        /// <summary>
        /// A string of an absolute url pointing to an image to be displayed in the ProGet UI (at both 64px and 128px);
        /// if package:// is used as the protocol, ProGet will search within the package and serve that image instead.
        /// </summary>
        [JsonProperty(PropertyName = "icon", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Icon { get; set; }

        /// <summary>
        /// A string containing any number of charters; these will be formatted as Markdown in the ProGet UI.
        /// </summary>
        [JsonProperty(PropertyName = "description", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Description { get; set; }

        /// <summary>
        /// An array of strings, each consisting of a package identification string; this string is formatted as follows:
        /// 
        /// «group»:«package-name»
        /// «group»/«package-name»:«version»
        /// 
        /// When the version is not specified, the latest is used.
        /// </summary>
        [JsonProperty(PropertyName = "dependencies")]
        public List<string> Dependencies { get; } = new List<string>();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeDependencies() => this.Dependencies.Count > 0;

        [JsonExtensionData]
        public Dictionary<string, object> ExtraProperties { get; } = new Dictionary<string, object>();
    }
}
