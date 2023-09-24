using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MagicSwords.Features.Dialog
{
    using Generic.Sequencer;

    internal sealed class DialogEntryPoint : IAsyncStartable
    {
        private Sequencer _sequencer;

        [Inject] internal void Construct(Sequencer sequencer) => _sequencer = sequencer;

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            Debug.Log("Вот начало диалога!");

            var outcome = await _sequencer.StartAsync(cancellation);
            outcome.Match
            (
                success: _ =>
                {
                    Debug.Log("Успешно завершено!");

                    return 1;
                },
                expected: _ =>
                {
                    Debug.Log("Выполнение было отменено!");

                    return 2;
                },
                unexpected: _ =>
                {
                    Debug.Log("Произошла ошибка...");

                    return 3;
                }
            );
        }
    }
}
