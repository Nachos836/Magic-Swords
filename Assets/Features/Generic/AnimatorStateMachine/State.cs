using System;
using UnityEngine;
using UnityEngine.Animations;

namespace MagicSwords.Features.Generic.AnimatorStateMachine
{
    public abstract class State : StateMachineBehaviour
    {
        private bool _isInitialized;
		private bool _isFirstEnter = true;

        protected event Action? FirstTimeEntered;
        protected event Action? Entered;
        protected event Action? Updated;
        protected event Action? Exited;

        internal void Construct()
        {
            AssertIsNotInitialized();

            _isInitialized = true;
        }

        public sealed override void OnStateEnter
        (
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex,
            AnimatorControllerPlayable controller
        ) {
            AssertIsInitialized();

            base.OnStateEnter(animator, stateInfo, layerIndex, controller);

            if (_isFirstEnter)
            {
                _isFirstEnter = false;

                FirstTimeEntered?.Invoke();
            }
            Entered?.Invoke();
        }

        public sealed override void OnStateUpdate
        (
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex,
            AnimatorControllerPlayable controller
        ) {
            AssertIsInitialized();

            base.OnStateUpdate(animator, stateInfo, layerIndex, controller);

            Updated?.Invoke();
        }

        public sealed override void OnStateExit
        (
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex,
            AnimatorControllerPlayable controller
        ) {
            AssertIsInitialized();

            base.OnStateExit(animator, stateInfo, layerIndex, controller);

            Exited?.Invoke();
        }

        public sealed override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
        }

        public sealed override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
        }

        public sealed override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);
        }

        private void AssertIsInitialized()
        {
            if (_isInitialized is false) throw new InvalidOperationException($"{GetType().Name} is not initialized");
        }

        private void AssertIsNotInitialized()
        {
            if (_isInitialized) throw new InvalidOperationException($"{GetType().Name} is already initialized");
        }
    }
}
