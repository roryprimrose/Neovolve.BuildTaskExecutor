namespace Neovolve.BuildTaskExecutor.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Neovolve.BuildTaskExecutor.Extensibility;
    using Neovolve.BuildTaskExecutor.Properties;
    using Neovolve.BuildTaskExecutor.Services;

    /// <summary>
    /// The <see cref="WildcardFileSearchTask"/>
    ///   class is provides the common implementation for searching for files
    ///   using a wildcard search pattern.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The only wildcard character supported is the asterix (*) character.
    ///     The task will identify the base search directory as the last absolute directory in the search path that does not have
    ///     a wildcard character. All file paths after this point are evaluated against a regular expression built against the wildcard path value.
    ///   </para>
    /// <note>
    /// <b>The regular expression will match wildcard patterns across multiple directories.</b>
    ///     <para>
    /// The pattern C:\Temp\Some*\Path\output.txt will match all the following paths:
    ///       <list type="bullet">
    /// <item>
    /// C:\Temp\Some\Path\output.txt
    ///         </item>
    /// <item>
    /// C:\Temp\SomeCustom\Path\output.txt
    ///         </item>
    /// <item>
    /// C:\Temp\Some\NestedDirectory\Path\output.txt.
    ///         </item>
    /// </list>
    /// </para>
    /// <para>
    /// The wildcard character also matches zero characters as indicated in the above example.
    ///     </para>
    /// </note>
    /// </remarks>
    public abstract class WildcardFileSearchTask : ITask
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WildcardFileSearchTask"/> class.
        /// </summary>
        /// <param name="writer">
        /// The writer.
        /// </param>
        protected WildcardFileSearchTask(EventWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            Writer = writer;
        }

        /// <summary>
        /// Executes the task using the specified arguments.
        /// </summary>
        /// <param name="arguments">
        /// The arguments for the task.
        /// </param>
        /// <returns>
        /// <c>true</c> if the task executed successfully; otherwise, <c>false</c>.
        /// </returns>
        public virtual Boolean Execute(IEnumerable<String> arguments)
        {
            if (IsValidArgumentSet(arguments) == false)
            {
                return false;
            }

            OnBeforeSearch(arguments);

            String searchPattern = arguments.ParseArgument(PatternArgument);

            Boolean result = ExecuteSearch(searchPattern);

            OnAfterSearch(arguments);

            return result;
        }

        /// <summary>
        /// Determines whether the specified arguments are valid.
        /// </summary>
        /// <param name="arguments">
        /// The arguments for the task.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified arguments are valid; otherwise, <c>false</c>.
        /// </returns>
        public virtual Boolean IsValidArgumentSet(IEnumerable<String> arguments)
        {
            if (arguments.Any() == false)
            {
                return false;
            }

            String searchPattern = arguments.ParseArgument(PatternArgument);

            if (String.IsNullOrEmpty(searchPattern))
            {
                return false;
            }

            String baseAbsolutePath = ParseBaseAbsolutePath(searchPattern);

            if (String.IsNullOrEmpty(baseAbsolutePath))
            {
                return false;
            }

            if (Path.IsPathRooted(baseAbsolutePath) == false)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Builds the search expression.
        /// </summary>
        /// <param name="searchPattern">
        /// The search pattern.
        /// </param>
        /// <returns>
        /// A <see cref="Regex"/> instance.
        /// </returns>
        protected virtual Regex BuildSearchExpression(String searchPattern)
        {
            if (String.IsNullOrWhiteSpace(searchPattern))
            {
                throw new ArgumentNullException("searchPattern");
            }

            // The search pattern will be encoded for regex with wildcard (*) characters replaced with .?
            String[] parts = searchPattern.Split('*');
            String calculatedExpression = "^";

            for (Int32 index = 0; index < parts.Length; index++)
            {
                if (index > 0)
                {
                    calculatedExpression += ".*";
                }

                calculatedExpression += Regex.Escape(parts[index]);
            }

            calculatedExpression += "$";

            return new Regex(calculatedExpression, RegexOptions.Singleline);
        }

        /// <summary>
        /// Executes the search using the provided search pattern.
        /// </summary>
        /// <param name="searchPattern">
        /// The search pattern.
        /// </param>
        /// <returns>
        /// <c>true</c> if the search was successful; otherwise, <c>false</c>.
        /// </returns>
        protected virtual Boolean ExecuteSearch(String searchPattern)
        {
            String basePath = ParseBaseAbsolutePath(searchPattern);

            if (Directory.Exists(basePath))
            {
                Writer.WriteMessage(TraceEventType.Information, Resources.WildcardFileSearchTask_SearchingDirectory, basePath);

                Regex expression = BuildSearchExpression(searchPattern);

                return SearchDirectory(basePath, expression);
            }

            Writer.WriteMessage(TraceEventType.Warning, Resources.WildcardFileSearchTask_DirectoryNotFound, basePath);

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
        protected abstract FileMatchResult FileMatchFound(String filePath);

        /// <summary>
        /// Called after the search is completed.
        /// </summary>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        protected virtual void OnAfterSearch(IEnumerable<String> arguments)
        {
        }

        /// <summary>
        /// Called before the wildcard search is executed.
        /// </summary>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        protected virtual void OnBeforeSearch(IEnumerable<String> arguments)
        {
        }

        /// <summary>
        /// Parses the base absolute path.
        /// </summary>
        /// <param name="searchPattern">
        /// The search pattern.
        /// </param>
        /// <returns>
        /// A <see cref="String"/> instance.
        /// </returns>
        protected virtual String ParseBaseAbsolutePath(String searchPattern)
        {
            if (String.IsNullOrWhiteSpace(searchPattern))
            {
                return String.Empty;
            }

            String basePath = searchPattern;
            Int32 firstWildcard = basePath.IndexOf('*');

            if (firstWildcard > -1)
            {
                basePath = basePath.Substring(0, firstWildcard);
            }

            Int32 lastDirectory = basePath.LastIndexOf('\\');

            if (lastDirectory == basePath.Length - 1)
            {
                // The path already ends in a directory separator
                return basePath;
            }

            if (lastDirectory < 0)
            {
                // This is an invalid path because there are no directory separators before the wildcard
                // This means that it can't be an absolute path (drive wildcards are not supported)
                return null;
            }

            return basePath.Substring(0, lastDirectory + 1);
        }

        /// <summary>
        /// Searches the directory.
        /// </summary>
        /// <param name="directoryPath">
        /// The directory path.
        /// </param>
        /// <param name="expression">
        /// The expression.
        /// </param>
        /// <returns>
        /// <c>true</c> if the search was successful; otherwise, <c>false</c>.
        /// </returns>
        private Boolean SearchDirectory(String directoryPath, Regex expression)
        {
            IEnumerable<String> matchingFiles = Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories);

            foreach (String matchingFile in matchingFiles)
            {
                if (expression.IsMatch(matchingFile) == false)
                {
                    continue;
                }

                FileMatchResult result = FileMatchFound(matchingFile);

                if (result == FileMatchResult.Cancel)
                {
                    break;
                }

                if (result == FileMatchResult.FailTask)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the command line argument help.
        /// </summary>
        /// <value>
        /// The command line argument help.
        /// </value>
        public abstract String CommandLineArgumentHelp
        {
            get;
        }

        /// <summary>
        /// Gets the description of the task.
        /// </summary>
        /// <value>
        /// The task description.
        /// </value>
        public abstract String Description
        {
            get;
        }

        /// <summary>
        /// Gets the command line names for the task.
        /// </summary>
        /// <value>
        /// The command line names for the task.
        /// </value>
        public abstract IEnumerable<String> Names
        {
            get;
        }

        /// <summary>
        /// Gets the pattern argument.
        /// </summary>
        /// <value>
        /// The search pattern argument name.
        /// </value>
        protected abstract String PatternArgument
        {
            get;
        }

        /// <summary>
        /// Gets the event writer.
        /// </summary>
        /// <value>
        /// The event write.
        /// </value>
        protected EventWriter Writer
        {
            get;
            private set;
        }
    }
}