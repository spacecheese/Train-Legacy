using UnityEditor;
using UnityEngine;

namespace Splines.Deform
{
    [CustomEditor(typeof(Bender))]
    public class BenderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var bender = target as Bender;

            if (GUILayout.Button("Refresh"))
            {
                bender.Refresh();
                SceneView.RepaintAll();
            }

            EditorGUI.BeginChangeCheck();
            Spline spline = EditorGUILayout.ObjectField("Target Spline", bender.Spline, typeof(Spline), true) as Spline;
            if (EditorGUI.EndChangeCheck())
            {
                bender.Spline = spline;
                SceneView.RepaintAll();
            }

            EditorGUI.BeginChangeCheck();
            Material material = EditorGUILayout.ObjectField("Material", bender.BendingMaterial, typeof(Material), true) as Material;
            if (EditorGUI.EndChangeCheck())
            {
                bender.BendingMaterial = material;
                SceneView.RepaintAll();
            }

            EditorGUI.BeginChangeCheck();
            Mesh mesh = EditorGUILayout.ObjectField("Mesh", bender.BendingMesh, typeof(Mesh), true) as Mesh;
            if (EditorGUI.EndChangeCheck())
            {
                bender.BendingMesh = mesh;
                SceneView.RepaintAll();
            }
        }

        [MenuItem("GameObject/3D Object/Splines/Bender")]
        public static void CreateRepeater()
        {
            var bender = new GameObject("Bender", typeof(Bender));
            Selection.activeObject = bender;
        }
    }
}
