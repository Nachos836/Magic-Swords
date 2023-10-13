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
            this AssetReference asset,
            Transform parent,
            PlayerLoopTiming yieldPoint = PlayerLoopTiming.Initialization,
            CancellationToken cancellation = default
        ) {
            try
            {
                var prefab = await asset.InstantiateAsync(parent)
                    .ToUniTask(progress: null, yieldPoint, cancellation);
                var textReceiver = prefab.GetComponent<TComponent>();

                return textReceiver;
            }
            catch (Exception exception) when (exception is OperationCanceledException or TaskCanceledException)
            {
                return AsyncResult<TComponent>.Cancel;
            }
            catch (Exception exception)
            {
                return exception;
            }
        }

        public static async UniTask<AsyncResult<GameObject>> LoadAsync
        (
            this AssetReference asset,
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
    }
}
