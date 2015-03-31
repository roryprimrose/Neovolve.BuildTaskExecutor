namespace Neovolve.BuildTaskExecutor.Services
{
    using System;
    using System.ComponentModel.Composition.Hosting;
    using System.Reflection;

    /// <summary>
    /// The <see cref="ServiceManager"/>
    ///   class is used to resolve services for the application.
    /// </summary>
    internal class ServiceManager : IDisposable
    {
        /// <summary>
        /// Stores the aggregate catalog.
        /// </summary>
        private readonly AggregateCatalog _aggregateCatalog;

        /// <summary>
        /// Stores the assembly catalog.
        /// </summary>
        private readonly AssemblyCatalog _assemblyCatalog;

        /// <summary>
        /// Stores the composition container.
        /// </summary>
        private readonly CompositionContainer _container;

        /// <summary>
        /// Stores the directory catalog.
        /// </summary>
        private readonly DirectoryCatalog _directoryCatalog;

        /// <summary>
        /// Tracks whether Dispose has been called.
        /// </summary>
        private Boolean _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceManager"/> class.
        /// </summary>
        public ServiceManager()
        {
            String domainBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _directoryCatalog = new DirectoryCatalog(domainBaseDirectory);
            _assemblyCatalog = new AssemblyCatalog(Assembly.GetEntryAssembly());
            _aggregateCatalog = new AggregateCatalog();

            _aggregateCatalog.Catalogs.Add(_assemblyCatalog);
            _aggregateCatalog.Catalogs.Add(_directoryCatalog);

            _container = new CompositionContainer(_aggregateCatalog);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <typeparam name="T">
        /// The type of service to obtain.
        /// </typeparam>
        /// <returns>
        /// A <typeparamref name="T"/> instance.
        /// </returns>
        public Lazy<T> GetService<T>()
        {
            return _container.GetExport<T>();
        }

        /// <summary>
        /// Releases the service.
        /// </summary>
        /// <typeparam name="T">
        /// The type of service.
        /// </typeparam>
        /// <param name="service">
        /// The service instance.
        /// </param>
        public void ReleaseService<T>(Lazy<T> service)
        {
            _container.ReleaseExport(service);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(Boolean disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // Dispose managed resources here
                _assemblyCatalog.Dispose();
                _directoryCatalog.Dispose();
                _aggregateCatalog.Dispose();
                _container.Dispose();
            }

            // Dispose unmanaged resources here
            _disposed = true;
        }
    }
}