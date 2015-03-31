namespace Neovolve.BuildTaskExecutor.Extensibility
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The <see cref="ITask"/>
    ///   interface defines the methods for evaluating and executing a task.
    /// </summary>
    public interface ITask
    {
        /// <summary>
        /// Executes the task using the specified arguments.
        /// </summary>
        /// <param name="arguments">
        /// The arguments for the task.
        /// </param>
        /// <returns>
        /// <c>true</c> if the task executed successfully; otherwise, <c>false</c>.
        /// </returns>
        Boolean Execute(IEnumerable<String> arguments);

        /// <summary>
        /// Determines whether the specified arguments are valid.
        /// </summary>
        /// <param name="arguments">
        /// The arguments for the task.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified arguments are valid; otherwise, <c>false</c>.
        /// </returns>
        Boolean IsValidArgumentSet(IEnumerable<String> arguments);

        /// <summary>
        /// Gets the command line argument help.
        /// </summary>
        /// <value>
        /// The command line argument help.
        /// </value>
        String CommandLineArgumentHelp
        {
            get;
        }

        /// <summary>
        /// Gets the description of the task.
        /// </summary>
        /// <value>
        /// The task description.
        /// </value>
        String Description
        {
            get;
        }

        /// <summary>
        /// Gets the command line names for the task.
        /// </summary>
        /// <value>
        /// The command line names for the task.
        /// </value>
        /// <remarks>
        /// <note>
        /// The first name in the list is the name used when rendering generic task help information.
        ///   </note>
        /// </remarks>
        IEnumerable<String> Names
        {
            get;
        }
    }
}