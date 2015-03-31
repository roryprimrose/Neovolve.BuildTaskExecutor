namespace Neovolve.BuildTaskExecutor.Services
{
    using System;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.Globalization;
    using Neovolve.BuildTaskExecutor.Extensibility;

    /// <summary>
    /// The <see cref="ConsoleEventWriter"/>
    ///   class is used to write event information to the console.
    /// </summary>
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Export(typeof(IEventWriter))]
    internal class ConsoleEventWriter : IEventWriter, IDisposable
    {
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Writes the message.
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
            switch (eventType)
            {
                case TraceEventType.Critical:
                case TraceEventType.Error:

                    Console.ForegroundColor = ConsoleColor.Red;

                    break;

                case TraceEventType.Warning:

                    Console.ForegroundColor = ConsoleColor.Yellow;

                    break;

                case TraceEventType.Verbose:

                    Console.ForegroundColor = ConsoleColor.DarkGreen;

                    break;

                default:

                    Console.ResetColor();

                    break;
            }

            if (arguments == null)
            {
                Console.WriteLine(message);
            }
            else if (arguments.Length == 0)
            {
                Console.WriteLine(message);
            }
            else
            {
                String messageToWrite = String.Format(CultureInfo.CurrentCulture, message, arguments);

                Console.WriteLine(messageToWrite);
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(Boolean disposing)
        {
            if (Disposed)
            {
                return;
            }

            if (disposing)
            {
                // Dispose managed resources here
                Console.ResetColor();
            }

            // Dispose unmanaged resources here
            Disposed = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ConsoleEventWriter"/> is disposed.
        /// </summary>
        /// <value>
        /// <c>true</c> if disposed; otherwise, <c>false</c>.
        /// </value>
        private Boolean Disposed
        {
            get;
            set;
        }
    }
}