using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using static System.Threading.CancellationTokenSource;

namespace MagicSwords.Features.Dialog
{
    public class TextAnimator : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private float _delay = 0.1f;
        [SerializeField] private string[] _monologue;

        private int _current;
        private CancellationTokenSource _buttonPressed;

        private async void Start()
        {
            _buttonPressed = CreateLinkedTokenSource(destroyCancellationToken);

            await TypingStartAsync(_current, _buttonPressed.Token);
        }

        public async void NextText()
        {
            _buttonPressed.Cancel();
            _current++;
            for (; _current < _monologue.Length; _current++)
            {
                _buttonPressed = CreateLinkedTokenSource(destroyCancellationToken);

                await TypingStartAsync(_current, _buttonPressed.Token);
            }
        }

        private async UniTask ShowTextAsync(string currentText,CancellationToken cancellation = default)
        {
            for (var i = 0; i < currentText.Length; i++)
            {
                if (cancellation.IsCancellationRequested) return;
                
                _text.text = currentText[..i];
                await UniTask.Delay(TimeSpan.FromSeconds(_delay), cancellationToken: cancellation);
                await UniTask.Yield();
            }
        }

        private UniTask TypingStartAsync(int index,CancellationToken cancellation = default)
        {
            return ShowTextAsync(_monologue[index], cancellation);
        }
    }
}
