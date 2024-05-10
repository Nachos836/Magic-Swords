using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MagicSwords.DI.Text
{
    using Features.Generic.Functional;

    internal static class TypedAddressableInstantiate
    {
        public static async UniTask<AsyncResult<TComponent>> InstantiateAsync<TComponent>
        (
            this AssetReferenceGameObject asset,
            Transform parent,
            PlayerLoopTiming yieldPoint = PlayerLoopTiming.Initialization,
            CancellationToken cancellation = default
        ) {
            try
            {
                var (isCanceled, prefab) = await asset.InstantiateAsync(parent)
                    .ToUniTask(progress: null, yieldPoint, cancellation)
                    .SuppressCancellationThrow();

                if (isCanceled) return AsyncResult<TComponent>.Cancel;

                var textReceiver = prefab.GetComponent<TComponent>();

                return textReceiver;
            }
            catch (Exception exception)
            {
                return exception;
            }
        }

        public static async UniTask<AsyncResult<GameObject>> LoadAsync
        (
            this AssetReferenceGameObject asset,
            PlayerLoopTiming yieldPoint = PlayerLoopTiming.Initialization,
            CancellationToken cancellation = default
        ) {
            try
            {
                return await asset.LoadAssetAsync<GameObject>()
                    .ToUniTask(progress: null, yieldPoint, cancellation);
            }
            catch (Exception exception) when (exception is OperationCanceledException or TaskCanceledException)
            {
                return AsyncResult<GameObject>.Cancel;
            }
            catch (Exception exception)
            {
                return exception;
            }
        }

        public static async UniTask<AsyncResult<GameObject>> LoadLazyAsync
        (
            this AssetReferenceGameObject asset,
            PlayerLoopTiming yieldPoint = PlayerLoopTiming.Initialization,
            CancellationToken cancellation = default
        ) {
            try
            {
                var candidate = await asset.LoadAssetAsync<GameObject>()
                    .ToUniTask(progress: null, yieldPoint, cancellation, cancelImmediately: true);

                candidate.SetActive(false);

                return candidate;
            }
            catch (Exception exception) when (exception is OperationCanceledException or TaskCanceledException)
            {
                return AsyncResult<GameObject>.Cancel;
            }
            catch (Exception exception)
            {
                return exception;
            }
        }
    }
}
