using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer.Unity;

namespace MagicSwords.DI.Text
{
    using Features.Generic.Functional;
    using Features.Text;
    using Features.Text.UI;

    internal sealed class TextUIPanel : ITextPanel
    {
        private readonly LifetimeScope _parent;
        private readonly AssetReferenceGameObject _panel;
        private readonly PlayerLoopTiming _yieldPoint;
        private readonly MessagePipeOptions _messagePipeOptions;
        private readonly IText _text;

        public TextUIPanel
        (
            LifetimeScope parent,
            AssetReferenceGameObject panel,
            PlayerLoopTiming yieldPoint,
            MessagePipeOptions messagePipeOptions,
            IText text
        ) {
            _parent = parent;
            _panel = panel;
            _yieldPoint = yieldPoint;
            _messagePipeOptions = messagePipeOptions;
            _text = text;
        }

        UniTask<AsyncResult<IDisposable>> ITextPanel.LoadAsync(CancellationToken cancellation)
        {
            return AsyncResult<IText, MessagePipeOptions>
                .FromResult(_text, _messagePipeOptions)
                .Run<TextPanelInstaller>(static (text, options) => new TextPanelInstaller(options, text))
                .Attach(_parent)
                .Run<TextPanelScope>(static (installer, parent) => parent.CreateChild<TextPanelScope>(installer))
                .Attach(_panel, _yieldPoint)
                .RunAsync(static async (scope, asset, yieldPoint, token) =>
                {
                    var result = await asset.InstantiateAsync<TextPresenter>(scope.transform, yieldPoint, token);

                    return result.Attach(scope, asset);
                }, cancellation)
                .RunAsync(async static (presenter, scope, asset, token) =>
                {
                    if (token.IsCancellationRequested) return AsyncResult<IDisposable>.Cancel;

                    var cancelled = await UniTask.RunOnThreadPool
                    (
                        action: () => scope.Container.Inject(presenter),
                        cancellationToken: token

                    ).SuppressCancellationThrow();

                    return cancelled is false
                        ? new UnloadHandler(scope, asset, presenter.gameObject)
                        : token;
                }, cancellation);
        }

        private sealed class UnloadHandler : IDisposable
        {
            private readonly LifetimeScope _scope;
            private readonly AssetReferenceGameObject _addressable;
            private readonly GameObject _prefab;

            public UnloadHandler
            (
                LifetimeScope scope,
                AssetReferenceGameObject addressable,
                GameObject prefab
            ) {
                _scope = scope;
                _addressable = addressable;
                _prefab = prefab;
            }

            void IDisposable.Dispose()
            {
                if (_prefab is not null) _addressable.ReleaseInstance(_prefab);

                _scope.Dispose();
            }
        }
    }
}
