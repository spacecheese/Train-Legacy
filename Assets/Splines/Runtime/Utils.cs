using UnityEditor;
using UnityEngine;

namespace Splines
{
    public static class Utils
    {
        /// <summary>
        /// Excecutes <see cref="UnityEngine.Object.Destroy(GameObject)"/> or <see cref="UnityEngine.Object.DestroyImmediate(GameObject)"/> 
        /// depending on the current application configuration.
        /// </summary>
        public static void AutoDestroy(GameObject obj)
        {
#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
                UnityEngine.Object.Destroy(obj);
            else
                UnityEngine.Object.DestroyImmediate(obj);
#else
            UnityEngine.Object.Destroy(obj);
#endif
        }
    }
}
