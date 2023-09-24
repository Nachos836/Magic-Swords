using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;

namespace MagicSwords.Features.Logger.Loggers
{
    internal sealed class VoidLogger : ILogger
    {
        [StringFormatMethod("format")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ILogger.LogInformation(Object context, string format, params object[] arguments) { }

        [StringFormatMethod("format")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ILogger.LogWarning(Object context, string format, params object[] arguments) { }

        [StringFormatMethod("format")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ILogger.LogError(Object context, string format, params object[] arguments) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ILogger.LogException(Object context, System.Exception exception) { }
    }
}