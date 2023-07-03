using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

using static System.Threading.CancellationTokenSource;

namespace MagicSwords.Features.Dialog
{
    internal sealed class AnimatedTextPresenter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private float _delay = 0.1f;
        [SerializeField] private string[] _monologue;

        private bool _segueEnded;
        private int _current;
        private CancellationTokenSource _buttonPressed;

        private async void Start()
        {
            _buttonPressed = CreateLinkedTokenSource(destroyCancellationToken);

            await TypingStartAsync(_current, _buttonPressed.Token);
        }

        public async void NextText()
        {
            if (_segueEnded) return;

            if (_current >= _monologue.Length)
            {
                await PerformOnTheLastSegmentAsync(destroyCancellationToken);

                _segueEnded = true;

                return;
            }

            _buttonPressed.Cancel();

            _current++;
            for (; _current < _monologue.Length; _current++)
            {
                _buttonPressed.Dispose();
                _buttonPressed = CreateLinkedTokenSource(destroyCancellationToken);

                await TypingStartAsync(_current, _buttonPressed.Token);
            }

            _buttonPressed.Dispose();
        }

        private UniTask TypingStartAsync(int index, CancellationToken cancellation = default)
        {
            return ShowTextAsync(_monologue[index], cancellation);
        }

        private UniTask PerformOnTheLastSegmentAsync(CancellationToken cancellation = default)
        {
            if (cancellation.IsCancellationRequested) return UniTask.CompletedTask;

            _text.text = string.Empty;

            return UniTask.CompletedTask;
        }

        private async UniTask ShowTextAsync(string currentText, CancellationToken cancellation = default)
        {
            for (var i = 0; i < currentText.Length; i++)
            {
                if (cancellation.IsCancellationRequested) return;

                _text.text = currentText[..i];

                await UniTask.Delay(TimeSpan.FromSeconds(_delay), cancellationToken: cancellation)
                    .SuppressCancellationThrow();

                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cancellation)
                    .SuppressCancellationThrow();
            }
        }
    }
}
