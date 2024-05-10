using System.Collections.Immutable;
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
        private readonly PlayerLoopTiming _loadPoint;
        private readonly PlayerLoopTiming _animationPoint;
        private readonly ImmutableArray<IText> _message;

        public TextUIPanel
        (
            AssetReferenceGameObject panelScopeAsset,
            LifetimeScope parent,
            PlayerLoopTiming loadPoint,
            PlayerLoopTiming animationPoint,
            ImmutableArray<IText> message
        ) {
            _panelScopeAsset = panelScopeAsset;
            _parent = parent;
            _loadPoint = loadPoint;
            _animationPoint = animationPoint;
            _message = message;
        }

        UniTask<AsyncResult<ScopeActivator>> ITextPanel.LoadAsync(CancellationToken cancellation)
        {
            return AsyncResult<AssetReferenceGameObject, PlayerLoopTiming>
                .FromResult(_panelScopeAsset, _loadPoint)
                .RunAsync(static async (panelScopeAsset, yieldPoint, token) =>
                {
                    return await panelScopeAsset.LoadLazyAsync(yieldPoint, token);

                }, cancellation)
                .RunAsync(static (scopeGameObject, token) =>
                {
                    if (token.IsCancellationRequested) return UniTask.FromResult(AsyncResult<TextPanelScope>.Cancel);

                    return UniTask.FromResult<AsyncResult<TextPanelScope>>
                    (
                        scopeGameObject.GetComponent<TextPanelScope>()
                    );
                }, cancellation: cancellation)
                .AttachAsync(_message, _animationPoint, cancellation: cancellation)
                .RunAsync(static (scope, text, yieldPoint, token) =>
                {
                    if (token.IsCancellationRequested)
                    {
                        return UniTask.FromResult(AsyncResult<SingleTextMessageInstaller, TextPanelScope>.Cancel);
                    }

                    return UniTask.FromResult
                    (
                        AsyncResult<SingleTextMessageInstaller>.FromResult(new SingleTextMessageInstaller(text[0], yieldPoint))
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
                    return UniTask.FromResult<AsyncResult<ScopeActivator>>
                    (
                        token.IsCancellationRequested is not true
                            ? new ScopeActivator(scope.gameObject, scope)
                            : token
                    );
                }, cancellation: cancellation);
        }
    }
}
