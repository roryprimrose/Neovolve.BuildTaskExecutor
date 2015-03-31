namespace Neovolve.BuildTaskExecutor.Extensibility
{
    using System;

    /// <summary>
    /// The <see cref="IDataManager"/>
    ///   interface is used to define the methods available for reading and writing information from and to paths.
    /// </summary>
    public interface IDataManager
    {
        /// <summary>
        /// Reads the text from the specified path.
        /// </summary>
        /// <param name="path">
        /// The path to read from.
        /// </param>
        /// <returns>
        /// A <see cref="String"/> instance.
        /// </returns>
        String ReadText(String path);

        /// <summary>
        /// Writes the text to the specified path.
        /// </summary>
        /// <param name="path">
        /// The path to write to.
        /// </param>
        /// <param name="contents">
        /// The contents.
        /// </param>
        void WriteText(String path, String contents);
    }
}