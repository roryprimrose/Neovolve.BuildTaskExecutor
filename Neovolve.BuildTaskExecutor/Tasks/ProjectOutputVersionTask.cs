namespace Neovolve.BuildTaskExecutor.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Xml;
    using Neovolve.BuildTaskExecutor.Properties;
    using Neovolve.BuildTaskExecutor.Services;

    /// <summary>
    /// The project output version task.
    /// </summary>
    internal abstract class ProjectOutputVersionTask : WildcardFileSearchTask
    {
        /// <summary>
        /// Defines the default format as '{ExistingFileName} {CurrentVersion}'.
        /// </summary>
        public const String DefaultnameFormat = "{0} {1}";

        /// <summary>
        /// Stores the expression used to extract a configuration name.
        /// </summary>
        private static readonly Regex _configurationExpression =
            new Regex("'\\$\\(Configuration\\)\\|\\$\\(Platform\\)' == '(?<configuration>.+)\\|.+'", RegexOptions.Singleline);

        /// <summary>
        /// Stores whether the operation is for a copy or a move.
        /// </summary>
        private Boolean _copyItems;

        /// <summary>
        /// Stores the rename format for the task.
        /// </summary>
        private String _nameFormat;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectOutputVersionTask"/> class.
        /// </summary>
        /// <param name="writer">
        /// The writer.
        /// </param>
        protected ProjectOutputVersionTask(EventWriter writer)
            : base(writer)
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
            XmlDocument doc = new XmlDocument();
            XmlNamespaceManager manager = new XmlNamespaceManager(doc.NameTable);

            manager.AddNamespace("x", "http://schemas.microsoft.com/developer/msbuild/2003");

            Writer.WriteMessage(TraceEventType.Verbose, Resources.ProjectOutputVersionTask_ProjectLoadNotification, filePath);

            doc.Load(filePath);

            String outputName = ParseOutputName(doc, manager);

            if (String.IsNullOrWhiteSpace(outputName))
            {
                Writer.WriteMessage(TraceEventType.Error, Resources.ProjectOutputVersionTask_OutputNameNotFound);

                return FileMatchResult.FailTask;
            }

            String projectPath = Path.GetDirectoryName(filePath);
            IEnumerable<String> targetDirectories = ParseTargetDirectories(doc, manager);
            Boolean filesChanged = false;

            foreach (String targetDirectory in targetDirectories)
            {
                String outputDirectory = Path.Combine(projectPath, targetDirectory);

                String outputFilePath = Path.Combine(outputDirectory, outputName);

                if (ProcessOutputDirectory(filePath, outputFilePath))
                {
                    filesChanged = true;
                }
            }

            if (filesChanged == false)
            {
                Writer.WriteMessage(TraceEventType.Error, Resources.ProjectOutputVersionTask_NoOutputFound);

                return FileMatchResult.FailTask;
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
            _nameFormat = GetNameFormat(arguments);
            _copyItems = arguments.ArgumentExists("/c");
        }

        /// <summary>
        /// Parses the output name of the project.
        /// </summary>
        /// <param name="projectXml">
        /// The project XML.
        /// </param>
        /// <param name="manager">
        /// The manager.
        /// </param>
        /// <returns>
        /// A <see cref="String"/> value.
        /// </returns>
        protected abstract String ParseOutputName(XmlDocument projectXml, XmlNamespaceManager manager);

        /// <summary>
        /// Parses the version.
        /// </summary>
        /// <param name="matchingFilePath">
        /// The matching file path.
        /// </param>
        /// <param name="outputFilePath">
        /// The output file path.
        /// </param>
        /// <returns>
        /// A <see cref="Version"/> instance.
        /// </returns>
        protected abstract Version ParseVersion(String matchingFilePath, String outputFilePath);

        /// <summary>
        /// Gets the name format.
        /// </summary>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <returns>
        /// A <see cref="String"/> instance.
        /// </returns>
        private static String GetNameFormat(IEnumerable<String> arguments)
        {
            String argument = arguments.ParseArgument("/format:", "/f:");

            if (String.IsNullOrWhiteSpace(argument))
            {
                return DefaultnameFormat;
            }

            return argument;
        }

        /// <summary>
        /// Parses the target directories.
        /// </summary>
        /// <param name="doc">
        /// The project document.
        /// </param>
        /// <param name="manager">
        /// The manager.
        /// </param>
        /// <returns>
        /// A <see cref="IEnumerable&lt;T&gt;"/> instance.
        /// </returns>
        private static IEnumerable<String> ParseTargetDirectories(XmlDocument doc, XmlNamespaceManager manager)
        {
            if (doc == null)
            {
                throw new ArgumentNullException("doc");
            }

            XmlNodeList configurations = doc.SelectNodes("//x:Project/x:PropertyGroup[@Condition != '']", manager);

            if (configurations == null)
            {
                throw new ArgumentException(Resources.ProjectOutputVersionTask_NoBuildConfigurationsFound, "doc");
            }

            foreach (XmlNode configuration in configurations)
            {
                String targetDirectory = ParseTargetDirectory(configuration, manager);

                if (String.IsNullOrWhiteSpace(targetDirectory))
                {
                    continue;
                }

                yield return targetDirectory;
            }

            yield break;
        }

        /// <summary>
        /// Parses the target directory.
        /// </summary>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <param name="manager">
        /// The manager.
        /// </param>
        /// <returns>
        /// A <see cref="String"/> instance.
        /// </returns>
        private static String ParseTargetDirectory(XmlNode configuration, XmlNamespaceManager manager)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            String outputPath = configuration.SelectSingleNode("./x:OutputPath", manager).InnerText;
            const String ConfigurationBuildVariable = "$(Configuration)";

            if (outputPath.Contains(ConfigurationBuildVariable))
            {
                String conditionValue = configuration.Attributes["Condition"].Value;
                Match configurationMatch = _configurationExpression.Match(conditionValue);

                if (configurationMatch.Success == false)
                {
                    return String.Empty;
                }

                String configurationName = configurationMatch.Groups["configuration"].Value;

                outputPath = outputPath.Replace(ConfigurationBuildVariable, configurationName);
            }

            return outputPath;
        }

        /// <summary>
        /// Processes the output directory.
        /// </summary>
        /// <param name="matchingFilePath">
        /// The matching file path.
        /// </param>
        /// <param name="outputFilePath">
        /// The output file path.
        /// </param>
        /// <returns>
        /// A <see cref="Boolean"/> instance.
        /// </returns>
        private Boolean ProcessOutputDirectory(String matchingFilePath, String outputFilePath)
        {
            String outputDirectory = Path.GetDirectoryName(outputFilePath);

            if (Directory.Exists(outputDirectory) == false)
            {
                Writer.WriteMessage(TraceEventType.Verbose, Resources.ProjectOutputVersionTask_SkippingDirectoryNotification, outputDirectory);

                return false;
            }

            if (File.Exists(outputFilePath) == false)
            {
                Writer.WriteMessage(TraceEventType.Verbose, Resources.ProjectOutputVersionTask_SkippingFileNotification, outputFilePath);

                return false;
            }

            String outputName = Path.GetFileNameWithoutExtension(outputFilePath);
            Version configuredVersion = ParseVersion(matchingFilePath, outputFilePath);

            if (configuredVersion == null)
            {
                Writer.WriteMessage(TraceEventType.Error, Resources.VersionAction_VersionNotFound, matchingFilePath);

                return false;
            }

            String versionText = configuredVersion.GenerateVersionString(false);
            IEnumerable<String> matchingFiles = Directory.EnumerateFiles(outputDirectory, outputName + ".*", SearchOption.AllDirectories);
            Boolean fileProcessed = false;

            foreach (String matchingFile in matchingFiles)
            {
                String currentFileName = Path.GetFileNameWithoutExtension(matchingFile);
                String currentExtension = Path.GetExtension(matchingFile);
                String newFileName = String.Format(CultureInfo.InvariantCulture, _nameFormat, currentFileName, versionText) + currentExtension;
                String newFilePath = Path.Combine(outputDirectory, newFileName);

                if (_copyItems)
                {
                    Writer.WriteMessage(
                        TraceEventType.Information, Resources.ProjectOutputVersionTask_FileCopyNotification, matchingFile, newFilePath);

                    File.Copy(matchingFile, newFilePath, true);
                }
                else
                {
                    if (File.Exists(newFilePath))
                    {
                        File.Delete(newFilePath);
                    }

                    Writer.WriteMessage(
                        TraceEventType.Information, Resources.ProjectOutputVersionTask_FileMoveNotification, matchingFile, newFilePath);

                    File.Move(matchingFile, newFilePath);
                }

                fileProcessed = true;
            }

            return fileProcessed;
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