using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.IO;

namespace VsRomp.Commands.Wrapper
{
    internal abstract class WrapperCommandBase
    {
        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("a30f8d3f-5deb-4ef3-9bf3-61b145395b45");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        internal VsRompPackage Package { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WrapperCommandBase"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        protected WrapperCommandBase(VsRompPackage package, int commandId)
        {
            this.Package = package ?? throw new ArgumentNullException("package");

            if (this.ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
            {
                var menuCommandID = new CommandID(CommandSet, commandId);
                var menuItem = new MenuCommand((s, e) => this.Run(), menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        internal abstract void Run();

        private static readonly string RompExecutableName = Path.Combine(Path.GetDirectoryName(typeof(WrapperCommandBase).Assembly.Location), "romp.exe");

        private static Guid RompPaneGuid = new Guid("{610c2be5-4ae8-4de0-ac96-0ff0836fcb43}");

        internal void Romp(string arguments, Action<string> onOutput = null, string executable = null)
        {
            executable = executable ?? RompExecutableName;
            var outputPane = this.CreateOutputPane();

            using (var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo(executable, arguments)
                {
                    WorkingDirectory = Path.GetDirectoryName(this.Package.SelectedProject.FullName),
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            })
            {
                process.OutputDataReceived += (s, e) =>
                {
                    if (onOutput != null)
                    {
                        onOutput(e.Data);
                    }
                    else if (!string.IsNullOrEmpty(e.Data))
                    {
                        outputPane.OutputStringThreadSafe(e.Data + "\n");
                    }
                };
                process.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        outputPane.OutputStringThreadSafe(e.Data + "\n");
                    }
                };

                outputPane.OutputStringThreadSafe($"> {Path.GetFileName(executable)} {arguments}\n");

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                outputPane.OutputStringThreadSafe($"{Path.GetFileName(executable)} exited with code {process.ExitCode}\n");

                if (process.ExitCode != 0)
                {
                    throw new Exception($"{Path.GetFileName(executable)} exited with code {process.ExitCode}");
                }
            }
        }

        internal IVsOutputWindowPane CreateOutputPane()
        {
            var shell = (IVsUIShell)this.ServiceProvider.GetService(typeof(SVsUIShell));
            int hr = shell.FindToolWindow((uint)__VSFINDTOOLWIN.FTW_fFindFirst, VSConstants.StandardToolWindows.Output, out var outputFrame);
            ErrorHandler.ThrowOnFailure(hr);
            outputFrame.ShowNoActivate();
            var outputWindow = (IVsOutputWindow)this.ServiceProvider.GetService(typeof(SVsOutputWindow));
            hr = outputWindow.CreatePane(ref RompPaneGuid, "Romp", 1, 1);
            ErrorHandler.ThrowOnFailure(hr);
            hr = outputWindow.GetPane(ref RompPaneGuid, out var outputPane);
            ErrorHandler.ThrowOnFailure(hr);
            outputPane.Activate();
            return outputPane;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        internal IServiceProvider ServiceProvider => this.Package;

        internal string PackageFileName
        {
            get
            {
                var project = this.Package.SelectedProject;

                ProjectItem upackFolder;
                try
                {
                    upackFolder = project.ProjectItems.Item(".upack");
                }
                catch (ArgumentException)
                {
                    return null;
                }

                ProjectItem packageFile;
                try
                {
                    packageFile = upackFolder.ProjectItems.Item("rompPackage.json");
                }
                catch (ArgumentException)
                {
                    return null;
                }

                return packageFile.FileNames[1];
            }
        }
    }
}
