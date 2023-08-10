using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace MagicSwords.Features.TextAnimator.Effects
{
    public class WobbleEffect
    {
        private TMP_Text _field;

        private async void PlayText(Func<float, Vector3>[] tweens, CancellationToken cancellation = default)
        {
            await foreach (var letter in PrepareTextPiecesAsync(_field, tweens, cancellation))
            {
                await letter.PrepareAsync(cancellation);
            }

            await foreach (var letter in ShowTextPiecesAsync(_field, cancellation))
            {
                await letter.ShowAsync(cancellation);
            }
        }

        private static async IAsyncEnumerable<IPrepare> PrepareTextPiecesAsync
        (
            TMP_Text field,
            Func<float, Vector3>[] tweens,
            [EnumeratorCancellation] CancellationToken cancellation = default
        ) {
            field.ForceMeshUpdate();

            var textInfo = field.textInfo;

            for (var i = 0; i < textInfo.characterCount; ++i)
            {
                var characterInfo = textInfo.characterInfo[i];
                var current = characterInfo.materialReferenceIndex;

                yield return new Preparation
                (
                    characterInfo,
                    textInfo.meshInfo[current].vertices,
                    tweens[current]
                );
            }
        }

        private async IAsyncEnumerable<IShowable> ShowTextPiecesAsync
        (
            TMP_Text field,
            [EnumeratorCancellation] CancellationToken cancellation = default
        ) {
            var textInfo = field.textInfo;

            for (var i = 0; i < textInfo.meshInfo.Length; ++i)
            {
                yield return new Appearance(field, textInfo, i);
            }
        }

        private static Vector3 WobbleTween(float origin)
        {
            return new Vector3
            (
                x: 0,
                y: Mathf.Sin(Time.time * 2f + origin * 0.01f) * 10f,
                z: 0
            );
        }

        private interface IShowable
        {
            UniTask ShowAsync(CancellationToken cancellation = default);
        }
        
        private interface IPrepare
        {
            UniTask<Vector3[]> PrepareAsync(CancellationToken cancellation = default);
        }

        private sealed class Preparation : IPrepare
        {
            private readonly TMP_CharacterInfo _characterInfo;
            private readonly Vector3[] _vertices;
            private readonly Func<float, Vector3> _tween;

            public Preparation(TMP_CharacterInfo characterInfo, Vector3[] vertices, Func<float, Vector3> tween)
            {
                _characterInfo = characterInfo;
                _vertices = vertices;
                _tween = tween;
            }

            UniTask<Vector3[]> IPrepare.PrepareAsync(CancellationToken cancellation)
            {
                if (_characterInfo.isVisible is false) return UniTask.FromResult(_vertices);

                for (var j = 0; j < 4; ++j)
                {
                    var origin = _vertices[_characterInfo.vertexIndex + j].x;
                    _vertices[_characterInfo.vertexIndex + j] += _tween.Invoke(origin);
                }

                return UniTask.FromResult(_vertices);
            }
        }

        private sealed class Appearance : IShowable
        {
            private readonly TMP_Text _field;
            private readonly TMP_TextInfo _textInfo;
            private readonly int _current;

            public Appearance(TMP_Text field, TMP_TextInfo textInfo, int current)
            {
                _field = field;
                _textInfo = textInfo;
                _current = current;
            }

            public UniTask ShowAsync(CancellationToken cancellation = default)
            {
                var meshInfo = _textInfo.meshInfo[_current];
                meshInfo.mesh.vertices = _textInfo.meshInfo[_current].vertices;

                _field.UpdateGeometry(meshInfo.mesh, _current);

                return UniTask.CompletedTask;
            }
        }
    }
}