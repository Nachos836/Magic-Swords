using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MagicSwords.Features.Editor
{
    internal static class PlayFromTheFirstScene
    {
        private const string PlayFromFirstMenuStr = "Edit/Always Start From Application Entry Scene &p";

        private static bool PlayFromFirstScene
        {
            get => EditorPrefs.HasKey(PlayFromFirstMenuStr)
                && EditorPrefs.GetBool(PlayFromFirstMenuStr);

            set => EditorPrefs.SetBool(PlayFromFirstMenuStr, value);
        }

        [MenuItem(PlayFromFirstMenuStr, false, 150)]
        private static void PlayFromFirstSceneCheckMenu() 
        {
            PlayFromFirstScene = !PlayFromFirstScene;
            Menu.SetChecked(PlayFromFirstMenuStr, PlayFromFirstScene);

            ShowNotifyOrLog(PlayFromFirstScene ? "Play from Application Entry" : "Play from current scene");
        }

        [MenuItem(PlayFromFirstMenuStr, isValidateFunction: true)]
        private static bool PlayFromFirstSceneCheckMenuValidate()
        {
            Menu.SetChecked(PlayFromFirstMenuStr, PlayFromFirstScene);

            return true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static async void LoadFirstSceneAtGameBegins()
        {
            if (PlayFromFirstScene is false) return;

            if (EditorBuildSettings.scenes.Length  == 0)
            {
                Debug.LogWarning("The scene build list is empty. Can't play from first scene.");

                return;
            }

            foreach (var candidate in Object.FindObjectsOfType<GameObject>())
            {
                candidate.SetActive(false);
            }

            await SceneManager.LoadSceneAsync(0);
        }

        private static void ShowNotifyOrLog(string msg)
        {
            if (Resources.FindObjectsOfTypeAll<SceneView>().Length > 0)
            {
                EditorWindow.GetWindow<SceneView>().ShowNotification(new GUIContent(msg));
            }
            else
            {
                Debug.Log(msg);
            }
        }
    }
}