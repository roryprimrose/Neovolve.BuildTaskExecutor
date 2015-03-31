namespace Neovolve.BuildTaskExecutor.Services
{
    using System;
    using System.ComponentModel.Composition;
    using System.Text.RegularExpressions;
    using Neovolve.BuildTaskExecutor.Extensibility;

    /// <summary>
    /// The version manager.
    /// </summary>
    [Export(VersionManagerExport.CSharp, typeof(IVersionManager))]
    internal class CSharpVersionManager : IVersionManager
    {
        /// <summary>
        /// The expression for parsing out the assembly version.
        /// </summary>
        private static readonly Regex _assemblyVersionExpression =
            new Regex(
                "(?<=^\\s*\\[assembly:\\s*AssemblyVersion\\(\")(?<major>\\d+)\\.(?<minor>\\d+)(\\.(?<build>(\\d+|\\*)))?(\\.(?<revision>(\\d+|\\*)))?(?=\"\\)\\])", 
                RegexOptions.Multiline);

        /// <summary>
        /// Initializes a new instance of the <see cref="CSharpVersionManager"/> class.
        /// </summary>
        /// <param name="dataManager">
        /// The data manager.
        /// </param>
        [ImportingConstructor]
        public CSharpVersionManager(IDataManager dataManager)
        {
            if (dataManager == null)
            {
                throw new ArgumentNullException("dataManager");
            }

            DataManager = dataManager;
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
            String productInfoContents = DataManager.ReadText(filePath);
            Match match = _assemblyVersionExpression.Match(productInfoContents);

            if (match.Success)
            {
                return match.Value.ToVersion();
            }

            return null;
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
            String productInfoContents = DataManager.ReadText(filePath);
            String versionNumber = newVersion.GenerateVersionString();

            productInfoContents = _assemblyVersionExpression.Replace(productInfoContents, versionNumber);

            DataManager.WriteText(filePath, productInfoContents);
        }

        /// <summary>
        /// Gets or sets the data manager.
        /// </summary>
        /// <value>
        /// The data manager.
        /// </value>
        private IDataManager DataManager
        {
            get;
            set;
        }
    }
}