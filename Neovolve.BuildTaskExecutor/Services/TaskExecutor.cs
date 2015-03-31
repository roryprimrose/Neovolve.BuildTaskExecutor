namespace Neovolve.BuildTaskExecutor.Services
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.Linq;
    using Neovolve.BuildTaskExecutor.Extensibility;
    using Neovolve.BuildTaskExecutor.Properties;
    using Neovolve.BuildTaskExecutor.Tasks;

    /// <summary>
    /// The <see cref="TaskExecutor"/>
    ///   class is used to execute the required task based on a set of arguments.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The first argument needs to match a name for an available task.
    ///   </para>
    /// <note>
    /// <para>
    /// <b>Do not import this type into an <see cref="ITask"/> constructor decorated with the <see cref="ImportingConstructorAttribute"/>.</b>
    ///     </para>
    /// <para>
    /// This action will cause a circular reference between the <see cref="TaskExecutor"/> and the <see cref="ITask"/> instance trying to import it.
    ///       MEF will throw a composition error in this scenario. The way to achieve this result is to import the <see cref="TaskExecutor"/>
    ///       as a property on the <see cref="ITask"/> class.
    ///     </para>
    /// </note>
    /// </remarks>
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TaskExecutor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskExecutor"/> class.
        /// </summary>
        /// <param name="writer">
        /// The writer.
        /// </param>
        [ImportingConstructor]
        public TaskExecutor(EventWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            Writer = writer;
        }

        /// <summary>
        /// Executes the specified arguments.
        /// </summary>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <returns>
        /// A <see cref="Boolean"/> instance that indicates whether a task could be executed successfully.
        /// </returns>
        public Boolean Execute(IEnumerable<String> arguments)
        {
            try
            {
                HelpTask helpTask = Resolver.Tasks.OfType<HelpTask>().Single();

                if (arguments.Any() == false)
                {
                    Writer.WriteMessage(TraceEventType.Error, Resources.Executor_NoArgumentsProvided);

                    helpTask.Execute(arguments);

                    return false;
                }

                String taskName = arguments.First();
                ITask task = Resolver.ResolveTask(taskName);

                if (task == null)
                {
                    return false;
                }

                // We have an exact task match
                String firstTaskName = task.Names.First();
                String[] taskArguments = arguments.Skip(1).ToArray();

                if (task is HelpTask == false)
                {
                    Writer.WriteMessage(TraceEventType.Information, Resources.Executor_ExecutingTask, firstTaskName);
                }

                if (task.IsValidArgumentSet(taskArguments))
                {
                    return task.Execute(taskArguments);
                }

                Writer.WriteMessage(TraceEventType.Error, Resources.Executor_InvalidTaskArguments, firstTaskName);

                helpTask.Execute(arguments);
            }
            catch (Exception ex)
            {
                Writer.WriteMessage(TraceEventType.Error, ex.ToString());
            }

            return false;
        }

        /// <summary>
        /// Gets or sets the resolver.
        /// </summary>
        /// <value>
        /// The resolver.
        /// </value>
        [Import]
        private TaskResolver Resolver
        {
            get;
            set;
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