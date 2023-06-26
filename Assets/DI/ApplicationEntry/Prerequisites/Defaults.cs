using System;
using NaughtyAttributes;
using UnityEngine;

namespace MagicSwords.DI.ApplicationEntry.Prerequisites
{
    [Serializable]
    internal struct Defaults
    {
        [field: SerializeField]
        [field: Scene]
        [field: MinValue(1)]
        [field: ValidateInput(nameof(DefaultsValidation.SceneIsGrantedToLoad), "It's forbidden to Load Application Entry explicitly")]
        public int MainMenuScene { get; private set; }
    }
}