using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

namespace MagicSwords.Features.Text.UI
{
    using Generic.Functional;

    public interface ITextPanel
    {
        UniTask<AsyncResult<ScopeActivator>> LoadAsync(CancellationToken cancellation = default);
    }

    public sealed class ScopeActivator
    {
        private readonly GameObject _scopeObject;
        private readonly LifetimeScope _scope;

        public ScopeActivator(GameObject scopeObject, LifetimeScope scope)
        {
            _scopeObject = scopeObject;
            _scope = scope;
        }

        public IDisposable Activate()
        {
            _scopeObject.SetActive(true);

            return _scope;
        }
    }
}
