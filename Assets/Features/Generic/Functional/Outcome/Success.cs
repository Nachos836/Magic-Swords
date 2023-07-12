using System.Runtime.InteropServices;
using Unity.Burst;

namespace MagicSwords.Features.Generic.Functional.Outcome
{
    [BurstCompile]
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    public readonly struct Success { }
}
