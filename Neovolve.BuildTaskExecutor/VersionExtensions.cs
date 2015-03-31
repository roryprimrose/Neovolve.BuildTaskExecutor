namespace Neovolve.BuildTaskExecutor
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The <see cref="VersionExtensions"/>
    ///   class is used to provide extension methods for version management.
    /// </summary>
    public static class VersionExtensions
    {
        /// <summary>
        /// The expression for parsing out the version parts.
        /// </summary>
        private static readonly Regex _versionExpression =
            new Regex("^(?<major>\\d+)\\.(?<minor>\\d+)(\\.(?<build>(\\d+|\\*)))?(\\.(?<revision>(\\d+|\\*)))?", RegexOptions.Singleline);

        /// <summary>
        /// Creates the version using the specified parts.
        /// </summary>
        /// <param name="major">
        /// The major.
        /// </param>
        /// <param name="minor">
        /// The minor.
        /// </param>
        /// <param name="build">
        /// The build.
        /// </param>
        /// <param name="revision">
        /// The revision.
        /// </param>
        /// <returns>
        /// A <see cref="Version"/> instance.
        /// </returns>
        public static Version Create(Int32 major, Int32 minor, Int32 build, Int32 revision)
        {
            if (revision >= 0)
            {
                return new Version(major, minor, build, revision);
            }

            if (build >= 0)
            {
                return new Version(major, minor, build);
            }

            return new Version(major, minor);
        }

        /// <summary>
        /// Generates the version string using the specified version information.
        /// </summary>
        /// <param name="version">
        /// The version.
        /// </param>
        /// <remarks>
        /// Parts in the version that have a value less than zero will not be added to the result.
        /// </remarks>
        /// <returns>
        /// A <see cref="String"/> instance.
        /// </returns>
        public static String GenerateVersionString(this Version version)
        {
            return GenerateVersionString(version, true);
        }

        /// <summary>
        /// Generates the version string using the specified version information.
        /// </summary>
        /// <param name="version">
        /// The version.
        /// </param>
        /// <param name="includeWildcards">
        /// If set to <c>true</c>, the first negative version part will be rendered with an asterix (*) wildcard character.
        /// </param>
        /// <returns>
        /// A <see cref="String"/> instance.
        /// </returns>
        public static String GenerateVersionString(this Version version, Boolean includeWildcards)
        {
            if (version == null)
            {
                return String.Empty;
            }

            if (version.Build < 0)
            {
                if (includeWildcards)
                {
                    return String.Concat(version.Major, ".", version.Minor, ".*");
                }

                return String.Concat(version.Major, ".", version.Minor);
            }

            if (version.Revision < 0)
            {
                if (includeWildcards)
                {
                    return String.Concat(version.Major, ".", version.Minor, ".", version.Build, ".*");
                }

                return String.Concat(version.Major, ".", version.Minor, ".", version.Build);
            }

            return String.Concat(version.Major, ".", version.Minor, ".", version.Build, ".", version.Revision);
        }

        /// <summary>
        /// Converts the specified value to a <see cref="Version"/> instance.
        /// </summary>
        /// <param name="versionText">
        /// The text containing a version value (in the format \d.\d[.\d.\d]).
        /// </param>
        /// <returns>
        /// A <see cref="Version"/> instance if the string contains a version value; otherwise, <c>null</c>.
        /// </returns>
        public static Version ToVersion(this String versionText)
        {
            Match match = _versionExpression.Match(versionText);

            if (match.Success == false)
            {
                return null;
            }

            String majorPart = match.Groups["major"].Value;
            Int32 major = ParseVersionPart(majorPart);
            String minorPart = match.Groups["minor"].Value;
            Int32 minor = ParseVersionPart(minorPart);
            String buildPart = "-1";

            if (match.Groups.Count > 2)
            {
                buildPart = match.Groups["build"].Value;
            }

            Int32 build = ParseVersionPart(buildPart);
            String revisionPart = "-1";

            if (match.Groups.Count > 3)
            {
                revisionPart = match.Groups["revision"].Value;
            }

            Int32 revision = ParseVersionPart(revisionPart);

            return Create(major, minor, build, revision);
        }

        /// <summary>
        /// Increments the version number.
        /// </summary>
        /// <param name="versionNumber">
        /// The version number.
        /// </param>
        /// <param name="settings">
        /// The settings.
        /// </param>
        /// <returns>
        /// A <see cref="String"/> instance.
        /// </returns>
        internal static Version IncrementVersionNumber(this Version versionNumber, VersionApplySettings settings)
        {
            Int32 major = versionNumber.Major;

            if (settings.ApplyMajor)
            {
                major++;
            }

            Int32 minor = versionNumber.Minor;

            if (settings.ApplyMinor)
            {
                minor++;
            }

            Int32 build = versionNumber.Build;

            if (settings.ApplyBuild)
            {
                build++;
            }

            Int32 revision = versionNumber.Revision;

            if (settings.ApplyRevision)
            {
                revision++;
            }

            return Create(major, minor, build, revision);
        }

        /// <summary>
        /// Parses the version part.
        /// </summary>
        /// <param name="part">
        /// The version part.
        /// </param>
        /// <returns>
        /// A <see cref="Int32"/> instance.
        /// </returns>
        private static Int32 ParseVersionPart(String part)
        {
            if (String.IsNullOrWhiteSpace(part))
            {
                return -1;
            }

            if (part == "*")
            {
                return -1;
            }

            return Int32.Parse(part, CultureInfo.InvariantCulture);
        }
    }
}