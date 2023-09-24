using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;

namespace MagicSwords.Features.Logger.Loggers
{
    internal sealed class UnityBasedLogger : ILogger
    {
        [StringFormatMethod("format")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ILogger.LogInformation(Object context, string format, params object[] arguments)
        {
            Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, context, format, arguments);
        }

        [StringFormatMethod("format")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ILogger.LogWarning(Object context, string format, params object[] arguments)
        {
            Debug.LogFormat(LogType.Warning, LogOption.NoStacktrace, context, format, arguments);
        }

        [StringFormatMethod("format")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ILogger.LogError(Object context, string format, params object[] arguments)
        {
            Debug.LogFormat(LogType.Error, LogOption.None, context, format, arguments);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ILogger.LogException(Object context, System.Exception exception)
        {
            Debug.LogException(exception, context);
        }
    }
}