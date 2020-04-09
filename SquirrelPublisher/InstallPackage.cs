using System;
using System.Linq;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;
using EnvDTE;

namespace SquirrelPublisher
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class InstallPackage
    {
        private EnvDTE.DTE _dteService;

        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("ec5a7cb8-667f-43ca-86c3-b27484bfc14c");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstallPackage"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private InstallPackage(AsyncPackage package, OleMenuCommandService commandService, EnvDTE.DTE dteService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            this._dteService = dteService ?? throw new ArgumentNullException(nameof(dteService));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static InstallPackage Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
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
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in InstallPackage's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            EnvDTE.DTE dteService = await package.GetServiceAsync(typeof(SDTE)) as EnvDTE.DTE;
            Instance = new InstallPackage(package, commandService, dteService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                string configuration = "Release";
                string platformTarget = "Any CPU";

                // ToDo: get PublishProject name from Settings
                var projToPublish = (string)((object[])_dteService.Solution.SolutionBuild.StartupProjects)[0];

                var project = _dteService.Solution.Projects.OfType<Project>().FirstOrDefault(x => x.UniqueName == projToPublish);
                var projShortName = System.IO.Path.GetFileNameWithoutExtension(projToPublish);
                var outputPath = projToPublish;

                Logger.Log($"------ Build started: Project: {projShortName}, Configuration: {configuration} {platformTarget} ------", true);

                _dteService.Solution.SolutionBuild.BuildProject(configuration, projToPublish, true);

                if (_dteService.Solution.SolutionBuild.BuildState != vsBuildState.vsBuildStateDone ||
                    _dteService.Solution.SolutionBuild.LastBuildInfo != 0)
                {
                    Logger.Log($"========== Build: 0 succeeded, 1 failed, 0 up-to-date, 0 skipped ==========", true);
                    Logger.Log($"========== Publish: 0 succeeded, 0 failed, 1 skipped ==========", true);
                    throw new Exception("Build failed. Check the output window for more details.");
                }

                Logger.Log($"========== Build: 1 succeeded, 0 failed, 0 up-to-date, 0 skipped ==========", true);
                Logger.Log($"------ Publish started: Project: {projShortName}, Configuration: {configuration} {platformTarget} ------", true);

                // object result = await Publisher.PublishProject(outputPath);

                Logger.Log($"========== Publish: 0 succeeded, 0 failed, 1 skipped ==========", true);
            }
            catch (Exception ex)
            {
                VsShellUtilities.ShowMessageBox(
                    this.package,
                    $"Publish has encountered an error.{System.Environment.NewLine}" +
                    $"Publishing failed.{System.Environment.NewLine}{System.Environment.NewLine}" +
                    ex.Message,
                    "Publish failed",
                    OLEMSGICON.OLEMSGICON_CRITICAL,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }

            VsShellUtilities.ShowMessageBox(
                this.package,
                "Publishing is complete",
                "Publish succeeded",
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}