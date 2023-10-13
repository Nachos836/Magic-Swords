using System;
using System.Linq;
using UnityEngine;

namespace MagicSwords.Features.Generic.AnimatorStateMachine
{
    [RequireComponent(typeof(Animator))]
    public sealed class StateMachine : MonoBehaviour, StateMachine.IController
    {
        [SerializeField] [HideInInspector] private Animator _animator;

        public Animator FinalStateMachine => _animator ??= GetComponent<Animator>();

        private void OnValidate() => _animator = GetComponent<Animator>();

        public void Construct()
        {
            if (Initialized)
            {
                throw new InvalidOperationException($"{GetType().Name} is already initialized");
            }

            foreach (var state in _animator.GetBehaviours<State>())
            {
                state.Construct(this);
            }

            Initialized = true;
        }

        private bool Initialized { get; set; }
        private bool NotInitialized => Initialized is false;

        private bool DoesParameterExist(AnimatorControllerParameterType type, string candidate)
        {
            return _animator.parameters.Any(parameter => parameter.type == type && parameter.name == candidate);
        }

        void IController.Set(string trigger)
        {
            if (NotInitialized)
            {
                throw new InvalidOperationException($"{GetType().Name} is not initialized");
            }

            if (DoesParameterExist(AnimatorControllerParameterType.Trigger, trigger) is false)
            {
                throw new UnityException($"{_animator.GetScenePath()} Trigger {trigger} not found!");
            }

            _animator.SetTrigger(trigger);
        }

        void IController.Set(string field, bool value)
        {
            if (NotInitialized)
            {
                throw new InvalidOperationException($"{GetType().Name} is not initialized");
            }

            if (DoesParameterExist(AnimatorControllerParameterType.Bool, field) is false)
            {
                throw new UnityException($"{_animator.GetScenePath()} Bool {field} not found!");
            }

            _animator.SetBool(field, value);
        }

        void IController.Set(string field, int value)
        {
            if (NotInitialized)
            {
                throw new InvalidOperationException($"{GetType().Name} is not initialized");
            }

            if (DoesParameterExist(AnimatorControllerParameterType.Int, field) is false)
            {
                throw new UnityException($"{_animator.GetScenePath()} Int {field} not found!");
            }

            _animator.SetInteger(field, value);
        }

        void IController.Set(string field, float value)
        {
            if (NotInitialized)
            {
                throw new InvalidOperationException($"{GetType().Name} is not initialized");
            }

            if (DoesParameterExist(AnimatorControllerParameterType.Float, field) is false)
            {
                throw new UnityException($"{_animator.GetScenePath()} Float {field} not found!");
            }

            _animator.SetFloat(field, value);
        }

        public interface IController
        {
            Animator FinalStateMachine { get; }
            void Set(string field, bool value);
            void Set(string field, float value);
            void Set(string field, int value);
            void Set(string trigger);
        }
    }

    internal static class AnimatorExtensions
    {
        public static string GetScenePath(this Animator _) => string.Empty;
    }
}
