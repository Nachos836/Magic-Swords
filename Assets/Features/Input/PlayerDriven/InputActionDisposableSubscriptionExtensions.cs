using System;
using UnityEngine.InputSystem;

namespace MagicSwords.Features.Input.PlayerDriven
{
    using static Generic.Extensions.DisposableSubscription;

    internal static class InputActionDisposableSubscriptionExtensions
    {
        public static IDisposable SubscribeStarted(this InputAction self, Action<InputAction.CallbackContext> income)
        {
            self.started += income;

            return new SubscriptionHandler(() => self.started -= income);
        }

        public static IDisposable SubscribePerformed(this InputAction self, Action<InputAction.CallbackContext> income)
        {
            self.performed += income;

            return new SubscriptionHandler(() => self.performed -= income);
        }

        public static IDisposable SubscribeCanceled(this InputAction self, Action<InputAction.CallbackContext> income)
        {
            self.canceled += income;

            return new SubscriptionHandler(() => self.canceled -= income);
        }
    }
}