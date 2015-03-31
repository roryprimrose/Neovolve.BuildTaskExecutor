namespace Neovolve.BuildTaskExecutor.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.Linq;
    using Neovolve.BuildTaskExecutor.Extensibility;
    using Neovolve.BuildTaskExecutor.Properties;
    using Neovolve.BuildTaskExecutor.Services;

    /// <summary>
    /// The help task.
    /// </summary>
    [Export(typeof(ITask))]
    internal class HelpTask : ITask
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HelpTask"/> class.
        /// </summary>
        /// <param name="writer">
        /// The writer.
        /// </param>
        [ImportingConstructor]
        public HelpTask(EventWriter writer)
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
        /// A <see cref="Boolean"/> instance that indicates whether the task executed successfully.
        /// </returns>
        public Boolean Execute(IEnumerable<String> arguments)
        {
            if (arguments.Any())
            {
                String taskName = arguments.FirstOrDefault();

                WriteTaskHelp(taskName);
            }
            else
            {
                WriteAvailableTaskHelp();
            }

            return true;
        }

        /// <summary>
        /// Determines whether the specified argument set is valid.
        /// </summary>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified argument set is valid; otherwise, <c>false</c>.
        /// </returns>
        public Boolean IsValidArgumentSet(IEnumerable<String> arguments)
        {
            return true;
        }

        /// <summary>
        /// Writes the available task help.
        /// </summary>
        private void WriteAvailableTaskHelp()
        {
            Writer.WriteMessage(TraceEventType.Information, Resources.HelpTask_TaskExecuteCommandLine);
            Writer.WriteMessage(TraceEventType.Information, Resources.HelpTask_GenericHelpCommandLine);
            Writer.WriteMessage(TraceEventType.Information, Resources.HelpTask_TaskHelpCommandLine);
            Writer.WriteMessage(TraceEventType.Information, String.Empty);
            Writer.WriteMessage(TraceEventType.Information, Resources.HelpTask_EventLevelHelp);
            Writer.WriteMessage(TraceEventType.Information, String.Empty);
            Writer.WriteMessage(TraceEventType.Information, Resources.HelpTask_TaskListHeader);

            IEnumerable<ITask> sortedTasks = from x in Resolver.Tasks
                                             orderby x.Names.First()
                                             select x;

            foreach (ITask task in sortedTasks)
            {
                String taskNames = task.GetTaskDisplayNames();

                Writer.WriteMessage(TraceEventType.Information, Resources.HelpTask_TaskHelpDescription, taskNames, task.Description);
            }
        }

        /// <summary>
        /// Writes the task help.
        /// </summary>
        /// <param name="taskName">
        /// Name of the task.
        /// </param>
        private void WriteTaskHelp(String taskName)
        {
            ITask task = Resolver.ResolveTask(taskName);

            if (task == null)
            {
                return;
            }

            String taskCommandLineHelp = task.CommandLineArgumentHelp;

            Writer.WriteMessage(TraceEventType.Information, task.Names.First());
            Writer.WriteMessage(TraceEventType.Information, task.Description);
            Writer.WriteMessage(TraceEventType.Information, String.Empty);

            String taskNames = task.GetTaskDisplayNames();

            Writer.WriteMessage(TraceEventType.Information, Resources.HelpTask_TaskCommandLineHelp, taskNames, taskCommandLineHelp);
        }

        /// <summary>
        /// Gets the command line argument help.
        /// </summary>
        /// <value>
        /// The command line argument help.
        /// </value>
        public String CommandLineArgumentHelp
        {
            get
            {
                return "[taskname]";
            }
        }

        /// <summary>
        /// Gets the description of the task.
        /// </summary>
        /// <value>
        /// The task description.
        /// </value>
        public String Description
        {
            get
            {
                return "Displays help information about the available tasks";
            }
        }

        /// <summary>
        /// Gets the command line names for the task.
        /// </summary>
        /// <value>
        /// The command line names for the task.
        /// </value>
        public IEnumerable<String> Names
        {
            get
            {
                return new[]
                       {
                           "Help", "/?"
                       };
            }
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