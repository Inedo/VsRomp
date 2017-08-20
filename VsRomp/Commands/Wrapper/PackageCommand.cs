using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading;

namespace VsRomp.Commands.Wrapper
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class PackageCommand : WrapperCommandBase
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0102;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private PackageCommand(VsRompPackage package) : base(package, CommandId)
        {
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static PackageCommand Instance
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
            Instance = new PackageCommand(package);
        }

        internal override void Run()
        {
            var project = this.Package.SelectedProject;
            var projectDir = Path.GetDirectoryName(project.FullName);
            var packageDir = Path.Combine(projectDir, ".upack", "package");

            WaitCallback run = _ =>
            {
                try
                {
                    var buildProject = ProjectCollection.GlobalProjectCollection.GetLoadedProjects(project.FullName).First();
                    var buildInstance = buildProject.CreateProjectInstance();
                    if (((IEnumerable)project.ExtenderNames).Cast<string>().Contains("WebApplication"))
                    {
                        buildInstance.SetProperty("WPPAllFilesInSingleFolder", packageDir);
                        buildInstance.Build(new[] { "Build", "PipelinePreDeployCopyAllFilesToOneFolder" }, new ILogger[] { new BuildLogger(this.CreateOutputPane()) });
                    }
                    else
                    {
                        buildInstance.SetProperty("OutputPath", packageDir);
                        buildInstance.Build("Build", new ILogger[] { new BuildLogger(this.CreateOutputPane()) });
                    }

                    this.Romp("pack .upack");
                }
                catch
                {
                    return;
                }
                finally
                {
                    if (Directory.Exists(packageDir))
                    {
                        Directory.Delete(packageDir, true);
                    }
                }
            };

            if (!Directory.Exists(packageDir))
            {
                ThreadPool.QueueUserWorkItem(run);
                return;
            }

            if (VsShellUtilities.ShowMessageBox(this.ServiceProvider, "The \"package\" directory will be deleted by this command.\n\nSelect \"OK\" to continue or \"Cancel\" to stop.", "The directory \"" + packageDir + "\" already exists.", OLEMSGICON.OLEMSGICON_WARNING, OLEMSGBUTTON.OLEMSGBUTTON_OKCANCEL, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND) != 1)
            {
                return;
            }

            ThreadPool.QueueUserWorkItem(_ =>
            {
                Directory.Delete(packageDir, true);
                run(_);
            });
        }

        private sealed class BuildLogger : Logger
        {
            private readonly IVsOutputWindowPane outputPane;

            public BuildLogger(IVsOutputWindowPane outputPane)
            {
                this.outputPane = outputPane;
            }

            private void WriteLine(string message)
            {
                int hr = this.outputPane.OutputStringThreadSafe(message + "\n");
                ErrorHandler.ThrowOnFailure(hr);
            }

            public override void Initialize(IEventSource eventSource)
            {
                eventSource.ErrorRaised += (s, e) =>
                {
                    this.WriteLine(this.FormatErrorEvent(e));
                };
                eventSource.WarningRaised += (s, e) =>
                {
                    this.WriteLine(this.FormatWarningEvent(e));
                };
            }
        }
    }
}
