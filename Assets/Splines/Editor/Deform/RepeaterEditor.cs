using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Splines.Deform
{
    [CustomEditor(typeof(Repeater))]
    public class RepeaterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var repeat = target as Repeater;

            EditorGUI.BeginChangeCheck();
            Spline spline = EditorGUILayout.ObjectField("Target Spline", repeat.Spline, typeof(Spline), true) as Spline;
            if (EditorGUI.EndChangeCheck())
                repeat.Spline = spline;

            EditorGUI.BeginChangeCheck();
            GameObject repeatObject = EditorGUILayout.ObjectField("Repeat Object", repeat.RepeatObject, typeof(GameObject), true) as GameObject;
            if (EditorGUI.EndChangeCheck())
                repeat.RepeatObject = repeatObject;

            EditorGUI.BeginChangeCheck();
            float repeatDistance = EditorGUILayout.FloatField("Target Repeat Distance", repeat.TargetRepeatDistance);
            if (EditorGUI.EndChangeCheck())
                repeat.TargetRepeatDistance = repeatDistance;

            EditorGUI.BeginChangeCheck();
            bool padEnds = EditorGUILayout.Toggle("Pad Ends", repeat.PadEnds);
            if (EditorGUI.EndChangeCheck())
                repeat.PadEnds = padEnds;
        }
    }
}
