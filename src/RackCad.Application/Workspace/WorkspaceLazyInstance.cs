#nullable enable
using System;

namespace RackCad.Application.Workspace
{
    /// <summary>
    /// Holds the one lazily created instance of something that must exist once per process (I-64 D-02, P-01).
    /// Nothing is created until the first <see cref="GetOrCreate"/>; every later call returns the same instance, and a
    /// factory that fails leaves nothing created.
    /// </summary>
    public sealed class WorkspaceLazyInstance<T> where T : class
    {
        private readonly Func<T> _factory;
        private readonly object _gate = new();
        private T? _instance;
        private int _createCount;

        public WorkspaceLazyInstance(Func<T> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public bool IsCreated
        {
            get
            {
                lock (_gate) return _instance != null;
            }
        }

        public int CreateCount
        {
            get
            {
                lock (_gate) return _createCount;
            }
        }

        public T GetOrCreate()
        {
            lock (_gate)
            {
                if (_instance == null)
                {
                    var created = _factory() ?? throw new InvalidOperationException("The factory returned nothing.");
                    _instance = created;
                    _createCount++;
                }

                return _instance;
            }
        }
    }
}
