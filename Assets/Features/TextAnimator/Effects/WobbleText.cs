using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using TMPro;
using UnityEngine;
using static Cysharp.Threading.Tasks.Linq.UniTaskAsyncEnumerable;

namespace MagicSwords.Features.TextAnimator.Effects
{
    [RequireComponent(typeof(TMP_Text))]
    public sealed class WobbleText : MonoBehaviour
    {
        private TMP_Text _field;
        private TMP_Text Field => _field ??= GetComponent<TMP_Text>();
        
        private async UniTaskVoid Start()
        {
            await WobbleAsync(destroyCancellationToken);
        }

        private async UniTask WobbleAsync(CancellationToken cancellation = default)
        {
            await foreach (var _ in EveryUpdate(PlayerLoopTiming.FixedUpdate)
                .TakeUntilCanceled(cancellation)
                .WithCancellation(cancellation)
            ) {
                Field.ForceMeshUpdate();

                var textInfo = Field.textInfo;
                for (var i = 0; i < textInfo.characterCount; ++i)
                {
                    var charInfo = textInfo.characterInfo[i];
                    
                    if (charInfo.isVisible is false) continue;

                    var vertices = textInfo
                        .meshInfo[charInfo.materialReferenceIndex]
                        .vertices;

                    for (var j = 0; j < 4; ++j)
                    {
                        var origin = vertices[charInfo.vertexIndex + j].x;
                        vertices[charInfo.vertexIndex + j] += new Vector3
                        (
                            x: 0,
                            y: Mathf.Sin(Time.time * 2f + origin * 0.01f) * 10f,
                            z: 0
                        );
                    }
                }

                for (var i = 0; i < textInfo.meshInfo.Length; ++i)
                {
                    var meshInfo = textInfo.meshInfo[i];
                    meshInfo.mesh.vertices = textInfo.meshInfo[i].vertices;

                    Field.UpdateGeometry(meshInfo.mesh, i);
                }
                
            }
        }
    }
}
