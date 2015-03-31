namespace Neovolve.BuildTaskExecutor.Services
{
    using System;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Xml;
    using Neovolve.BuildTaskExecutor.Extensibility;
    using Neovolve.BuildTaskExecutor.Properties;

    /// <summary>
    /// The <see cref="WixVersionManager"/>
    ///   class is used to provide parsing operations for Wix projects.
    /// </summary>
    [Export(VersionManagerExport.Wix, typeof(IVersionManager))]
    internal class WixVersionManager : IVersionManager
    {
        /// <summary>
        /// Stores the expression used to check for project variable names.
        /// </summary>
        private static readonly Regex _projectVariableExpression = new Regex("\\$\\(var\\.(?<VariableName>.+)\\)", RegexOptions.Singleline);

        /// <summary>
        /// Stores the expression for parsing out version numbers.
        /// </summary>
        private static readonly Regex _versionExpression = new Regex("\\d+(\\.\\d+){0,3}", RegexOptions.Singleline);

        /// <summary>
        /// Initializes a new instance of the <see cref="WixVersionManager"/> class.
        /// </summary>
        /// <param name="eventWriter">
        /// The event writers.
        /// </param>
        [ImportingConstructor]
        public WixVersionManager(EventWriter eventWriter)
        {
            if (eventWriter == null)
            {
                throw new ArgumentNullException("eventWriter");
            }

            Writer = eventWriter;
        }

        /// <summary>
        /// Reads the version for the specified path.
        /// </summary>
        /// <param name="filePath">
        /// The file path.
        /// </param>
        /// <returns>
        /// A <see cref="Version"/> instance.
        /// </returns>
        public Version ReadVersion(String filePath)
        {
            XmlNode versionNode = SearchProject(filePath, null);

            if (versionNode == null)
            {
                Writer.WriteMessage(TraceEventType.Verbose, Resources.WixVersionManager_ProductVersionNotFound);

                return null;
            }

            if (versionNode is XmlProcessingInstruction)
            {
                // Parse out the version number
                Match versionMatch = _versionExpression.Match(versionNode.Value);

                if (versionMatch.Success == false)
                {
                    Writer.WriteMessage(TraceEventType.Verbose, Resources.WixVersionManager_InvalidVersionValue, versionNode.Value);

                    return null;
                }

                return versionMatch.Value.ToVersion();
            }

            return versionNode.InnerText.ToVersion();
        }

        /// <summary>
        /// Writes the version to the specified file.
        /// </summary>
        /// <param name="filePath">
        /// The file path.
        /// </param>
        /// <param name="newVersion">
        /// The new version.
        /// </param>
        public void WriteVersion(String filePath, Version newVersion)
        {
            XmlNode versionNode = SearchProject(filePath, x => UpdateProductVersion(x, newVersion));

            if (versionNode == null)
            {
                Writer.WriteMessage(TraceEventType.Verbose, Resources.WixVersionManager_ProductVersionNotFound);
            }
        }

        /// <summary>
        /// Searches the document.
        /// </summary>
        /// <param name="absoluteIncludePath">
        /// The absolute include path.
        /// </param>
        /// <param name="xpath">
        /// The xpath query.
        /// </param>
        /// <returns>
        /// A <see cref="XmlNode"/> instance.
        /// </returns>
        private static XmlNode SearchDocument(String absoluteIncludePath, String xpath)
        {
            XmlDocument doc = new XmlDocument
                              {
                                  PreserveWhitespace = true
                              };

            try
            {
                doc.Load(absoluteIncludePath);
            }
            catch (XmlException)
            {
                // The file is not an XML document
                return null;
            }

            XmlNamespaceManager manager = new XmlNamespaceManager(doc.NameTable);

            manager.AddNamespace("x", "http://schemas.microsoft.com/wix/2006/wi");

            XmlNode versionNode = doc.SelectSingleNode(xpath, manager);

            if (versionNode == null)
            {
                return null;
            }

            return versionNode;
        }

        /// <summary>
        /// Updates the product version.
        /// </summary>
        /// <param name="versionNode">
        /// The version node.
        /// </param>
        /// <param name="newVersion">
        /// The new version.
        /// </param>
        private static void UpdateProductVersion(XmlNode versionNode, Version newVersion)
        {
            String newVersionText = newVersion.GenerateVersionString(false);

            if (versionNode is XmlProcessingInstruction)
            {
                // Parse out the version number
                Match versionMatch = _versionExpression.Match(versionNode.Value);

                if (versionMatch.Success == false)
                {
                    throw new InvalidOperationException("The wix product variable '" + versionNode.Value + "' does not contain a version number.");
                }

                versionNode.Value = _versionExpression.Replace(versionNode.Value, newVersionText);
            }
            else
            {
                versionNode.InnerText = newVersionText;
            }
        }

        /// <summary>
        /// Searches the project.
        /// </summary>
        /// <param name="projectPath">
        /// The project path.
        /// </param>
        /// <param name="modifyVersion">
        /// The modify version.
        /// </param>
        /// <returns>
        /// A <see cref="XmlNode"/> instance.
        /// </returns>
        private XmlNode SearchProject(String projectPath, Action<XmlNode> modifyVersion)
        {
            XmlDocument doc = new XmlDocument();
     
            try
            {
                doc.Load(projectPath);
            }
            catch (XmlException)
            {
                // Not a valid XML document
                return null;
            }

            XmlNamespaceManager manager = new XmlNamespaceManager(doc.NameTable);

            manager.AddNamespace("x", "http://schemas.microsoft.com/developer/msbuild/2003");

            XmlNode versionNode = SearchProject(projectPath, doc, manager, modifyVersion, "//x:Wix/x:Product/@Version");

            if (versionNode == null)
            {
                // No version node was found in any of the WiX project documents
                return null;
            }

            Match projectVariableMatch = _projectVariableExpression.Match(versionNode.InnerText);

            if (projectVariableMatch.Success)
            {
                String variableName = projectVariableMatch.Groups["VariableName"].Value;

                // This is a wix project variable
                // We need to run the project search again looking for an XmlProcessingInstruction node that has the right contents
                versionNode = SearchProject(
                    projectPath, doc, manager, modifyVersion, "*/processing-instruction(\"define\")[contains(., '" + variableName + "')]");
            }

            return versionNode;
        }

        /// <summary>
        /// Searches the project.
        /// </summary>
        /// <param name="projectPath">
        /// The project path.
        /// </param>
        /// <param name="doc">
        /// The project document.
        /// </param>
        /// <param name="manager">
        /// The manager.
        /// </param>
        /// <param name="modifyVersion">
        /// The modify version.
        /// </param>
        /// <param name="xpath">
        /// The xpath query.
        /// </param>
        /// <returns>
        /// A <see cref="XmlNode"/> instance.
        /// </returns>
        private XmlNode SearchProject(String projectPath, XmlDocument doc, XmlNamespaceManager manager, Action<XmlNode> modifyVersion, String xpath)
        {
            XmlNodeList includeNodes = doc.SelectNodes("(//x:Project/x:ItemGroup/x:Compile|//x:Project/x:ItemGroup/x:Content)/@Include", manager);

            if (includeNodes == null)
            {
                Writer.WriteMessage(TraceEventType.Verbose, Resources.WixVersionManager_NoIncludeFilesFound);

                return null;
            }

            String projectDirectory = Path.GetDirectoryName(projectPath);

            foreach (XmlNode includeNode in includeNodes)
            {
                String relativeIncludePath = includeNode.Value;
                String absoluteIncludePath = Path.Combine(projectDirectory, relativeIncludePath);

                if (File.Exists(absoluteIncludePath) == false)
                {
                    Writer.WriteMessage(TraceEventType.Verbose, Resources.WixVersionManager_InvalidIncludeFile, relativeIncludePath);

                    return null;
                }

                XmlNode versionNode = SearchDocument(absoluteIncludePath, xpath);

                if (versionNode == null)
                {
                    continue;
                }

                Match projectVariableMatch = _projectVariableExpression.Match(versionNode.InnerText);

                if (projectVariableMatch.Success)
                {
                    // We need to search for the project variable across the project
                    // Return this node so we know what we are looking for
                    return versionNode;
                }

                if (modifyVersion != null)
                {
                    modifyVersion(versionNode);

                    if (versionNode.OwnerDocument != null)
                    {
                        versionNode.OwnerDocument.Save(absoluteIncludePath);
                    }
                }

                return versionNode;
            }

            return null;
        }

        /// <summary>
        /// Gets or sets the writer.
        /// </summary>
        /// <value>
        /// The writer.
        /// </value>
        private EventWriter Writer
        {
            get;
            set;
        }
    }
}