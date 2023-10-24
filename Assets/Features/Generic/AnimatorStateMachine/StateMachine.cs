using System;
using System.Linq;
using UnityEngine;

namespace MagicSwords.Features.Generic.AnimatorStateMachine
{
    [RequireComponent(typeof(Animator))]
    public sealed class StateMachine : MonoBehaviour, StateMachine.IController
    {
        [SerializeField] [HideInInspector] private Animator? _animator;

        private bool _initialized;

        Animator IController.FinalStateMachine => FinalStateMachine;
        private Animator FinalStateMachine => _animator ??= GetComponent<Animator>();

        private void OnValidate() => _animator = GetComponent<Animator>();

        internal void Construct()
        {
            if (_initialized)
            {
                throw new InvalidOperationException($"{GetType().Name} is already initialized");
            }

            foreach (var state in FinalStateMachine.GetBehaviours<State>())
            {
                state.Construct();
            }

            _initialized = true;
        }

        private bool DoesParameterExist(AnimatorControllerParameterType type, string candidate)
        {
            return FinalStateMachine.parameters.Any(parameter => parameter.type == type && parameter.name == candidate);
        }

        void IController.Set(string trigger)
        {
            if (_initialized is false)
            {
                throw new InvalidOperationException($"{GetType().Name} is not initialized");
            }

            if (DoesParameterExist(AnimatorControllerParameterType.Trigger, trigger) is false)
            {
                throw new UnityException($"{FinalStateMachine.GetScenePath()} Trigger {trigger} not found!");
            }

            FinalStateMachine.SetTrigger(trigger);
        }

        void IController.Set(string field, bool value)
        {
            if (_initialized is false)
            {
                throw new InvalidOperationException($"{GetType().Name} is not initialized");
            }

            if (DoesParameterExist(AnimatorControllerParameterType.Bool, field) is false)
            {
                throw new UnityException($"{FinalStateMachine.GetScenePath()} Bool {field} not found!");
            }

            FinalStateMachine.SetBool(field, value);
        }

        void IController.Set(string field, int value)
        {
            if (_initialized is false)
            {
                throw new InvalidOperationException($"{GetType().Name} is not initialized");
            }

            if (DoesParameterExist(AnimatorControllerParameterType.Int, field) is false)
            {
                throw new UnityException($"{FinalStateMachine.GetScenePath()} Int {field} not found!");
            }

            FinalStateMachine.SetInteger(field, value);
        }

        void IController.Set(string field, float value)
        {
            if (_initialized is false)
            {
                throw new InvalidOperationException($"{GetType().Name} is not initialized");
            }

            if (DoesParameterExist(AnimatorControllerParameterType.Float, field) is false)
            {
                throw new UnityException($"{FinalStateMachine.GetScenePath()} Float {field} not found!");
            }

            FinalStateMachine.SetFloat(field, value);
        }

        internal interface IController
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
