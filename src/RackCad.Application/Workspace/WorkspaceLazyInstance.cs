#nullable enable
using System;

namespace RackCad.Application.Workspace
{
    /// <summary>
    /// Holds the one lazily created instance of something that must exist once per process (I-64 D-02, P-01).
    /// Nothing is created until the first <see cref="GetOrCreate"/>; every later call returns the same instance.
    /// </summary>
    public sealed class WorkspaceLazyInstance<T> where T : class
    {
        private readonly Func<T> _factory;

        public WorkspaceLazyInstance(Func<T> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public bool IsCreated => false;

        public int CreateCount => 0;

        public T GetOrCreate() => _factory();
    }
}
