using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;

namespace VsRomp
{
    /// <summary>
    /// Romp Configuration file
    /// </summary>
    public sealed class RompPackage
    {
        public const string SchemaUrl = "https://gist.githubusercontent.com/BenLubar/c00bc6513b5020acbea1b74a3b7a9155/raw/c3963caa7b4296e395ab2d406793d94bc7190dc3/gistfile1.txt";

        /// <summary>
        /// Property names in this array do not cascade, nor may they be edited using the configure command. This is a meta paramter, and may not be specified as a commandline argument.
        /// </summary>
        [JsonProperty(PropertyName = "lockedParameters")]
        public SortedSet<string> LockedParameters { get; } = new SortedSet<string>();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeLockedParameters() => this.LockedParameters.Count > 0;

        /// <summary>
        /// Packages will be cached.
        /// </summary>
        [JsonProperty(PropertyName = "cachePackages")]
        [DefaultValue(true)]
        public bool CachePackages { get; set; } = true;

        /// <summary>
        /// When true, the local datastore will default to %user%\.romp and the local package registry will default as a user-package registry.
        /// </summary>
        [JsonProperty(PropertyName = "userMode")]
        [DefaultValue(false)]
        public bool UserMode { get; set; } = false;

        /// <summary>
        /// When set, the specified value will be used as the local data store instead of the default value.
        /// </summary>
        [JsonProperty(PropertyName = "localDataStore", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(null)]
        public string LocalDataStore { get; set; } = null;

        /// <summary>
        /// When set, the specified value will be used to create a custom package store instead of a machine or user store.
        /// </summary>
        [JsonProperty(PropertyName = "localPackageRegistry", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(null)]
        public string LocalPackageRegistry { get; set; } = null;

        /// <summary>
        /// When set, the specified value will be used to determine which package source is used when none is specified.
        /// </summary>
        [JsonProperty(PropertyName = "packageSource", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(null)]
        public string PackageSource { get; set; } = null;

        /// <summary>
        /// When true, credentials must be entered interactively when using the "store" command and may not be displayed with "display" command.
        /// </summary>
        [JsonProperty(PropertyName = "secureCredentials")]
        [DefaultValue(false)]
        public bool SecureCredentials { get; set; } = false;

        /// <summary>
        /// When true, collected configuration is stored in a local database.
        /// </summary>
        [JsonProperty(PropertyName = "storeConfiguration")]
        [DefaultValue(true)]
        public bool StoreConfiguration { get; set; } = true;

        /// <summary>
        /// When true, execution logs are stored in a local database.
        /// </summary>
        [JsonProperty(PropertyName = "storeLogs")]
        [DefaultValue(true)]
        public bool StoreLogs { get; set; } = true;

        /// <summary>
        /// The full path of a directory containing .otterx extensions to load. Default is <localDataStore>\extensions
        /// </summary>
        [JsonProperty(PropertyName = "extensionsPath", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(null)]
        public string ExtensionsPath { get; set; } = null;

        /// <summary>
        /// The full path of a directory which contains unpacked cached extensions. Default is <localDataStore>\temp\extensions
        /// </summary>
        [JsonProperty(PropertyName = "extensionsTempPath", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(null)]
        public string ExtensionsTempPath { get; set; } = null;

        /// <summary>
        /// URL of a proxy server to use for any HTTP requests.
        /// </summary>
        [JsonProperty(PropertyName = "proxy", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(null)]
        public string Proxy { get; set; } = null;

        /// <summary>
        /// A map of file system rafts that Romp has access to.
        /// Key: The name of a raft.
        /// Value: A directory containing a file system or zip file raft that Romp will use.
        /// </summary>
        [JsonProperty(PropertyName = "rafts")]
        public Dictionary<string, string> Rafts { get; } = new Dictionary<string, string>();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeRafts() => this.Rafts.Count > 0;
    }
}
