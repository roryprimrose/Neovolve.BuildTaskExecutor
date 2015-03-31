namespace Neovolve.BuildTaskExecutor.Services
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.Linq;
    using Neovolve.BuildTaskExecutor.Extensibility;
    using Neovolve.BuildTaskExecutor.Properties;

    /// <summary>
    /// The <see cref="TaskResolver"/>
    ///   class provides the ability to resolve a task from a task name.
    /// </summary>
    /// <remarks>
    /// <note>
    /// <para>
    /// <b>Do not import this type into an <see cref="ITask"/> constructor decorated with the <see cref="ImportingConstructorAttribute"/>.</b>
    ///     </para>
    /// <para>
    /// This action will cause a circular reference between the <see cref="TaskResolver"/> and the <see cref="ITask"/> instance trying to import it.
    ///       MEF will throw a composition error in this scenario. The way to achieve this result is to import the <see cref="TaskResolver"/>
    ///       as a property on the <see cref="ITask"/> class.
    ///     </para>
    /// </note>
    /// </remarks>
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TaskResolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskResolver"/> class.
        /// </summary>
        /// <param name="writer">
        /// The writer.
        /// </param>
        [ImportingConstructor]
        public TaskResolver(EventWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            Writer = writer;
        }

        /// <summary>
        /// The resolve task.
        /// </summary>
        /// <param name="taskName">
        /// The task name.
        /// </param>
        /// <returns>
        /// A <see cref="ITask"/> instance if a task can be resolved by name; otherwise, <c>null</c>.
        /// </returns>
        public ITask ResolveTask(String taskName)
        {
            Writer.WriteMessage(TraceEventType.Verbose, Resources.TaskResolver_TaskResolutionAttempt, taskName);

            IEnumerable<ITask> matchingTasks = from x in Tasks
                                               where x.Names.Contains(taskName, StringComparer.OrdinalIgnoreCase)
                                               select x;

            if (matchingTasks.Any() == false)
            {
                Writer.WriteMessage(TraceEventType.Error, Resources.TaskResolver_TaskNotFound, taskName);

                return null;
            }

            if (matchingTasks.Count() > 1)
            {
                // There are multiple tasks that match
                Writer.WriteMessage(TraceEventType.Warning, Resources.TaskResolver_MultipleTasksFound, taskName);
                Writer.WriteMessage(TraceEventType.Information, Resources.TaskResolver_MatchingTasksHeader, taskName);
                matchingTasks.ToList().ForEach(x => Writer.WriteMessage(TraceEventType.Information, "\t" + x.GetTaskDisplayNames()));

                return null;
            }

            // We have an exact task match
            ITask task = matchingTasks.First();
            String firstTaskName = task.Names.First();

            Writer.WriteMessage(TraceEventType.Verbose, Resources.TaskResolver_TaskFound, firstTaskName, task.GetType().AssemblyQualifiedName);

            return task;
        }

        /// <summary>
        /// Gets the tasks.
        /// </summary>
        /// <value>
        /// The available tasks.
        /// </value>
        [ImportMany]
        public IEnumerable<ITask> Tasks
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the writer.
        /// </summary>
        /// <value>
        /// The writer.
        /// </value>
        private EventWriter Writer
        {
            get;
            set;
        }
    }
}