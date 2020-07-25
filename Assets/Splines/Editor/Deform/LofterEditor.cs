using UnityEngine;
using UnityEditor;

namespace Splines.Deform
{
    [CustomEditor(typeof(Lofter))]
    public class LofterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var loft = target as Lofter;

            if (GUILayout.Button("Refresh"))
            {
                loft.Refresh();
                SceneView.RepaintAll();
            }

            EditorGUI.BeginChangeCheck();
            Spline spline = EditorGUILayout.ObjectField("Target Spline", loft.Spline, typeof(Spline), true) as Spline;
            if (EditorGUI.EndChangeCheck())
            {
                loft.Spline = spline;
                
            }

            EditorGUI.BeginChangeCheck();
            Mesh profile = EditorGUILayout.ObjectField("Target Spline", loft.Profile, typeof(Mesh), true) as Mesh;
            if (EditorGUI.EndChangeCheck())
            {
                loft.Profile = profile;
                SceneView.RepaintAll();
            }
        }

        [MenuItem("GameObject/3D Object/Splines/Lofter")]
        public static void CreateEmptySpline()
        {
            var repeater = new GameObject("Lofter", typeof(Lofter));
            Selection.activeObject = repeater;
        }
    }
}
