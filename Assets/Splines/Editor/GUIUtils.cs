using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Splines
{
    static class GUIUtils
    {
        public static float LogarithmicSlider(float value, float leftValue, float rightValue, params GUILayoutOption[] options)
        {
            float newValue = value;

            EditorGUILayout.BeginHorizontal(options);

            EditorGUI.BeginChangeCheck();
            float sliderValue = Mathf.Log10(value * (9 + leftValue) / rightValue + 1 - leftValue);
            sliderValue = GUILayout.HorizontalSlider(sliderValue, 0f, 1f, GUILayout.ExpandWidth(true));
            if (EditorGUI.EndChangeCheck())
            {
                float scaledSliderValue = (Mathf.Pow(10, sliderValue) + leftValue - 1) * (rightValue / (9 + leftValue));
                newValue = Mathf.Clamp(scaledSliderValue, leftValue, rightValue);
            }

            EditorGUI.BeginChangeCheck();
            float fieldValue = EditorGUILayout.FloatField(value, GUILayout.MaxWidth(65f));
            if (EditorGUI.EndChangeCheck())
                newValue = Mathf.Clamp(fieldValue, leftValue, rightValue);

            EditorGUILayout.EndHorizontal();

            return newValue;
        }

        public static float LogarithmicSlider(string label, float value, float leftValue, float rightValue, params GUILayoutOption[] options)
        {
            float newValue = value;
            EditorGUILayout.BeginHorizontal(options);
            GUILayout.Label(label);

            EditorGUI.BeginChangeCheck();
            float sliderValue = LogarithmicSlider(value, leftValue, rightValue);
            if (EditorGUI.EndChangeCheck())
                newValue = sliderValue;

            EditorGUILayout.EndHorizontal();
            return newValue;
        }
    }
}
