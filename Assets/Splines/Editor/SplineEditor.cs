using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Splines
{
    [CustomEditor(typeof(Spline))]
    public partial class SplineEditor : Editor
    {
        private CurveNode.HandleRelation selectedHandleRelation = CurveNode.HandleRelation.None;
        private CurveNode selectedNode = null;

        private readonly Color nodeColor = Color.green;
        private readonly Color handleColor = Color.blue;

        private static void DrawTooledHandle(ref Vector3 position, Quaternion rotation)
        {
            if (Tools.current == Tool.Move || Tools.current == Tool.Transform)
            {
                if (Tools.pivotRotation == PivotRotation.Global)
                    rotation = Quaternion.identity;

                position = Handles.PositionHandle(position, rotation);
            }
        }

        private static void DrawTooledHandle(ref Vector3 position, ref Quaternion rotation)
        {
            if (Tools.current == Tool.Move || Tools.current == Tool.Transform)
            {
                if (Tools.pivotRotation == PivotRotation.Global)
                    position = Handles.PositionHandle(position, Quaternion.identity);
                else
                    position = Handles.PositionHandle(position, rotation);
            }


            if (Tools.current == Tool.Rotate || Tools.current == Tool.Transform)
                rotation = Handles.RotationHandle(rotation, position);
        }

        private static void RemoveNode(Spline spline, CurveNode node)
        {
            if (spline.Curves.Count == 1)
            {
                if (EditorUtility.DisplayDialog("Delete spline",
                    "Removing this node will delete the spline",
                    "Ok", "Cancel"))
                    DestroyImmediate(spline.gameObject);

                return;
            }

            spline.Nodes.Remove(node);
            SceneView.RepaintAll();
        }

        private static void HalveCurve(int index, Spline spline)
        {
            float distance = spline.Curves[index].Length / 2;
            CurveNode middle =
                new CurveNode(
                    spline.Curves[index].GetPositionAtDistance(distance),
                    spline.Curves[index].GetRotationAtDistance(distance));

            spline.Nodes.Insert(index + 1, middle);
            SceneView.RepaintAll();
        }

        private static void AddHandle(Spline spline, CurveNode node, CurveNode.HandleRelation relation)
        {
            const float HANDLE_NORMAL_SCALE = 2f;

            Vector3 normal = node.Rotation * Vector3.forward;
            normal.Normalize();

            Vector3? handlePosition = null;
            if (relation == CurveNode.HandleRelation.Before)
                handlePosition = -(normal * HANDLE_NORMAL_SCALE);
            else if (relation == CurveNode.HandleRelation.After)
                handlePosition = normal * HANDLE_NORMAL_SCALE;

            node.SetHandle(relation, handlePosition);
                
            SceneView.RepaintAll();
        }

        private static void BreakSpline(Spline spline, CurveNode startNode)
        {
            spline.Break(startNode);
        }

        [MenuItem("GameObject/3D Object/Spline")]
        public static void CreateEmptySpline()
        {
            var spline = new GameObject("Spline", typeof(Spline));
            Selection.activeObject = spline;
        }
    }
}
