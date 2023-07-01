using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZBase.Foundation.Mvvm.ComponentModel;

namespace MagicSwords.Features.MainMenu
{
    internal sealed class MainMenuModel
    {
        public void ApplicationExitHandler(in PropertyChangeEventArgs args)
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

        public void ApplicationRestartHandler(in PropertyChangeEventArgs args)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
