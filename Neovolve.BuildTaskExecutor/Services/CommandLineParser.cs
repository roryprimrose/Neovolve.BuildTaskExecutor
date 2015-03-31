namespace Neovolve.BuildTaskExecutor.Services
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// The <see cref="CommandLineParser"/>
    ///   class is used to parse command line arguments for the application and extract task execution arguments.
    /// </summary>
    internal class CommandLineParser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineParser"/> class.
        /// </summary>
        public CommandLineParser()
        {
            String[] commandLineArguments = Environment.GetCommandLineArgs();
            IEnumerable<String> availableArguments = GetArguments(commandLineArguments);

            ParseArguments(availableArguments);
        }

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <returns>
        /// A <see cref="IEnumerable&lt;T&gt;"/> instance.
        /// </returns>
        private static IEnumerable<String> GetArguments(IEnumerable<String> arguments)
        {
            if (arguments == null)
            {
                return new List<String>();
            }

            return arguments.Skip(1).Where(x => String.IsNullOrWhiteSpace(x) == false).ToList();
        }

        /// <summary>
        /// Parses the arguments.
        /// </summary>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        private void ParseArguments(IEnumerable<String> arguments)
        {
            const String EventLevelArgumentPrefix = "/el:";

            EventLevel = TraceEventType.Information;
            IList<String> taskArguments = new List<String>(arguments);
            String eventLevelArgument = taskArguments.ParseArgument(EventLevelArgumentPrefix);

            if (String.IsNullOrWhiteSpace(eventLevelArgument) == false)
            {
                TraceEventType eventLevel;

                if (Enum.TryParse(eventLevelArgument, true, out eventLevel))
                {
                    EventLevel = eventLevel;
                }

                taskArguments.Remove(EventLevelArgumentPrefix + eventLevelArgument);
            }

            TaskArguments = taskArguments;
        }

        /// <summary>
        /// Gets the event level.
        /// </summary>
        [Export]
        public TraceEventType EventLevel
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the task arguments.
        /// </summary>
        [Export]
        public IEnumerable<String> TaskArguments
        {
            get;
            private set;
        }
    }
}