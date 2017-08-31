using Newtonsoft.Json;
using System.IO;
using System.Threading;

namespace VsRomp.Commands.Wrapper
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ValidateCommand : WrapperCommandBase
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0101;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidateCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private ValidateCommand(VsRompPackage package) : base(package, CommandId)
        {
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static ValidateCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(VsRompPackage package)
        {
            Instance = new ValidateCommand(package);
        }

        internal override void Run()
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                var metadataJson = File.ReadAllText(Path.Combine(Path.GetDirectoryName(this.Package.SelectedProject.FullName), ".upack", "upack.json"));
                var metadata = JsonConvert.DeserializeObject<UPackMetadata>(metadataJson);

                try
                {
                    this.Romp("validate \"" + metadata.Name + "-" + metadata.Version + ".upack\"");
                }
                catch
                {
                }
            });
        }
    }
}
