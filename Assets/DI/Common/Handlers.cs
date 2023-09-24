using System;
using UnityEngine;

namespace MagicSwords.DI.Common
{
    internal static class Handlers
    {
        public static void DefaultExceptionHandler(Exception exception)
        {
#           if DEBUG
            Debug.LogException(exception);
#           else

#           endif
        }
    }
}
