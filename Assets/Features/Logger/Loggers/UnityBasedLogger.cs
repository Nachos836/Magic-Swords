using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;

namespace MagicSwords.Features.Logger.Loggers
{
    internal sealed class UnityBasedLogger : ILogger, System.IDisposable
    {
        private readonly Object _defaultContext = new ();

        [StringFormatMethod("format")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ILogger.LogInformation(Object context, string format, params object[] arguments)
        {
            Debug.LogFormat(LogType.Log, LogOption.None, context, format, arguments);
        }

        [StringFormatMethod("format")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ILogger.LogInformation(string format, params object[] arguments)
        {
            Debug.LogFormat(LogType.Log, LogOption.None, _defaultContext, format, arguments);
        }

        [StringFormatMethod("format")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ILogger.LogWarning(Object context, string format, params object[] arguments)
        {
            Debug.LogFormat(LogType.Warning, LogOption.None, context, format, arguments);
        }

        [StringFormatMethod("format")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ILogger.LogWarning(string format, params object[] arguments)
        {
            Debug.LogFormat(LogType.Warning, LogOption.None, _defaultContext, format, arguments);
        }

        [StringFormatMethod("format")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ILogger.LogError(Object context, string format, params object[] arguments)
        {
            Debug.LogFormat(LogType.Error, LogOption.None, context, format, arguments);
        }

        [StringFormatMethod("format")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ILogger.LogError(string format, params object[] arguments)
        {
            Debug.LogFormat(LogType.Error, LogOption.None, _defaultContext, format, arguments);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ILogger.LogException(Object context, System.Exception exception)
        {
            Debug.LogException(exception, context);
        }

        void ILogger.LogException(System.Exception exception)
        {
            Debug.LogException(exception);
        }

        void System.IDisposable.Dispose()
        {
#       if UNITY_EDITOR
            Object.DestroyImmediate(_defaultContext);
#       else
            Object.Destroy(_defaultContext);
#       endif
        }

        private sealed class DefaultContext : ScriptableObject { }
    }
}
