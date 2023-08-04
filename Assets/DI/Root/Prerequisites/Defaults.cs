using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MagicSwords.DI.Root.Prerequisites
{
    [Serializable]
    internal struct Defaults
    {
        [field: SerializeField]
        [field: ValidateInput(nameof(DefaultsValidation.AssetIsScene), "It's forbidden to Load Application Entry explicitly")]
        public AssetReference MainMenuSceneReference { get; private set; }
    }
}