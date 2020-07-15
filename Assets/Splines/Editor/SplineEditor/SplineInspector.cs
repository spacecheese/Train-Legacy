using System.Linq;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace Splines
{
    public partial class SplineEditor : Editor
    {
        GUIContent transformFoldoutContent;
        GUIContent handlesFoldoutContent;
        GUIContent toolsFoldoutContent;

        bool transformFoldout = true;
        bool handlesFoldout = false;
        bool toolsFoldout = false;

        public void OnEnable()
        {
            transformFoldoutContent = EditorGUIUtility.IconContent("Transform Icon");
            transformFoldoutContent.text = "Transform";

            handlesFoldoutContent = EditorGUIUtility.IconContent("EditCollider");
            handlesFoldoutContent.text = " Handles";

            toolsFoldoutContent = EditorGUIUtility.IconContent("Settings");
            toolsFoldoutContent.text = "Tools";
        }

        public override void OnInspectorGUI()
        {
            var spline = target as Spline;

            EditorGUILayout.EditorToolbar(new EditorTool[] { CreateInstance<SplinePrependExtenderTool>(), CreateInstance<SplinePostpendExtenderTool>() });

            if (EditorTools.activeToolType == typeof(SplinePrependExtenderTool) ||
                EditorTools.activeToolType == typeof(SplinePostpendExtenderTool))
                EditorGUILayout.HelpBox("Hold Shift and drag when placing a node to add control handles and a symmetric constraint", MessageType.Info);

            spline.TesselationError = GUIUtils.LogarithmicSlider("Allowed Tesselation Error", spline.TesselationError, .0001f, .2f);
            spline.HighlightSamples = EditorGUILayout.Toggle("Highlight Samples", spline.HighlightSamples);

            EditorGUILayout.Space();

            if (selectedNode == null)
            {
                if (spline.Curves.Count == 0)
                    EditorGUILayout.HelpBox("Create some nodes to display here", MessageType.Info);
                else
                    EditorGUILayout.HelpBox("Select a node or handle to display here", MessageType.Info);
                
                return;
            }
            else
            {
                if (selectedHandleRelation == CurveNode.HandleRelation.None)
                    LayoutSelectedNode();
                else
                    LayoutSelectedHandle();
            }
        }

        private void LayoutSelectedHandle()
        {
            if (selectedNode == null ||
                selectedHandleRelation == CurveNode.HandleRelation.None ||
                !selectedNode.GetHandle(selectedHandleRelation).HasValue) return;

            GUILayout.Label("Selected Handle");
            transformFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(transformFoldout, transformFoldoutContent);
            if (transformFoldout)
            {
                EditorGUI.BeginChangeCheck();
                Vector3 position = EditorGUILayout.Vector3Field("Position", selectedNode.GetHandle(selectedHandleRelation).Value);
                if (EditorGUI.EndChangeCheck())
                {
                    selectedNode.SetHandle(selectedHandleRelation, position);
                    SceneView.RepaintAll();
                }

                EditorGUILayout.Space();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            toolsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(toolsFoldout, toolsFoldoutContent);
            if (toolsFoldout)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Remove Handle"))
                {
                    selectedNode.SetHandle(selectedHandleRelation, null);
                    selectedNode = null;
                }
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void LayoutSelectedNode()
        {
            if (selectedNode == null) return;

            GUILayout.Label("Selected Node");
            var spline = target as Spline;
            transformFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(transformFoldout, transformFoldoutContent);
            if (transformFoldout)
            {
                EditorGUI.BeginChangeCheck();
                Vector3 position = EditorGUILayout.Vector3Field("Position", selectedNode.Position);
                if (EditorGUI.EndChangeCheck())
                {
                    selectedNode.Position = position;
                    SceneView.RepaintAll();
                }

                EditorGUI.BeginChangeCheck();
                Quaternion rotation = Quaternion.Euler(EditorGUILayout.Vector3Field("Rotation", selectedNode.Rotation.eulerAngles));
                if (EditorGUI.EndChangeCheck())
                {
                    selectedNode.Rotation = rotation;
                    SceneView.RepaintAll();
                }

                EditorGUILayout.Space();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            handlesFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(handlesFoldout, handlesFoldoutContent);
            if (handlesFoldout)
            {
                EditorGUI.BeginChangeCheck();
                var constraint = (CurveNode.HandleConstraintType)
                EditorGUILayout.EnumPopup("Handles Constraint", selectedNode.HandleConstraint);
                if (EditorGUI.EndChangeCheck())
                {
                    selectedNode.HandleConstraint = constraint;
                    SceneView.RepaintAll();
                }

                GUILayout.BeginHorizontal();
                GUILayout.Label("Add Handle");

                GUI.enabled = selectedNode.BeforeHandle == null;
                if (GUILayout.Button("Before", GUILayout.Width(65)))
                    AddHandle(spline, selectedNode, CurveNode.HandleRelation.Before);

                GUI.enabled = selectedNode.AfterHandle == null;
                if (GUILayout.Button("After", GUILayout.Width(65)))
                    AddHandle(spline, selectedNode, CurveNode.HandleRelation.After);

                GUI.enabled = true;
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Remove Handle");
                GUI.enabled = selectedNode.BeforeHandle != null;
                if (GUILayout.Button("Before", GUILayout.Width(65)))
                {
                    selectedNode.BeforeHandle = null;
                    SceneView.RepaintAll();
                }

                GUI.enabled = selectedNode.AfterHandle != null;
                if (GUILayout.Button("After", GUILayout.Width(65)))
                {
                    selectedNode.AfterHandle = null;
                    SceneView.RepaintAll();
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();

                EditorGUILayout.Space();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            toolsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(toolsFoldout, toolsFoldoutContent);
            if (toolsFoldout)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Insert Node");

                string endNodeInsertTip = "Use the tools at the top to add nodes to the ends of the spline";

                GUI.enabled = selectedNode != spline.Start;
                var insertNodeBeforeContent = new GUIContent("Before");
                if (!GUI.enabled)
                    insertNodeBeforeContent.tooltip = endNodeInsertTip;
                if (GUILayout.Button(insertNodeBeforeContent, GUILayout.Width(65)))
                {
                    Curve curve = spline.Curves.First((item) => item.End == selectedNode);
                    HalveCurve(spline.Curves.IndexOf(curve), spline);
                    SceneView.RepaintAll();
                }

                GUI.enabled = selectedNode != spline.End;
                var insertNodeAfterContent = new GUIContent("After");
                if (!GUI.enabled)
                    insertNodeAfterContent.tooltip = endNodeInsertTip;
                if (GUILayout.Button(insertNodeAfterContent, GUILayout.Width(65)))
                {
                    Curve curve = spline.Curves.First((item) => item.Start == selectedNode);
                    HalveCurve(spline.Curves.IndexOf(curve), spline);
                    SceneView.RepaintAll();
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                GUI.enabled = spline.IsClosed;
                if (GUILayout.Button("Break Spline", GUILayout.Width(90)))
                {
                    BreakSpline(spline, selectedNode);
                    SceneView.RepaintAll();
                }

                GUI.enabled = !spline.IsClosed;
                if (GUILayout.Button("Close Spline", GUILayout.Width(90)))
                {
                    spline.Close();
                    SceneView.RepaintAll();
                }

                if (GUILayout.Button("Delete Node", GUILayout.Width(90)))
                {
                    RemoveNode(spline, selectedNode);
                    SceneView.RepaintAll();
                }
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}