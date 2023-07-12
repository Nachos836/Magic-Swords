using UnityEngine;
using VContainer;
using ZBase.Foundation.Mvvm.ComponentModel;
using ZBase.Foundation.Mvvm.Input;

namespace MagicSwords.Features.MainMenu
{
    public partial class MainMenuViewModel : MonoBehaviour, IObservableObject
    {
        [ObservableProperty] private bool _playing;
        [ObservableProperty] private bool _exitNeeded;
        [ObservableProperty] private bool _restartNeeded;

        private MainMenuModel _model;

        [Inject] internal void Construct(MainMenuModel model) => _model = model;

        private void OnEnable()
        {
            _onChangedExitNeeded += _model.ApplicationExitHandler;
            _onChangedRestartNeeded += _model.ApplicationRestartHandler;
        }

        private void OnDisable()
        {
            _onChangedExitNeeded -= _model.ApplicationExitHandler;
            _onChangedRestartNeeded -= _model.ApplicationRestartHandler;
        }

        [RelayCommand] private void OnSetPlayState() => Playing = !Playing;
        [RelayCommand] private void OnSetExit() => ExitNeeded = !ExitNeeded;
        [RelayCommand] private void OnSetRestart() => RestartNeeded = !RestartNeeded;
    }
}
