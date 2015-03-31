namespace Neovolve.BuildTaskExecutor.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.Text;
    using Neovolve.BuildTaskExecutor.Extensibility;
    using Neovolve.BuildTaskExecutor.Properties;
    using Neovolve.BuildTaskExecutor.Services;

    /// <summary>
    /// The <see cref="IncrementAssemblyVersionTask"/>
    ///   class is used to increment the assembly version in a source file.
    /// </summary>
    [Export(typeof(ITask))]
    internal class IncrementAssemblyVersionTask : WildcardFileSearchTask
    {
        /// <summary>
        /// The settings as defined by the execution arguments.
        /// </summary>
        private VersionApplySettings _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="IncrementAssemblyVersionTask"/> class.
        /// </summary>
        /// <param name="eventWriter">
        /// The event writer.
        /// </param>
        /// <param name="versionManager">
        /// The version manager.
        /// </param>
        [ImportingConstructor]
        public IncrementAssemblyVersionTask(EventWriter eventWriter, [Import(VersionManagerExport.CSharp)] IVersionManager versionManager)
            : base(eventWriter)
        {
            if (versionManager == null)
            {
                throw new ArgumentNullException("versionManager");
            }

            VersionAction = versionManager;
        }

        /// <summary>
        /// Determines whether the specified argument set is valid.
        /// </summary>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified argument set is valid; otherwise, <c>false</c>.
        /// </returns>
        public override Boolean IsValidArgumentSet(IEnumerable<String> arguments)
        {
            if (base.IsValidArgumentSet(arguments) == false)
            {
                return false;
            }

            if (IsIncrementMajor(arguments))
            {
                return true;
            }

            if (IsIncrementMinor(arguments))
            {
                return true;
            }

            if (IsIncrementBuild(arguments))
            {
                return true;
            }

            if (IsIncrementRevision(arguments))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Called when a file has been found that matches the search pattern.
        /// </summary>
        /// <param name="filePath">
        /// The file path.
        /// </param>
        /// <returns>
        /// A <see cref="FileMatchResult"/> value.
        /// </returns>
        protected override FileMatchResult FileMatchFound(String filePath)
        {
            Version currentVersion = VersionAction.ReadVersion(filePath);

            if (currentVersion == null)
            {
                return FileMatchResult.Continue;
            }

            Version newVersion = currentVersion.IncrementVersionNumber(_settings);

            Writer.WriteMessage(
                TraceEventType.Information, Resources.IncrementAssemblyVersionTask_FileUpdateNotification, filePath, currentVersion, newVersion);

            VersionAction.WriteVersion(filePath, newVersion);

            return FileMatchResult.Continue;
        }

        /// <summary>
        /// Called before the wildcard search is executed.
        /// </summary>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        protected override void OnBeforeSearch(IEnumerable<String> arguments)
        {
            Boolean isIncrementBuild = IsIncrementBuild(arguments);
            Boolean isIncrementMajor = IsIncrementMajor(arguments);
            Boolean isIncrementMinor = IsIncrementMinor(arguments);
            Boolean isIncrementRevision = IsIncrementRevision(arguments);

            _settings = new VersionApplySettings(isIncrementMajor, isIncrementMinor, isIncrementBuild, isIncrementRevision);
        }

        /// <summary>
        /// Determines whether [is increment build] [the specified arguments].
        /// </summary>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <returns>
        /// <c>true</c> if [is increment build] [the specified arguments]; otherwise, <c>false</c>.
        /// </returns>
        private static Boolean IsIncrementBuild(IEnumerable<String> arguments)
        {
            return arguments.ArgumentExists("/build", "/b");
        }

        /// <summary>
        /// Determines whether [is increment major] [the specified arguments].
        /// </summary>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <returns>
        /// <c>true</c> if [is increment major] [the specified arguments]; otherwise, <c>false</c>.
        /// </returns>
        private static Boolean IsIncrementMajor(IEnumerable<String> arguments)
        {
            return arguments.ArgumentExists("/major") || arguments.ArgumentExists(StringComparison.Ordinal, "/M");
        }

        /// <summary>
        /// Determines whether [is increment minor] [the specified arguments].
        /// </summary>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <returns>
        /// <c>true</c> if [is increment minor] [the specified arguments]; otherwise, <c>false</c>.
        /// </returns>
        private static Boolean IsIncrementMinor(IEnumerable<String> arguments)
        {
            return arguments.ArgumentExists("/minor") || arguments.ArgumentExists(StringComparison.Ordinal, "/m");
        }

        /// <summary>
        /// Determines whether [is increment revision] [the specified arguments].
        /// </summary>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <returns>
        /// <c>true</c> if [is increment revision] [the specified arguments]; otherwise, <c>false</c>.
        /// </returns>
        private static Boolean IsIncrementRevision(IEnumerable<String> arguments)
        {
            return arguments.ArgumentExists("/revision", "/r");
        }

        /// <summary>
        /// Gets the command line argument help.
        /// </summary>
        /// <value>
        /// The command line argument help.
        /// </value>
        public override String CommandLineArgumentHelp
        {
            get
            {
                StringBuilder builder = new StringBuilder("/pattern:<fileSearch> [/major|/M] [/minor|m] [/build|b] [/revision|r]");

                builder.AppendLine();
                builder.AppendLine();
                builder.AppendLine(
                    "/pattern:<fileSearch>\tThe file search pattern used to identify files to search for the AssemblyVersionAttribute.");
                builder.AppendLine("\t\t\tMust be an absolute path and may contain * wildcard characters.");
                builder.AppendLine("/major|/M\t\tIncrement the minor number.");
                builder.AppendLine("/minor|/m\t\tIncrement the major number.");
                builder.AppendLine("/build|/b\t\tIncrement the build number.");
                builder.AppendLine("/revision|/r\t\tIncrement the revision number.");

                return builder.ToString();
            }
        }

        /// <summary>
        /// Gets the description of the task.
        /// </summary>
        /// <value>
        /// The task description.
        /// </value>
        public override String Description
        {
            get
            {
                return "Increments version number parts in AssemblyVersionAttribute entries in code files";
            }
        }

        /// <summary>
        /// Gets the command line names for the task.
        /// </summary>
        /// <value>
        /// The command line names for the task.
        /// </value>
        public override IEnumerable<String> Names
        {
            get
            {
                return new[]
                       {
                           "IncrementAssemblyVersion", "iav"
                       };
            }
        }

        /// <summary>
        /// Gets the pattern argument.
        /// </summary>
        protected override String PatternArgument
        {
            get
            {
                return "/pattern:";
            }
        }

        /// <summary>
        /// Gets or sets the version action.
        /// </summary>
        /// <value>
        /// The version action.
        /// </value>
        private IVersionManager VersionAction
        {
            get;
            set;
        }
    }
}