using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;

namespace MagicSwords.Features.Logger
{
    public interface ILogger
    {
        [StringFormatMethod("format")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void LogInformation(Object context, string format, params object[] arguments);

        [StringFormatMethod("format")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void LogInformation(string format, params object[] arguments);

        [StringFormatMethod("format")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void LogWarning(Object context, string format, params object[] arguments);

        [StringFormatMethod("format")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void LogWarning(string format, params object[] arguments);

        [StringFormatMethod("format")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void LogError(Object context, string format, params object[] arguments);

        [StringFormatMethod("format")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void LogError(string format, params object[] arguments);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void LogException(Object context, System.Exception exception);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void LogException(System.Exception exception);
    }
}
