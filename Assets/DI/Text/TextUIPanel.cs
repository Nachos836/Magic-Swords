﻿using System;
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
        private readonly AssetReferenceGameObject _panelScopeAsset;
        private readonly LifetimeScope _parent;
        private readonly PlayerLoopTiming _yieldPoint;
        private readonly MessagePipeOptions _messagePipeOptions;
        private readonly IText _text;

        public TextUIPanel
        (
            AssetReferenceGameObject panelScopeAsset,
            LifetimeScope parent,
            PlayerLoopTiming yieldPoint,
            MessagePipeOptions messagePipeOptions,
            IText text
        ) {
            _panelScopeAsset = panelScopeAsset;
            _parent = parent;
            _yieldPoint = yieldPoint;
            _messagePipeOptions = messagePipeOptions;
            _text = text;
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
                .AttachAsync(_messagePipeOptions, cancellation: cancellation)
                .RunAsync(static (scope, options, token) =>
                {
                    if (token.IsCancellationRequested)
                    {
                        return UniTask.FromResult(AsyncResult<TextPanelInstaller, TextPanelScope>.Cancel);
                    }

                    return UniTask.FromResult
                    (
                        AsyncResult<TextPanelInstaller>.FromResult(new TextPanelInstaller(options))
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
                            ? new UnloadHandler(scope, null, null)
                            : token
                    );
                }, cancellation: cancellation);
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
