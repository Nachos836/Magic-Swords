﻿using UnityEngine.AddressableAssets;

namespace MagicSwords.DI.Root.Prerequisites
{
    internal static class DefaultsValidation
    {
        public static bool ConfigIsProvided(Defaults candidate) => candidate.Equals(default) is false;
        public static bool SceneIsGrantedToLoad(int candidate) => candidate is not 0;
        public static bool AssetIsScene(AssetReference candidate) => candidate is { };
    }
}