using System;
using UnityEngine;
using UnityEngine.Animations;

namespace MagicSwords.Features.Generic.AnimatorStateMachine
{
    public abstract class State : StateMachineBehaviour
    {
        protected StateMachine.IController _controller;
        private bool _isInitialized;
		private bool _isFirstEnter = true;

        protected event Action FirstTimeEntered;
        protected event Action Entered;
        protected event Action Updated;
        protected event Action Exited;

        public void Construct(StateMachine.IController controller)
        {
            if (AssertIsNotInitialized() is false) return;

            _controller = controller;

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
            var claim = $"{GetType().Name} is not initialized";
            return _isInitialized ? true : throw new InvalidOperationException(claim);
        }

        private bool AssertIsNotInitialized()
        {
            var claim = $"{GetType().Name} is already initialized";
            return !_isInitialized ? true : throw new InvalidOperationException(claim);
        }
    }
}