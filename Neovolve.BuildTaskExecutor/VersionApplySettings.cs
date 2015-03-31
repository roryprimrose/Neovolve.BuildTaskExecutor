namespace Neovolve.BuildTaskExecutor
{
    using System;

    /// <summary>
    /// The <see cref="VersionApplySettings"/>
    ///   class is used to describe the settings for applying version numbers.
    /// </summary>
    internal class VersionApplySettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VersionApplySettings"/> class.
        /// </summary>
        /// <param name="applyMajor">
        /// If set to <c>true</c> [apply major].
        /// </param>
        /// <param name="applyMinor">
        /// If set to <c>true</c> [apply minor].
        /// </param>
        /// <param name="applyBuild">
        /// If set to <c>true</c> [apply build].
        /// </param>
        /// <param name="applyRevision">
        /// If set to <c>true</c> [apply revision].
        /// </param>
        public VersionApplySettings(Boolean applyMajor, Boolean applyMinor, Boolean applyBuild, Boolean applyRevision)
        {
            ApplyBuild = applyBuild;
            ApplyMajor = applyMajor;
            ApplyMinor = applyMinor;
            ApplyRevision = applyRevision;
        }

        /// <summary>
        /// Gets a value indicating whether [apply build].
        /// </summary>
        /// <value>
        /// <c>true</c> if [apply build]; otherwise, <c>false</c>.
        /// </value>
        public Boolean ApplyBuild
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether [apply major].
        /// </summary>
        /// <value>
        /// <c>true</c> if [apply major]; otherwise, <c>false</c>.
        /// </value>
        public Boolean ApplyMajor
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether [apply minor].
        /// </summary>
        /// <value>
        /// <c>true</c> if [apply minor]; otherwise, <c>false</c>.
        /// </value>
        public Boolean ApplyMinor
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether [apply revision].
        /// </summary>
        /// <value>
        /// <c>true</c> if [apply revision]; otherwise, <c>false</c>.
        /// </value>
        public Boolean ApplyRevision
        {
            get;
            private set;
        }
    }
}