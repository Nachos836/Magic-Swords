using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MagicSwords.Features.Generic.StateMachine;
using MagicSwords.Features.MainMenu.DisplayText;
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

        private readonly StateMachine2 _stateMachine = new ();
        private bool _segueEnded;
        private int _current;
        private CancellationTokenSource _buttonPressed;
        private abstract class Arrow<TFrom,TTo> where TFrom : IState where TTo : IState{}
        private void Awake()
        {
            var autoPrint = new AutoPrint(_monologue[0],_text,_delay);
            var downMouse = new DownMouse();
            var endText = new EndText();
            var nextSegment = new NextSegment();
            var wait = new Wait();

            _stateMachine.AddTransition<Arrow<AutoPrint,DownMouse>>(autoPrint,downMouse);
            _stateMachine.AddTransition<Arrow<AutoPrint,EndText>>(autoPrint,endText);
            _stateMachine.AddTransition<Arrow<DownMouse,Wait>>(downMouse,wait);
            _stateMachine.AddTransition<Arrow<DownMouse,NextSegment>>(downMouse,nextSegment);
            _stateMachine.AddTransition<Arrow<Wait,EndText>>(wait,endText);
            _stateMachine.AddTransition<Arrow<NextSegment,EndText>>(nextSegment,endText);
        }

        private void Start()
        {
            _buttonPressed = CreateLinkedTokenSource(destroyCancellationToken);
        }

        public async void NextText()
        {
            await _stateMachine.TransitAsync<Arrow<AutoPrint, DownMouse>>();

            await _stateMachine.TransitAsync<Arrow<DownMouse, NextSegment>>();
            
            await _stateMachine.TransitAsync<Arrow<DownMouse,Wait>>();
        }

        public async void StopPrint()
        {
            await _stateMachine.TransitAsync<Arrow<Wait, EndText>>();
            await _stateMachine.TransitAsync<Arrow<NextSegment, EndText>>();
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
