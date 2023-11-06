using System;
using UnityEngine.AddressableAssets;
using VContainer;

namespace MagicSwords.DI.Common
{
    internal static class Validation
    {
        internal const string OfAssetReferenceGameObject = nameof(ValidateAssetReferenceGameObject);
        internal const string OfAssetReference = nameof(ValidateAssetReference);
        internal const string OfUnityObject = nameof(ValidateUnityObject);

        internal static bool ValidateAssetReferenceGameObject(AssetReferenceGameObject? input)
        {
            return input is not null and not AssetReferenceGameObjectEmpty;
        }

        internal static bool ValidateAssetReference(AssetReference? input)
        {
            return input is not null and not AssetReferenceEmpty;
        }

        internal static bool ValidateUnityObject(UnityEngine.Object? input)
        {
            return input is not null;
        }
    }

    [InjectIgnore]
    internal sealed class AssetReferenceGameObjectEmpty : AssetReferenceGameObject
    {
        internal static AssetReferenceGameObject Instance { get; } = new AssetReferenceGameObjectEmpty();

        private AssetReferenceGameObjectEmpty() : base(Guid.Empty.ToString()) { }
    }

    [InjectIgnore]
    internal sealed class AssetReferenceEmpty : AssetReference
    {
        internal static AssetReference Instance { get; } = new AssetReferenceEmpty();

        private AssetReferenceEmpty() : base(Guid.Empty.ToString()) { }
    }
}
