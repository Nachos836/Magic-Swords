using TMPro;
using UnityEngine;
using NaughtyAttributes;

namespace MagicSwords.Features.Text
{
    internal sealed class TextFieldProvider : MonoBehaviour
    {
        [field: SerializeField] [field: ReadOnly] public TMP_Text? Field { get; private set; }

        private void OnValidate() => Field ??= GetComponentInChildren<TMP_Text>();
        private void Awake() => Field!.ClearMesh();
    }
}
