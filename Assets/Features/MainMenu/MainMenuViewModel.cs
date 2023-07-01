using UnityEditor;
using UnityEngine;
using ZBase.Foundation.Mvvm.ComponentModel;
using ZBase.Foundation.Mvvm.Input;

namespace MagicSwords.Features.MainMenu
{
    internal partial class MainMenuViewModel : MonoBehaviour, IObservableObject
    {
        [ObservableProperty] private bool _playing;

        [RelayCommand]
        private void OnSetPlayState()
        {
            Playing = !Playing;
        }

        [RelayCommand]
        private void OnSetExit()
        {
            if (Application.isEditor)
            {
                EditorApplication.ExitPlaymode();
            }
            else
            {
                Application.Quit();
            }
        }
    }
}
