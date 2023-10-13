using System;
using UnityEngine;
using UnityEngine.Animations;

namespace MagicSwords.Features.Generic.AnimatorStateMachine
{
    public abstract class State : StateMachineBehaviour
    {
        protected StateMachine.IController Controller;
        private bool _isInitialized;
		private bool _isFirstEnter = true;

        protected event Action FirstTimeEntered;
        protected event Action Entered;
        protected event Action Updated;
        protected event Action Exited;

        internal void Construct(StateMachine.IController controller)
        {
            if (AssertIsNotInitialized() is false) return;

            Controller = controller;

            _isInitialized = true;
        }

        public sealed override void OnStateEnter
        (
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex,
            AnimatorControllerPlayable controller
        ) {
            if (AssertIsInitialized() is false) return;

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
            if (AssertIsInitialized() is false) return;

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
            if (AssertIsInitialized() is false) return;

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

        private bool AssertIsInitialized()
        {
            return _isInitialized
                ? true
                : throw new InvalidOperationException($"{GetType().Name} is not initialized");
        }

        private bool AssertIsNotInitialized()
        {
            return _isInitialized is false
                ? true
                : throw new InvalidOperationException($"{GetType().Name} is already initialized");
        }
    }
}
