namespace Neovolve.BuildTaskExecutor.Services
{
    using System;
    using Neovolve.BuildTaskExecutor.Extensibility;

    /// <summary>
    /// The <see cref="VersionManagerExport"/>
    ///   class is defines the inbuilt MEF export names for the <see cref="IVersionManager"/> interface.
    /// </summary>
    public static class VersionManagerExport
    {
        /// <summary>
        /// Defines the export name for the binary file version manager.
        /// </summary>
        public const String Binary = "BinaryVersionManager";

        /// <summary>
        /// Defines the export name for the C# version manager.
        /// </summary>
        public const String CSharp = "CSharpVersionManager";

        /// <summary>
        /// Defines the export name for the Wix version manager.
        /// </summary>
        public const String Wix = "WixVersionManager";
    }
}