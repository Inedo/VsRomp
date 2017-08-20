using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Newtonsoft.Json;
using System;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;

namespace VsRomp.Commands.Configure
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ConfigureCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("a30f8d3f-5deb-4ef3-9bf3-61b145395b45");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly VsRompPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private ConfigureCommand(VsRompPackage package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static ConfigureCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(VsRompPackage package)
        {
            Instance = new ConfigureCommand(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            var project = this.package.SelectedProject;

            ProjectItem upackFolder;
            try
            {
                upackFolder = project.ProjectItems.Item(".upack");
            }
            catch (ArgumentException)
            {
                upackFolder = project.ProjectItems.AddFolder(".upack");
            }

            ProjectItem packageFile;
            try
            {
                packageFile = upackFolder.ProjectItems.Item("rompPackage.json");
            }
            catch (ArgumentException)
            {
                using (var file = new FileStream(Path.GetTempFileName(), FileMode.Create, FileAccess.ReadWrite, FileShare.Read, 4096, FileOptions.DeleteOnClose))
                {
                    file.Write(new[] { (byte)'{', (byte)'}', (byte)'\n' }, 0, 3);
                    file.Flush();
                    packageFile = upackFolder.ProjectItems.AddFromFileCopy(file.Name);
                    packageFile.Name = "rompPackage.json";
                }
            }

            ProjectItem upackFile;
            try
            {
                upackFile = upackFolder.ProjectItems.Item("upack.json");
            }
            catch (ArgumentException)
            {
                using (var file = new FileStream(Path.GetTempFileName(), FileMode.Create, FileAccess.ReadWrite, FileShare.Read, 4096, FileOptions.DeleteOnClose))
                using (var writer = new StreamWriter(file, new UTF8Encoding(false), 4096, true))
                {
                    Version version = null;
                    try { version = Version.Parse((string)project.Properties.Item("AssemblyVersion").Value); } catch { }
                    writer.Write(JsonConvert.SerializeObject(new { name = project.Name, version = version?.ToString(3) ?? "1.0.0" }, Formatting.Indented));
                    writer.Flush();
                    file.Flush();
                    upackFile = upackFolder.ProjectItems.AddFromFileCopy(file.Name);
                    upackFile.Name = "upack.json";
                }
            }

            ProjectItem variablesFile;
            try
            {
                variablesFile = upackFolder.ProjectItems.Item("rompVariables.json");
            }
            catch (ArgumentException)
            {
                using (var file = new FileStream(Path.GetTempFileName(), FileMode.Create, FileAccess.ReadWrite, FileShare.Read, 4096, FileOptions.DeleteOnClose))
                {
                    file.Write(new[] { (byte)'{', (byte)'}', (byte)'\n' }, 0, 3);
                    file.Flush();
                    variablesFile = upackFolder.ProjectItems.AddFromFileCopy(file.Name);
                    variablesFile.Name = "rompVariables.json";
                }
            }

            ProjectItem credentialsFile;
            try
            {
                credentialsFile = upackFolder.ProjectItems.Item("rompCredentials.json");
            }
            catch (ArgumentException)
            {
                using (var file = new FileStream(Path.GetTempFileName(), FileMode.Create, FileAccess.ReadWrite, FileShare.Read, 4096, FileOptions.DeleteOnClose))
                {
                    file.Write(new[] { (byte)'[', (byte)']', (byte)'\n' }, 0, 3);
                    file.Flush();
                    credentialsFile = upackFolder.ProjectItems.AddFromFileCopy(file.Name);
                    credentialsFile.Name = "rompCredentials.json";
                }
            }

            ProjectItem installOtterFile;
            try
            {
                installOtterFile = upackFolder.ProjectItems.Item("install.otter");
            }
            catch (ArgumentException)
            {
                using (var file = new FileStream(Path.GetTempFileName(), FileMode.Create, FileAccess.ReadWrite, FileShare.Read, 4096, FileOptions.DeleteOnClose))
                {
                    installOtterFile = upackFolder.ProjectItems.AddFromFileCopy(file.Name);
                    installOtterFile.Name = "install.otter";
                }
            }

            project.Globals["_1upack_4rompPackage_1json__JSONSchema"] = RompPackage.SchemaUrl;
            project.Globals.VariablePersists["_1upack_4rompPackage_1json__JSONSchema"] = true;

            project.Globals["_1upack_4upack_1json__JSONSchema"] = UPackMetadata.SchemaUrl;
            project.Globals.VariablePersists["_1upack_4upack_1json__JSONSchema"] = true;

            project.Globals["_1upack_4rompVariables_1json__JSONSchema"] = RompVariables.SchemaUrl;
            project.Globals.VariablePersists["_1upack_4rompVariables_1json__JSONSchema"] = true;

            try
            {
                var wizard = new ConfigureWizard(upackFile.FileNames[1], packageFile.FileNames[1], variablesFile.FileNames[1], credentialsFile.FileNames[1]);
                wizard.Show();
            }
            catch
            {
                packageFile.Open().Activate();
            }
        }
    }
}
