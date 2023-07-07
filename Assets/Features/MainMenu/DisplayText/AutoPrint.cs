using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MagicSwords.Features.Generic.StateMachine;
using TMPro;

namespace MagicSwords.Features.MainMenu.DisplayText
{
    public class AutoPrint : IState, IState.IWithEnterAction
    {
        private readonly string _currentText;
        private readonly TextMeshProUGUI _text;
        private readonly float _delay;

        public AutoPrint(string currentText, TextMeshProUGUI text, float delay)
        {
            _currentText = currentText;
            _text = text;
            _delay = delay;

        }
        public async UniTask OnEnterAsync(CancellationToken cancellation = default)
        {
            for (var i = 0; i < _currentText.Length; i++)
            {
                if (cancellation.IsCancellationRequested) return;

                _text.text = _currentText[..i];

                await UniTask.Delay(TimeSpan.FromSeconds(_delay), cancellationToken: cancellation)
                    .SuppressCancellationThrow();

                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cancellation)
                    .SuppressCancellationThrow();
            }
        }
    }
}