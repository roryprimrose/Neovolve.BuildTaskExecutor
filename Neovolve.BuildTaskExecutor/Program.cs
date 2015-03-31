namespace Neovolve.BuildTaskExecutor
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Neovolve.BuildTaskExecutor.Properties;
    using Neovolve.BuildTaskExecutor.Services;

    /// <summary>
    /// The <see cref="Program"/>
    ///   class is used to provide the entry point of the program.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// The Main method is the entry point into the application.
        /// </summary>
        /// <returns>
        /// A <see cref="Int32"/> value that indicates success or failure.
        /// </returns>
        private static Int32 Main()
        {
            using (ServiceManager manager = new ServiceManager())
            {
                Lazy<EventWriter> writerService = manager.GetService<EventWriter>();
                Lazy<TaskExecutor> executorService = manager.GetService<TaskExecutor>();

                try
                {
                    EventWriter writer = writerService.Value;

                    if (writer == null)
                    {
                        throw new InvalidOperationException("Failed to resolve writer");
                    }

                    WriteApplicationInfo(writer);

                    TaskExecutor taskExecutor = executorService.Value;

                    if (taskExecutor == null)
                    {
                        throw new InvalidOperationException("Failed to resolve executor");
                    }

                    Lazy<IEnumerable<String>> arguments = manager.GetService<IEnumerable<String>>();

                    Boolean success = taskExecutor.Execute(arguments.Value);

                    if (success)
                    {
                        return 0;
                    }

                    return 1;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);

                    return 1;
                }
                finally
                {
                    manager.ReleaseService(writerService);
                    manager.ReleaseService(executorService);

#if DEBUG
                    Console.ReadKey();
#endif
                }
            }
        }

        /// <summary>
        /// Writes the application info.
        /// </summary>
        /// <param name="writer">
        /// The writer.
        /// </param>
        private static void WriteApplicationInfo(EventWriter writer)
        {
            String assemblyPath = typeof(Program).Assembly.Location;
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(assemblyPath);

            writer.WriteMessage(TraceEventType.Information, Resources.Executor_ApplicationInformation, versionInfo.ProductVersion);
            writer.WriteMessage(TraceEventType.Information, String.Empty);
        }
    }
}