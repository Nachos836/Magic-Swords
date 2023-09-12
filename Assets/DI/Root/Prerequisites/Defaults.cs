using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MagicSwords.DI.Root.Prerequisites
{
    [Serializable]
    internal struct Defaults
    {
        [field: SerializeField] public AssetReference MainMenuSceneReference { get; private set; }
    }
}
