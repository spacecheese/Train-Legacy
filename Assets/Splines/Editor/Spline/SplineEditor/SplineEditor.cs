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
        private readonly Color selectedNodeColor = Color.Lerp(Color.white, Color.green, .3f);
        private readonly Color handleColor = Color.blue;
        private readonly Color selectedHandleColor = Color.Lerp(Color.white, Color.blue, .3f);

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
            var curve = spline.Curves[index];
            CurveNode middle =
                new CurveNode(curve.GetPositionAtDistance(distance), curve.GetAngleAtDistance(distance));

            spline.Nodes.Insert(index + 1, middle);
            SceneView.RepaintAll();
        }

        private static void AddHandle(Spline spline, CurveNode node, CurveNode.HandleRelation relation)
        {
            if (node.BeforeHandle.HasValue && relation == CurveNode.HandleRelation.After)
            {
                // Reflect an existing handle if one exists.
                node.AfterHandle = -node.BeforeHandle;
            }
            else if (node.AfterHandle.HasValue && relation == CurveNode.HandleRelation.Before)
            {
                node.BeforeHandle = -node.AfterHandle;
            }
            else
            {
                const float HANDLE_DISTANCE_FACTOR = .2f;

                Curve beforeCurve = spline.GetRelatedCurveFromNode(node, Curve.NodeRelation.End);
                Curve afterCurve = spline.GetRelatedCurveFromNode(node, Curve.NodeRelation.Start);

                Vector3? beforePosition, afterPosition;
                Vector3 handle = Vector3.zero;

                beforePosition = beforeCurve?.GetPositionAtDistance(beforeCurve.Length - beforeCurve.Length * HANDLE_DISTANCE_FACTOR) - node.Position;
                afterPosition = afterCurve?.GetPositionAtDistance(afterCurve.Length * HANDLE_DISTANCE_FACTOR) - node.Position;

                // Sample either attached curve to find a position for the new handle.
                if (relation == CurveNode.HandleRelation.Before)
                {
                    if (beforePosition.HasValue)
                        handle = beforePosition.Value;
                    else
                        handle = -afterPosition.GetValueOrDefault();
                }
                else if (relation == CurveNode.HandleRelation.After)
                {
                    if (afterPosition.HasValue)
                        handle = afterPosition.Value;
                    else
                        handle = -beforePosition.GetValueOrDefault();
                }
                node.SetHandle(relation, handle);
            }

            SceneView.RepaintAll();
        }

        private static void BreakSpline(Spline spline, CurveNode startNode)
        {
            spline.Break(startNode);
        }

        [MenuItem("GameObject/3D Object/Splines/Spline")]
        public static void CreateEmptySpline()
        {
            var spline = new GameObject("Spline", typeof(Spline));
            Selection.activeObject = spline;
        }
    }
}
