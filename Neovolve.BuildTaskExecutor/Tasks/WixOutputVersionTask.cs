namespace Neovolve.BuildTaskExecutor.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.Text;
    using System.Xml;
    using Neovolve.BuildTaskExecutor.Extensibility;
    using Neovolve.BuildTaskExecutor.Properties;
    using Neovolve.BuildTaskExecutor.Services;

    /// <summary>
    /// The <see cref="WixOutputVersionTask"/>
    ///   class is used to rename the output of a wix project using a version number.
    /// </summary>
    [Export(typeof(ITask))]
    internal class WixOutputVersionTask : ProjectOutputVersionTask
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WixOutputVersionTask"/> class.
        /// </summary>
        /// <param name="writer">
        /// The writer.
        /// </param>
        /// <param name="versionAction">
        /// The version action.
        /// </param>
        [ImportingConstructor]
        public WixOutputVersionTask(EventWriter writer, [Import(VersionManagerExport.Wix)] IVersionManager versionAction)
            : base(writer)
        {
            if (versionAction == null)
            {
                throw new ArgumentNullException("versionAction");
            }

            VersionAction = versionAction;
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
        protected override String ParseOutputName(XmlDocument projectXml, XmlNamespaceManager manager)
        {
            XmlElement outputNameNode = projectXml.SelectSingleNode("//x:Project/x:PropertyGroup/x:OutputName", manager) as XmlElement;

            if (outputNameNode == null)
            {
                Writer.WriteMessage(TraceEventType.Verbose, Resources.WixOutputVersionTask_NoOutputNameNode);

                return null;
            }

            String outputName = outputNameNode.InnerText;

            if (String.IsNullOrWhiteSpace(outputName))
            {
                Writer.WriteMessage(TraceEventType.Verbose, Resources.WixOutputVersionTask_NoOutputName);

                return null;
            }

            return outputName + ".msi";
        }

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
        protected override Version ParseVersion(String matchingFilePath, String outputFilePath)
        {
            return VersionAction.ReadVersion(matchingFilePath);
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
                StringBuilder builder = new StringBuilder("/pattern:<fileSearch> [/format|f:<format>] [/c]");

                builder.AppendLine();
                builder.AppendLine();
                builder.AppendLine("/pattern:<fileSearch>\tThe file search pattern used to identify wix project files to process.");
                builder.AppendLine("\t\t\tMust be an absolute path and may contain * wildcard characters.");
                builder.AppendLine("/format|/f\t\tThe format for updating the output file name. Defaults to '{0} {1}'");
                builder.AppendLine("\t\t\tFormat mask {0} is the original file name.");
                builder.AppendLine("\t\t\tFormat mask {1} is the product version.");
                builder.AppendLine("/c\t\t\tCopies the output files to the new name. Default action is a move.");

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
                return "Renames the Wix project output to include the wix product version.";
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
                           "WixOutputVersion", "wov"
                       };
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