using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

namespace MagicSwords.Features.UnityEditorUtils
{
    internal static class EditorSceneManagerUtility
    {
        public static IQueryable<Scene> GetAllScenes
        {
            get
            {
                return GetAllScenesInternal().AsQueryable();

                IEnumerable<Scene> GetAllScenesInternal()
                {
                    for (var index = 0; index < SceneManager.sceneCount; index++)
                    {
                        yield return SceneManager.GetSceneAt(index);
                    }
                }
            }
        }
    }
}
