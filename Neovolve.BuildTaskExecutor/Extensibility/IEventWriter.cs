namespace Neovolve.BuildTaskExecutor.Extensibility
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// The <see cref="IEventWriter"/>
    ///   interface defines the methods for writing event messages.
    /// </summary>
    public interface IEventWriter
    {
        /// <summary>
        /// Writes the specified message.
        /// </summary>
        /// <param name="eventType">
        /// Type of the event.
        /// </param>
        /// <param name="message">
        /// The event message.
        /// </param>
        /// <param name="arguments">
        /// The optional event arguments.
        /// </param>
        /// <remarks>
        /// The processing of the <paramref name="arguments"/> parameter is up to the implementation of the <see cref="IEventWriter"/>.
        ///   Typically this would involve an invocation of <see cref="string.Format(System.IFormatProvider, String, Object[])"/>.
        /// </remarks>
        void WriteMessage(TraceEventType eventType, String message, params Object[] arguments);
    }
}