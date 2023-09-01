using UnityEditor;
using UnityEngine;

namespace MagicSwords.Editor
{
    internal static class PlayFromTheFirstSceneMenu
    {
        private const string PlayFromFirstMenuStr = "Edit/Always Start From Application Entry Scene #&p";

        internal static bool PlayFromFirstScene
        {
            get => EditorPrefs.HasKey(PlayFromFirstMenuStr)
                   && EditorPrefs.GetBool(PlayFromFirstMenuStr);

            private set => EditorPrefs.SetBool(PlayFromFirstMenuStr, value);
        }

        [MenuItem(PlayFromFirstMenuStr, false, 150)]
        private static void PlayFromFirstSceneCheckMenu() 
        {
            PlayFromFirstScene = !PlayFromFirstScene;
            Menu.SetChecked(PlayFromFirstMenuStr, PlayFromFirstScene);

            var msg = PlayFromFirstScene
                ? "Play from Application Entry"
                : "Play from current scene";

            if (Resources.FindObjectsOfTypeAll<SceneView>().Length > 0)
            {
                EditorWindow.GetWindow<SceneView>().ShowNotification(new GUIContent(msg));
            }
            else
            {
                Debug.Log(msg);
            }
        }

        [MenuItem(PlayFromFirstMenuStr, isValidateFunction: true)]
        private static bool PlayFromFirstSceneCheckMenuValidate()
        {
            Menu.SetChecked(PlayFromFirstMenuStr, PlayFromFirstScene);

            return true;
        }
    }
}
