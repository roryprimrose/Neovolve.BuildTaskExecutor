namespace Neovolve.BuildTaskExecutor.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using Neovolve.BuildTaskExecutor.Extensibility;
    using Neovolve.BuildTaskExecutor.Properties;
    using Neovolve.BuildTaskExecutor.Services;

    /// <summary>
    /// The <see cref="TfsCheckoutTask"/>
    ///   class is a task that searches for files to checkout from TFS.
    /// </summary>
    [Export(typeof(ITask))]
    internal class TfsCheckoutTask : WildcardFileSearchTask
    {
        /// <summary>
        /// Stores whether failures are ignored.
        /// </summary>
        private Boolean _ignoreFailures;

        /// <summary>
        /// Stores the path to the TF.exe.
        /// </summary>
        private String _tfPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="TfsCheckoutTask"/> class.
        /// </summary>
        /// <param name="eventWriter">
        /// The event writers.
        /// </param>
        [ImportingConstructor]
        public TfsCheckoutTask(EventWriter eventWriter)
            : base(eventWriter)
        {
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
            ProcessStartInfo info = new ProcessStartInfo(_tfPath, "checkout \"" + filePath + "\"")
                                    {
                                        RedirectStandardOutput = true, 
                                        RedirectStandardError = true, 
                                        UseShellExecute = false
                                    };

            Writer.WriteMessage(TraceEventType.Information, Resources.TfsCheckoutTask_CheckingOutFile, filePath);

            Process tfProcess = Process.Start(info);

            tfProcess.WaitForExit();

            if (tfProcess.ExitCode == 0)
            {
                // The process completed successfully
                using (StreamReader outputStream = tfProcess.StandardOutput)
                {
                    String successOutput = outputStream.ReadToEnd();

                    Writer.WriteMessage(TraceEventType.Information, successOutput);
                }
            }
            else
            {
                using (StreamReader outputStream = tfProcess.StandardError)
                {
                    String errorOutput = outputStream.ReadToEnd();

                    if (_ignoreFailures)
                    {
                        Writer.WriteMessage(TraceEventType.Warning, errorOutput);
                    }
                    else
                    {
                        Writer.WriteMessage(TraceEventType.Error, Resources.TfsCheckoutTask_FileCheckOutFailed, filePath);
                        Writer.WriteMessage(TraceEventType.Error, errorOutput);

                        return FileMatchResult.FailTask;
                    }
                }
            }

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
            _tfPath = ResolveTfPath();

            if (String.IsNullOrWhiteSpace(_tfPath))
            {
                throw new InvalidOperationException("The tf.exe assembly could not be found.");
            }

            _ignoreFailures = IsIgnoreFailures(arguments);

            Writer.WriteMessage(TraceEventType.Information, Resources.TfsCheckoutTask_CheckOutNotification, _tfPath);
        }

        /// <summary>
        /// Determines whether [is ignore failures] [the specified arguments].
        /// </summary>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <returns>
        /// <c>true</c> if [is ignore failures] [the specified arguments]; otherwise, <c>false</c>.
        /// </returns>
        private static Boolean IsIgnoreFailures(IEnumerable<String> arguments)
        {
            return arguments.ArgumentExists("/ignoreFailures", "/i");
        }

        /// <summary>
        /// Resolves the tf path.
        /// </summary>
        /// <returns>
        /// A <see cref="String"/> instance.
        /// </returns>
        private static String ResolveTfPath()
        {
            String basePath;

            if (Environment.Is64BitProcess)
            {
                basePath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            }
            else
            {
                basePath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            }

            String[] matchingDirectories = Directory.GetDirectories(basePath, "Microsoft Visual Studio *");

            if (matchingDirectories.Length == 0)
            {
                return String.Empty;
            }

            Regex versionExpression = new Regex("Microsoft Visual Studio (?<version>\\d+(.\\d+)?)");
            Decimal maximumVersion = 0;
            String tfPath = String.Empty;

            foreach (String matchingDirectory in matchingDirectories)
            {
                Match match = versionExpression.Match(matchingDirectory);

                if (match.Success == false)
                {
                    continue;
                }

                String versionNumber = match.Groups["version"].Value;

                Decimal version = Decimal.Parse(versionNumber, CultureInfo.InvariantCulture);

                if (version <= maximumVersion)
                {
                    continue;
                }

                // Build the tf path
                String pathToCheck = Path.Combine(matchingDirectory, "Common7\\IDE\\tf.exe");

                if (File.Exists(pathToCheck))
                {
                    tfPath = pathToCheck;
                    maximumVersion = Decimal.Parse(versionNumber, CultureInfo.InvariantCulture);
                }
            }

            return tfPath;
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
                StringBuilder builder = new StringBuilder("/pattern:<fileSearch> [/ignoreFailures|i]");

                builder.AppendLine();
                builder.AppendLine();
                builder.AppendLine("/pattern:<fileSearch>\tThe file search pattern used to identify files to checkout from TFS.");
                builder.AppendLine("\t\t\tMust be an absolute path and may contain * wildcard characters.");
                builder.AppendLine("/ignoreFailures|/i\tIgnores TFS checkout failures and returns successfully.");

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
                return
                    "Checks out files from TFS based on a search pattern. The checkout will use default credentials resolved for the identified files.";
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
                           "TfsCheckout", "tfsedit"
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
    }
}