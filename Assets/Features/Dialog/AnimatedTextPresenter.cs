using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace MagicSwords.Features.Dialog
{
    using Generic.Sequencer;

    public sealed class AnimatedTextPresenter : MonoBehaviour
    {
        private Sequencer _sequencer;

        [Inject] internal void Construct(Sequencer sequencer) => _sequencer = sequencer;

        private async UniTaskVoid Start()
        {
            var outcome = await _sequencer.StartAsync(destroyCancellationToken);
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
