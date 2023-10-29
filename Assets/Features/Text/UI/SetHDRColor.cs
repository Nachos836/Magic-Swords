using JetBrains.Annotations;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace MagicSwords.Features.Text.UI
{
    [RequireComponent(typeof(RawImage))]
    internal sealed class SetHDRColor : MonoBehaviour
    {
        [ColorUsage(true,true)]
        [SerializeField] private Color _color;

        [Button] [UsedImplicitly]
        private void Apply()
        {
            if (!TryGetComponent<RawImage>(out var image)) return;

            if (image is not null) image.color = _color;
        }
    }
}
