namespace Neovolve.BuildTaskExecutor.Services
{
    using System;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.IO;
    using Neovolve.BuildTaskExecutor.Extensibility;

    /// <summary>
    /// The <see cref="BinaryVersionManager"/>
    ///   class is used to read version information from a binary file.
    /// </summary>
    [Export(VersionManagerExport.Binary, typeof(IVersionManager))]
    internal class BinaryVersionManager : IVersionManager
    {
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
            if (File.Exists(filePath) == false)
            {
                return null;
            }

            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(filePath);

            return VersionExtensions.Create(
                versionInfo.ProductMajorPart, versionInfo.ProductMinorPart, versionInfo.ProductBuildPart, versionInfo.ProductPrivatePart);
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
        /// <exception cref="NotSupportedException">
        /// Writing version information to a binary file is not supported.
        /// </exception>
        public void WriteVersion(String filePath, Version newVersion)
        {
            throw new NotSupportedException();
        }
    }
}