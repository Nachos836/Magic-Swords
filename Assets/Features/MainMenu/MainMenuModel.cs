using UnityEditor;
using UnityEngine.SceneManagement;
using ZBase.Foundation.Mvvm.ComponentModel;

namespace MagicSwords.Features.MainMenu
{
    internal sealed class MainMenuModel
    {
        public void ApplicationExitHandler(in PropertyChangeEventArgs args)
        {
#       if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#       else
            Application.Quit();
#       endif
        }

        public void ApplicationRestartHandler(in PropertyChangeEventArgs args)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}