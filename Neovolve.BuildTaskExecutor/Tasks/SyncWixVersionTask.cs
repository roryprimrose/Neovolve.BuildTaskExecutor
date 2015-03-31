namespace Neovolve.BuildTaskExecutor.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using Neovolve.BuildTaskExecutor.Extensibility;
    using Neovolve.BuildTaskExecutor.Properties;
    using Neovolve.BuildTaskExecutor.Services;

    /// <summary>
    /// The <see cref="SyncWixVersionTask"/>
    ///   class is used to sync a Wix project product version against the assembly version of a binary.
    /// </summary>
    [Export(typeof(ITask))]
    internal class SyncWixVersionTask : WildcardFileSearchTask
    {
        /// <summary>
        /// The settings as defined by the execution arguments.
        /// </summary>
        private VersionApplySettings _settings;

        /// <summary>
        /// Stores the source binary path that is used to obtain the assembly version.
        /// </summary>
        private String _sourceBinaryPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncWixVersionTask"/> class.
        /// </summary>
        /// <param name="eventWriter">
        /// The event writer.
        /// </param>
        /// <param name="binaryVersionReader">
        /// The binary version reader.
        /// </param>
        /// <param name="wixVersionAction">
        /// The wix version action.
        /// </param>
        [ImportingConstructor]
        public SyncWixVersionTask(
            EventWriter eventWriter, 
            [Import(VersionManagerExport.Binary)] IVersionManager binaryVersionReader, 
            [Import(VersionManagerExport.Wix)] IVersionManager wixVersionAction)
            : base(eventWriter)
        {
            if (eventWriter == null)
            {
                throw new ArgumentNullException("eventWriter");
            }

            if (binaryVersionReader == null)
            {
                throw new ArgumentNullException("binaryVersionReader");
            }

            if (wixVersionAction == null)
            {
                throw new ArgumentNullException("wixVersionAction");
            }

            BinaryVersionReader = binaryVersionReader;
            WixVersionAction = wixVersionAction;
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

            String sourcePath = GetSourceBinaryPath(arguments);

            if (String.IsNullOrWhiteSpace(sourcePath))
            {
                return false;
            }

            if (File.Exists(sourcePath) == false)
            {
                return false;
            }

            if (IsApplyMajor(arguments))
            {
                return true;
            }

            if (IsApplyMinor(arguments))
            {
                return true;
            }

            if (IsApplyBuild(arguments))
            {
                return true;
            }

            if (IsApplyRevision(arguments))
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
            Version currentVersion = WixVersionAction.ReadVersion(filePath);

            if (currentVersion == null)
            {
                Writer.WriteMessage(TraceEventType.Error, Resources.VersionAction_VersionNotFound, filePath);

                return FileMatchResult.FailTask;
            }

            Version sourceVersion = BinaryVersionReader.ReadVersion(_sourceBinaryPath);
            String newVersionText = String.Empty;

            if (_settings.ApplyMajor)
            {
                newVersionText = AppendVersion(newVersionText, sourceVersion.Major);
            }

            if (_settings.ApplyMinor)
            {
                newVersionText = AppendVersion(newVersionText, sourceVersion.Minor);
            }

            if (_settings.ApplyBuild)
            {
                newVersionText = AppendVersion(newVersionText, sourceVersion.Build);
            }

            if (_settings.ApplyRevision)
            {
                newVersionText = AppendVersion(newVersionText, sourceVersion.Revision);
            }

            Writer.WriteMessage(
                TraceEventType.Information, Resources.SyncWixVersionTask_FileUpdateNotification, filePath, currentVersion, newVersionText);

            Version newVersion = newVersionText.ToVersion();

            WixVersionAction.WriteVersion(filePath, newVersion);

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
            Boolean isApplyBuild = IsApplyBuild(arguments);
            Boolean isApplyMajor = IsApplyMajor(arguments);
            Boolean isApplyMinor = IsApplyMinor(arguments);
            Boolean isApplyRevision = IsApplyRevision(arguments);

            _settings = new VersionApplySettings(isApplyMajor, isApplyMinor, isApplyBuild, isApplyRevision);

            _sourceBinaryPath = GetSourceBinaryPath(arguments);
        }

        /// <summary>
        /// Appends the version.
        /// </summary>
        /// <param name="newVersion">
        /// The new version.
        /// </param>
        /// <param name="versionPart">
        /// The version part.
        /// </param>
        /// <returns>
        /// A <see cref="String"/> instance.
        /// </returns>
        private static String AppendVersion(String newVersion, Int32 versionPart)
        {
            if (String.IsNullOrWhiteSpace(newVersion) == false)
            {
                newVersion += ".";
            }

            newVersion += versionPart.ToString(CultureInfo.InvariantCulture);

            return newVersion;
        }

        /// <summary>
        /// Gets the source binary path.
        /// </summary>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <returns>
        /// A <see cref="String"/> instance.
        /// </returns>
        private static String GetSourceBinaryPath(IEnumerable<String> arguments)
        {
            return arguments.ParseArgument("/source:");
        }

        /// <summary>
        /// Determines whether [is apply build] [the specified arguments].
        /// </summary>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <returns>
        /// <c>true</c> if [is apply build] [the specified arguments]; otherwise, <c>false</c>.
        /// </returns>
        private static Boolean IsApplyBuild(IEnumerable<String> arguments)
        {
            return arguments.ArgumentExists("/build", "/b");
        }

        /// <summary>
        /// Determines whether [is apply major] [the specified arguments].
        /// </summary>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <returns>
        /// <c>true</c> if [is apply major] [the specified arguments]; otherwise, <c>false</c>.
        /// </returns>
        private static Boolean IsApplyMajor(IEnumerable<String> arguments)
        {
            return arguments.ArgumentExists("/major") || arguments.ArgumentExists(StringComparison.Ordinal, "/M");
        }

        /// <summary>
        /// Determines whether [is apply minor] [the specified arguments].
        /// </summary>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <returns>
        /// <c>true</c> if [is apply minor] [the specified arguments]; otherwise, <c>false</c>.
        /// </returns>
        private static Boolean IsApplyMinor(IEnumerable<String> arguments)
        {
            return arguments.ArgumentExists("/minor") || arguments.ArgumentExists(StringComparison.Ordinal, "/m");
        }

        /// <summary>
        /// Determines whether [is apply revision] [the specified arguments].
        /// </summary>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <returns>
        /// <c>true</c> if [is apply revision] [the specified arguments]; otherwise, <c>false</c>.
        /// </returns>
        private static Boolean IsApplyRevision(IEnumerable<String> arguments)
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
                StringBuilder builder = new StringBuilder("/pattern:<fileSearch> /source:<filePath> [/major|/M] [/minor|m] [/build|b] [/revision|r]");

                builder.AppendLine();
                builder.AppendLine();
                builder.AppendLine("/pattern:<fileSearch>\tThe file search pattern used to identify wix projects to synchronize.");
                builder.AppendLine("\t\t\tMust be an absolute path and may contain * wildcard characters.");
                builder.AppendLine("/source:<fileSearch>\tThe binary file to get the version number from.");
                builder.AppendLine("\t\t\tMust be an absolute path. Wildcard (*) characters are not supported.");
                builder.AppendLine("/major|/M\t\tIncludes the minor number.");
                builder.AppendLine("/minor|/m\t\tIncludes the major number.");
                builder.AppendLine("/build|/b\t\tIncludes the build number.");
                builder.AppendLine("/revision|/r\t\tIncludes the revision number.");

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
                return "Synchronizes the product version in a Wix project to the version of a binary file.";
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
                           "SyncWixVersion", "swv"
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
        /// Gets or sets the binary version reader.
        /// </summary>
        /// <value>
        /// The binary version reader.
        /// </value>
        private IVersionManager BinaryVersionReader
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the wix version action.
        /// </summary>
        /// <value>
        /// The wix version action.
        /// </value>
        private IVersionManager WixVersionAction
        {
            get;
            set;
        }
    }
}