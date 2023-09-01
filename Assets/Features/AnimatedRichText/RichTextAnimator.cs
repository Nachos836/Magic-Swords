using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using NaughtyAttributes;
using TMPro;
using UnityEngine;

namespace MagicSwords.Features.AnimatedRichText
{
    using Playing;
    using TimeProvider.Providers;

    internal sealed class RichTextAnimator : MonoBehaviour
    {
        private Player _player;

        [SerializeField] [ReadOnly] private TMP_Text _field;
        [SerializeField] private RichText _configurationProvider;
        [SerializeField] private PlayerLoopTiming _yieldPoint = PlayerLoopTiming.Update;

        private void Awake()
        {
            _player = new Player(_field, _yieldPoint, new UnityTimeProvider());
        }

        private void Reset()
        {
            _field = GetComponentInChildren<TMP_Text>();
        }

        [UsedImplicitly] // ReSharper disable once Unity.IncorrectMethodSignature
        private async UniTaskVoid Start()
        {
            await PresentAsync(destroyCancellationToken);
        }

        private async UniTask PresentAsync(CancellationToken cancellation = default)
        {
            var config = _configurationProvider.GeneratedConfig;

            await _player.PlayAsync(config.PlainText, config.Tweens, cancellation);
        }
    }
}
