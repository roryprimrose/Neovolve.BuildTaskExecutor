namespace Neovolve.BuildTaskExecutor
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The <see cref="ArgumentExtensions"/>
    ///   class is used to provide extension methods for dealing with command line arguments.
    /// </summary>
    public static class ArgumentExtensions
    {
        /// <summary>
        /// Determines whether an argument exists that matches one of the specified values.
        /// </summary>
        /// <param name="arguments">
        /// The arguments to search.
        /// </param>
        /// <param name="matchValues">
        /// The set of values to match.
        /// </param>
        /// <returns>
        /// <c>true</c> if there is an argument that matches one of the specified values; otherwise, <c>false</c>.
        /// </returns>
        public static Boolean ArgumentExists(this IEnumerable<String> arguments, params String[] matchValues)
        {
            return ArgumentExists(arguments, StringComparison.OrdinalIgnoreCase, matchValues);
        }

        /// <summary>
        /// Determines whether an argument exists that matches one of the specified values.
        /// </summary>
        /// <param name="arguments">
        /// The arguments to search.
        /// </param>
        /// <param name="stringComparison">
        /// The string comparison to apply to the value match.
        /// </param>
        /// <param name="matchValues">
        /// The set of values to match.
        /// </param>
        /// <returns>
        /// <c>true</c> if there is an argument that matches one of the specified values; otherwise, <c>false</c>.
        /// </returns>
        public static Boolean ArgumentExists(this IEnumerable<String> arguments, StringComparison stringComparison, params String[] matchValues)
        {
            if (arguments == null)
            {
                return false;
            }

            if (matchValues == null)
            {
                return false;
            }

            foreach (String prefix in matchValues)
            {
                foreach (String argument in arguments)
                {
                    if (argument.Equals(prefix, stringComparison))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Parses the argument from the set where there is a match on a prefix value.
        /// </summary>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <param name="prefixes">
        /// The prefix values.
        /// </param>
        /// <returns>
        /// The argument value without the matching prefix, otherwise <c>null</c> if no match is found.
        /// </returns>
        public static String ParseArgument(this IEnumerable<String> arguments, params String[] prefixes)
        {
            return ParseArgument(arguments, StringComparison.OrdinalIgnoreCase, prefixes);
        }

        /// <summary>
        /// Parses the argument from the set where there is a match on a prefix value.
        /// </summary>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <param name="stringComparison">
        /// The string comparison to apply to the value match.
        /// </param>
        /// <param name="prefixes">
        /// The prefix values.
        /// </param>
        /// <returns>
        /// The argument value without the matching prefix, otherwise <c>null</c> if no match is found.
        /// </returns>
        public static String ParseArgument(this IEnumerable<String> arguments, StringComparison stringComparison, params String[] prefixes)
        {
            if (arguments == null)
            {
                return null;
            }

            if (prefixes == null)
            {
                return null;
            }

            foreach (String prefix in prefixes)
            {
                foreach (String argument in arguments)
                {
                    if (argument.StartsWith(prefix, stringComparison))
                    {
                        return argument.Substring(prefix.Length);
                    }
                }
            }

            return null;
        }
    }
}