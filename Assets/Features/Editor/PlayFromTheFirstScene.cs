using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MagicSwords.Features.Editor
{
    internal static class PlayFromTheFirstScene
    {
        private static string EditModeScenesKey { get; } = $"{nameof(PlayFromTheFirstScene)}.{nameof(EditModeScenes)}";

        private static IQueryable<string> EditModeScenes
        {
            set => EditorPrefs.SetString(EditModeScenesKey, string.Join("|", value));
            get => EditorPrefs.GetString(EditModeScenesKey, string.Empty).Split('|').AsQueryable();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void LoadFirstSceneAtGameBegins()
        {
            if (PlayFromTheFirstSceneMenu.PlayFromFirstScene)
            {
                EditorApplication.playModeStateChanged += SwitchPlayModeScenes;
            }
            else
            {
                EditorApplication.playModeStateChanged -= SwitchPlayModeScenes;
            }

            if (EditorBuildSettings.scenes.Length is 0)
            {
                Debug.LogWarning("The scene build list is empty. Can't play from first scene.");
            }
        }

        private static void SwitchPlayModeScenes(PlayModeStateChange nextState)
        {
            var scenes = EditorSceneManagerUtility.GetAllScenes
                .Where(scene => scene.isLoaded);

            if (scenes.Count() <= 1 && scenes.First().buildIndex < 0) return;

            if (nextState is PlayModeStateChange.ExitingEditMode)
            {
                SaveEditModeScenes(scenes);
            }
            else if (nextState is PlayModeStateChange.EnteredEditMode)
            {
                RestoreEditModeScenes();
            }
        }

        private static void SaveEditModeScenes(IQueryable<Scene> scenes)
        {
            EditModeScenes = scenes.Select(scene => scene.path);

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);
            }
            else
            {
                EditorApplication.isPlaying = false;
            }
        }

        private static void RestoreEditModeScenes()
        {
            EditorSceneManager.OpenScene(EditModeScenes.First(), OpenSceneMode.Single);

            foreach (var scene in EditModeScenes.Skip(1))
            {
                EditorSceneManager.OpenScene(scene, OpenSceneMode.Additive);
            }
        }
    }
}
