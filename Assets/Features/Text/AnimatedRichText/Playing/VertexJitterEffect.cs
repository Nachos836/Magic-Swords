using UnityEngine;
using System.Threading;
using TMPro;
using VContainer;
using ZBase.Foundation.PolymorphicStructs;

namespace MagicSwords.Features.Text.AnimatedRichText.Playing
{
    [InjectIgnore]
    [PolymorphicStruct]
    public readonly partial struct VertexJitterEffect : IIdleAsyncEffect
    {
        private readonly float _angleMultiplier;
        private readonly float _speedMultiplier;
        private readonly float _curveScale;

        private readonly TMP_Text _field;
        // Cache the vertex data of the text object as the Jitter FX is applied to the original position of the characters.
        private readonly TMP_MeshInfo[] _cachedMeshInfo; // _field.textInfo.CopyMeshInfoVertexData();
        private readonly int _current;

        /// <summary>
        /// Method to animate vertex colors of a TMP Text object.
        /// </summary>
        /// <returns></returns>
        void IIdleAsyncEffect.ApplyAsync(float time, CancellationToken cancellation)
        {
            var textInfo = _field.textInfo;

            // Get the index of the material used by the current character.
            var materialIndex = textInfo.characterInfo[_current].materialReferenceIndex;

            // Get the index of the first vertex used by this text element.
            var vertexIndex = textInfo.characterInfo[_current].vertexIndex;

            // Get the cached vertices of the mesh used by this text element (character or sprite).
            var sourceVertices = _cachedMeshInfo[materialIndex].vertices;

            // Determine the center point of each character at the baseline.
            //Vector2 charMidBaseline = new Vector2((sourceVertices[vertexIndex + 0].x + sourceVertices[vertexIndex + 2].x) / 2, charInfo.baseLine);
            // Determine the center point of each character.
            Vector2 charMidBaseline = (sourceVertices[vertexIndex + 0] + sourceVertices[vertexIndex + 2]) / 2;

            // Need to translate all 4 vertices of each quad to aligned with middle of character / baseline.
            // This is needed so the matrix TRS is applied at the origin for each character.
            Vector3 offset = charMidBaseline;

            Vector3[] destinationVertices = textInfo.meshInfo[materialIndex].vertices;

            destinationVertices[vertexIndex + 0] = sourceVertices[vertexIndex + 0] - offset;
            destinationVertices[vertexIndex + 1] = sourceVertices[vertexIndex + 1] - offset;
            destinationVertices[vertexIndex + 2] = sourceVertices[vertexIndex + 2] - offset;
            destinationVertices[vertexIndex + 3] = sourceVertices[vertexIndex + 3] - offset;

            Vector3 jitterOffset = new Vector3(Random.Range(-.25f, .25f), Random.Range(-.25f, .25f), 0);

            var matrix = Matrix4x4.TRS(jitterOffset * _curveScale, Quaternion.Euler(0, 0, Random.Range(-5f, 5f) * _angleMultiplier), Vector3.one);

            destinationVertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 0]);
            destinationVertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 1]);
            destinationVertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 2]);
            destinationVertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 3]);

            destinationVertices[vertexIndex + 0] += offset;
            destinationVertices[vertexIndex + 1] += offset;
            destinationVertices[vertexIndex + 2] += offset;
            destinationVertices[vertexIndex + 3] += offset;
        }
    }
}
