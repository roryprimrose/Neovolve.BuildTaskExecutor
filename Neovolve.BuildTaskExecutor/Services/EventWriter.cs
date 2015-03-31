namespace Neovolve.BuildTaskExecutor.Services
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.Linq;
    using Neovolve.BuildTaskExecutor.Extensibility;

    /// <summary>
    /// The <see cref="EventWriter"/>
    ///   class writes event information to the provided <see cref="IEventWriter"/>
    ///   instances.
    /// </summary>
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class EventWriter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventWriter"/> class.
        /// </summary>
        /// <param name="eventWriters">
        /// The event writers.
        /// </param>
        /// <param name="eventLevel">
        /// The event level.
        /// </param>
        [ImportingConstructor]
        public EventWriter([ImportMany] IEventWriter[] eventWriters, TraceEventType eventLevel)
        {
            if (eventWriters == null)
            {
                throw new ArgumentNullException("eventWriters");
            }

            EventWriters = eventWriters.ToList();
            EventLevel = eventLevel;
        }

        /// <summary>
        /// Writes the message to the configured <see cref="IEventWriter"/> instances.
        /// </summary>
        /// <param name="eventType">
        /// Type of the event.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        public void WriteMessage(TraceEventType eventType, String message, params Object[] arguments)
        {
            if (eventType > EventLevel)
            {
                return;
            }

            EventWriters.ForEach(x => x.WriteMessage(eventType, message, arguments));
        }

        /// <summary>
        /// Gets the event level.
        /// </summary>
        /// <value>
        /// The event level.
        /// </value>
        /// <remarks>
        /// Any trace event provided to the <see cref="WriteMessage"/> method that has a level higher than this value
        ///   is be ignored.
        /// </remarks>
        public TraceEventType EventLevel
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the event writers.
        /// </summary>
        /// <value>
        /// The event writers.
        /// </value>
        private List<IEventWriter> EventWriters
        {
            get;
            set;
        }
    }
}