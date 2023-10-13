using System;
using NaughtyAttributes;
using UnityEngine;

namespace MagicSwords.Features.Text.AnimatedRichText.Animating
{
    [Serializable]
    internal abstract class EffectConfig : ScriptableObject
    {
        [field: SerializeField]
        [field: ReadOnly]
        [field: ValidateInput(nameof(NameIsProvided), ValidationFails)]
        public string Name { get; protected set; }

        [field: SerializeReference]
        [field: Label("Default Effect Params")]
        [field: ReadOnly]
        [field: ValidateInput(nameof(EffectIsProvided), ValidationFails)]
        public IEffect Effect { get; protected set; }

        [Button]
        private void Configure() => (Name, Effect) = ProvideConfiguration();

        protected abstract (string Name, IEffect Effect) ProvideConfiguration();

#   region Configure Validation

        private const string ValidationFails = "You must Configure asset!";
        private static bool EffectIsProvided(IEffect effect) => effect is not null;
        private static bool NameIsProvided(string name) => name is { Length:> 0 };

#   endregion
    }
}
