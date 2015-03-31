namespace Neovolve.BuildTaskExecutor
{
    using System;
    using System.Linq;
    using Neovolve.BuildTaskExecutor.Extensibility;

    /// <summary>
    /// The <see cref="TaskExtensions"/>
    ///   class is used to provide extension methods for the <see cref="ITask"/> interface.
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Gets the task display names.
        /// </summary>
        /// <param name="task">
        /// The task to get the names of.
        /// </param>
        /// <returns>
        /// A <see cref="String"/> instance.
        /// </returns>
        public static String GetTaskDisplayNames(this ITask task)
        {
            if (task == null)
            {
                return String.Empty;
            }

            String taskNames = task.Names.First();

            task.Names.Skip(1).ToList().ForEach(x => taskNames = taskNames + "|" + x);

            return taskNames;
        }
    }
}