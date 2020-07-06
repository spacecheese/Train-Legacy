using System;
using UnityEditor;
using UnityEngine;

namespace Splines
{
    public static class Utils
    {
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
