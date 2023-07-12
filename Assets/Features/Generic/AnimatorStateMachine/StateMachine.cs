using System;
using System.Linq;
using UnityEngine;

namespace MagicSwords.Features.Generic.AnimatorStateMachine
{
    [RequireComponent(typeof(Animator))]
    public sealed class StateMachine : MonoBehaviour, StateMachine.IController
    {
        [SerializeField] [HideInInspector] private Animator _animator;

        private bool _isInitialized;
        public Animator FinalStateMachine => _animator;

        private void OnValidate() => _animator = GetComponent<Animator>();

        public void Construct()
        {
            if (AssertIsNotInitialized() is false) return;
 
            foreach (var state in _animator.GetBehaviours<State>())
            {
                state.Construct(this);
            }

            _isInitialized = true;
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

        private bool DoesParameterExist(AnimatorControllerParameterType type, string candidate)
        {
            return _animator.parameters.Any(parameter => parameter.type == type && parameter.name == candidate);
        }

        void IController.Set(string trigger)
        {
            if (!AssertIsInitialized()) return;

            if (!DoesParameterExist(AnimatorControllerParameterType.Trigger, trigger))
            {
                throw new UnityException($"{_animator.GetScenePath()} Trigger {trigger} not found!");
            }
            _animator.SetTrigger(trigger);
        }

        void IController.Set(string field, bool value)
        {
            if (!AssertIsInitialized()) return;

            if (!DoesParameterExist(AnimatorControllerParameterType.Bool, field))
            {
                throw new UnityException($"{_animator.GetScenePath()} Bool {field} not found!");
            }
            _animator.SetBool(field, value);
        }

        void IController.Set(string field, int value)
        {
            if (!AssertIsInitialized()) return;

            if (!DoesParameterExist(AnimatorControllerParameterType.Int, field))
            {
                throw new UnityException($"{_animator.GetScenePath()} Int {field} not found!");
            }
            _animator.SetInteger(field, value);
        }

        void IController.Set(string field, float value)
        {
            if (!AssertIsInitialized()) return;

            if (!DoesParameterExist(AnimatorControllerParameterType.Float, field))
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
        public static string GetScenePath(this Animator _)
        {
            return "";
        }
    }
}