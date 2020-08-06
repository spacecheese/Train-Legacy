using UnityEditor;
using UnityEngine;

namespace Splines.Deform
{
    [CustomEditor(typeof(Repeater))]
    public class RepeaterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var repeat = target as Repeater;

            if (GUILayout.Button("Refresh"))
            {
                repeat.Refresh();
                SceneView.RepaintAll();
            }

            EditorGUI.BeginChangeCheck();
            Spline spline = EditorGUILayout.ObjectField("Target Spline", repeat.Spline, typeof(Spline), true) as Spline;
            if (EditorGUI.EndChangeCheck())
            {
                repeat.Spline = spline;
                SceneView.RepaintAll();
            }

            EditorGUI.BeginChangeCheck();
            GameObject repeatObject = EditorGUILayout.ObjectField("Repeat Object", repeat.RepeatObject, typeof(GameObject), false) as GameObject;
            if (EditorGUI.EndChangeCheck())
            {
                repeat.RepeatObject = repeatObject;
                SceneView.RepaintAll();
            }

            EditorGUI.BeginChangeCheck();
            float repeatDistance = EditorGUILayout.DelayedFloatField("Target Repeat Distance", repeat.TargetRepeatDistance);
            if (EditorGUI.EndChangeCheck())
            {
                repeat.TargetRepeatDistance = repeatDistance;
                SceneView.RepaintAll();
            }

            EditorGUI.BeginChangeCheck();
            Repeater.Justify padding = (Repeater.Justify)EditorGUILayout.EnumPopup("Padding", repeat.Padding);
            if (EditorGUI.EndChangeCheck())
            {
                repeat.Padding = padding;
                SceneView.RepaintAll();
            }
        }

        [MenuItem("GameObject/3D Object/Splines/Repeater")]
        public static void CreateRepeater()
        {
            var repeater = new GameObject("Repeater", typeof(Repeater));
            Selection.activeObject = repeater;
        }
    }
}
