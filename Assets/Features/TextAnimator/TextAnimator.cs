using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace MagicSwords.Features.TextAnimator
{
    using Effect;
    using Effect.Variants;
    using Effect.Modifiers;
    using TextPlaying;
    using TextParsing;
    using TimeProvider;

    public sealed class TextAnimator : MonoBehaviour
    {
        private TextPlayer _textPlayer;
        private TextParser _textParser;
        private ICurrentTimeProvider _currentTime;
        private IEffect[] _effects;

        [SerializeField] private TMP_Text _field;
        [SerializeField] private string _text;
        [SerializeField] private PlayerLoopTiming _yieldPoint = PlayerLoopTiming.Update;

        private void Awake()
        {
            _textPlayer = new TextPlayer(_field, _yieldPoint);
            _textParser = new TextParser(_text);
            _currentTime = new UnityTimeProvider();
            _effects = new IEffect[]
            {
                new WobbleEffect(() => Appearance.EaseIn(_currentTime.Value))
            };
        }

        private async UniTaskVoid Start()
        {
            await PresentAsync(destroyCancellationToken);
        }

        private async UniTask PresentAsync(CancellationToken cancellation = default)
        {
            List<Tween> tweensList = new ();
            var textBuilder = new StringBuilder();

            await UniTask.SwitchToTaskPool();

            await foreach (var (chars, tweens) in _textParser.ParseAsync(_effects, cancellation))
            {
                tweensList.Add(tweens);
                textBuilder.Append(chars);
            }

            await UniTask.SwitchToMainThread(cancellation);

            await _textPlayer.PlayAsync
            (
                text: textBuilder.ToString(),
                tweens: tweensList.AsQueryable(),
                cancellation: cancellation
            );
        }
    }
}
