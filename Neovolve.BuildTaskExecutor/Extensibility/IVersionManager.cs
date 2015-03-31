namespace Neovolve.BuildTaskExecutor.Extensibility
{
    using System;

    /// <summary>
    /// The <see cref="IVersionManager"/>
    ///   interface defines the operations for reading and writing file version information.
    /// </summary>
    public interface IVersionManager
    {
        /// <summary>
        /// Reads the version from the specified file path.
        /// </summary>
        /// <param name="filePath">
        /// The file path.
        /// </param>
        /// <returns>
        /// A <see cref="Version"/> instance, or <c>null</c> if no version information could be determined.
        /// </returns>
        Version ReadVersion(String filePath);

        /// <summary>
        /// Writes the version to the specified file path.
        /// </summary>
        /// <param name="filePath">
        /// The file path.
        /// </param>
        /// <param name="newVersion">
        /// The new version.
        /// </param>
        void WriteVersion(String filePath, Version newVersion);
    }
}