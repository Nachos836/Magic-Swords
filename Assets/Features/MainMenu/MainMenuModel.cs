using UnityEditor;
using UnityEngine.SceneManagement;
using ZBase.Foundation.Mvvm.ComponentModel;

namespace MagicSwords.Features.MainMenu
{
    public sealed class MainMenuModel
    {
        public void ApplicationExitHandler(in PropertyChangeEventArgs args)
        {
#       if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#       else
            UnityEngine.Application.Quit();
#       endif
        }

        public void ApplicationRestartHandler(in PropertyChangeEventArgs args)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
