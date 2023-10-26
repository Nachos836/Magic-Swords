using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using VContainer.Unity;

namespace MagicSwords.DI.Text
{
    using Features.Generic.Functional;
    using Features.Text;
    using Features.Text.UI;

    internal sealed class TextUIPanel : ITextPanel
    {
        private readonly AssetReferenceGameObject _panelScopeAsset;
        private readonly LifetimeScope _parent;
        private readonly PlayerLoopTiming _yieldPoint;
        private readonly IText[] _message;

        public TextUIPanel
        (
            AssetReferenceGameObject panelScopeAsset,
            LifetimeScope parent,
            PlayerLoopTiming yieldPoint,
            IText[] message
        ) {
            _panelScopeAsset = panelScopeAsset;
            _parent = parent;
            _yieldPoint = yieldPoint;
            _message = message;
        }

        UniTask<AsyncResult<IDisposable>> ITextPanel.LoadAsync(CancellationToken cancellation)
        {
            return AsyncResult<AssetReferenceGameObject, PlayerLoopTiming>
                .FromResult(_panelScopeAsset, _yieldPoint)
                .RunAsync(static async (panelScopeAsset, yieldPoint, token) =>
                {
                    return await panelScopeAsset.LoadAsync(yieldPoint, token);

                }, cancellation)
                .RunAsync(static (scopeGameObject, token) =>
                {
                    if (token.IsCancellationRequested) return UniTask.FromResult(AsyncResult<TextPanelScope>.Cancel);

                    return UniTask.FromResult<AsyncResult<TextPanelScope>>
                    (
                        scopeGameObject.GetComponent<TextPanelScope>()
                    );
                }, cancellation: cancellation)
                .AttachAsync(_message, cancellation: cancellation)
                .RunAsync(static (scope, text, token) =>
                {
                    if (token.IsCancellationRequested)
                    {
                        return UniTask.FromResult(AsyncResult<SequencedTextMessageInstaller, TextPanelScope>.Cancel);
                    }

                    return UniTask.FromResult
                    (
                        AsyncResult<SequencedTextMessageInstaller>.FromResult(new SequencedTextMessageInstaller(text))
                            .Attach(scope)
                    );
                }, cancellation)
                .AttachAsync(_parent, cancellation)
                .RunAsync(static (installer, scope, parent, token) =>
                {
                    if (token.IsCancellationRequested) return UniTask.FromResult(AsyncResult<TextPanelScope>.Cancel);

                    return UniTask.FromResult<AsyncResult<TextPanelScope>>
                    (
                        parent.CreateChildFromPrefab(scope, installer)
                    );
                }, cancellation)
                .RunAsync(static (scope, token) =>
                {
                    return UniTask.FromResult<AsyncResult<IDisposable>>
                    (
                        token.IsCancellationRequested is false
                            ? new UnloadHandler(scope)
                            : token
                    );
                }, cancellation: cancellation);
        }

        private sealed class UnloadHandler : IDisposable
        {
            private readonly LifetimeScope _scope;

            public UnloadHandler(LifetimeScope scope) => _scope = scope;

            void IDisposable.Dispose() => _scope.Dispose();
        }
    }
}
