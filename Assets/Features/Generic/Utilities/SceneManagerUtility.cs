using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

namespace MagicSwords.Features.Generic.Utilities
{
    internal static class SceneManagerUtility
    {
        public static IQueryable<Scene> GetAllScenes
        {
            get
            {
                return GetAllScenes().AsQueryable();

                IEnumerable<Scene> GetAllScenes()
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
