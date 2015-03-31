namespace Neovolve.BuildTaskExecutor.Services
{
    using System;
    using System.ComponentModel.Composition;
    using System.IO;
    using System.Text;
    using Neovolve.BuildTaskExecutor.Extensibility;

    /// <summary>
    /// The <see cref="FileDataManager"/>
    ///   class is used to read data from a file.
    /// </summary>
    [Export(typeof(IDataManager))]
    internal class FileDataManager : IDataManager
    {
        /// <summary>
        /// Reads the text from the specified path.
        /// </summary>
        /// <param name="path">
        /// The path to read.
        /// </param>
        /// <returns>
        /// A <see cref="String"/> instance.
        /// </returns>
        public String ReadText(String path)
        {
            using (StreamReader reader = new StreamReader(path, true))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Writes the text to the specified path.
        /// </summary>
        /// <param name="path">
        /// The path to write to.
        /// </param>
        /// <param name="contents">
        /// The contents.
        /// </param>
        public void WriteText(String path, String contents)
        {
            Encoding encoding;

            using (StreamReader reader = new StreamReader(path, true))
            {
                encoding = reader.CurrentEncoding;
            }

            using (StreamWriter writer = new StreamWriter(path, false, encoding))
            {
                writer.Write(contents);
            }
        }
    }
}