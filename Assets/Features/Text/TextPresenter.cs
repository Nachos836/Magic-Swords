using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using MessagePipe;
using NaughtyAttributes;
using VContainer;

namespace MagicSwords.Features.Text
{
    using Generic.Functional;
    using AnimatedRichText.Playing;
    using TimeProvider;

    internal interface IPresentJob
    {
        AsyncLazy<AsyncResult> PresentAsync(CancellationToken token = default);
    }

    internal sealed class TextPresenter : MonoBehaviour, IPresentJob
    {
        private IText _text;
        private IBufferedAsyncPublisher<IPresentJob> _readyToPresent;
        private ICurrentTimeProvider _timeProvider;

        [SerializeField] [ReadOnly] private TMP_Text _field;

        [Inject]
        internal void Construct
        (
            IText text,
            IBufferedAsyncPublisher<IPresentJob> readyToPresent,
            ICurrentTimeProvider timeProvider
        ) {
            _text = text;
            _readyToPresent = readyToPresent;
            _timeProvider = timeProvider;
        }

        private void OnValidate() => _field ??= GetComponentInChildren<TMP_Text>();
        private void Awake() => _field.ClearMesh();

        [UsedImplicitly] // ReSharper disable once Unity.IncorrectMethodSignature
        private async UniTaskVoid Start()
        {
            await _readyToPresent.PublishAsync(this, destroyCancellationToken);
        }

        AsyncLazy<AsyncResult> IPresentJob.PresentAsync(CancellationToken token)
        {
            return _text.PresentAsync(new Player(_field, _timeProvider), token);
        }
    }
}
