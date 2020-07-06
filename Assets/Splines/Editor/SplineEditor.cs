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

            Curve firstCurve = spline.Curves.FirstOrDefault((item) => item.End == node);
            Curve lastCurve = spline.Curves.FirstOrDefault((item) => item.Start == node);

            int firstIndex = spline.Curves.IndexOf(firstCurve);

            if (firstCurve != null)
                spline.Curves.RemoveAt(firstIndex);
            if (lastCurve != null)
                spline.Curves.Remove(lastCurve);

            if (firstCurve == null || lastCurve == null) return;

            Curve newCurve = new Curve(firstCurve.Start, lastCurve.End);
            spline.Curves.Insert(firstIndex, newCurve);
            SceneView.RepaintAll();
        }

        private static void HalveCurve(int index, IList<Curve> curves)
        {
            float distance = curves[index].Length / 2;
            CurveNode middle =
                new CurveNode(
                    curves[index].GetPositionAtDistance(distance),
                    curves[index].GetRotationAtDistance(distance));

            Curve lowHalf = new Curve(curves[index].Start, middle);
            Curve highHalf = new Curve(middle, curves[index].End);

            curves.Insert(index, highHalf);
            curves.Insert(index, lowHalf);
            curves.RemoveAt(index + 2);

            SceneView.RepaintAll();
        }

        private static void AddHandle(Spline spline, CurveNode node, CurveNode.HandleRelation relation)
        {
            const float HANDLE_SCALE = 2f;

            spline.GetTransformAtDistance(spline.GetDistanceOfNode(node),
                out Vector3 position, out Quaternion rotation);

            Vector3 normal = rotation * Vector3.forward;
            normal.Normalize();

            Vector3? handlePosition = null;
            if (relation == CurveNode.HandleRelation.Before)
                handlePosition = position - (normal * HANDLE_SCALE);
            else if (relation == CurveNode.HandleRelation.After)
                handlePosition = position + (normal * HANDLE_SCALE);

            node.SetHandle(relation, handlePosition);
                
            SceneView.RepaintAll();
        }

        private static void BreakSpline(Spline spline, CurveNode startNode)
        {
            Curve startCurve = spline.Curves.First((item) => startNode == item.Start);
            int startCurveIndex = spline.Curves.IndexOf(startCurve);

            var newNode = new CurveNode(startNode);
            startCurve.Start = newNode;

            startNode.AfterHandle = null;
            newNode.BeforeHandle = null;

            Curve[] curves = new Curve[spline.Curves.Count];
            spline.Curves.CopyTo(curves, 0);

            // Reorder curves so that the break is between the start and end of Curves.
            int splineIndex = 0;
            int localIndex = startCurveIndex;
            do
            {
                // Iterate until the localIndex wraps around to its original value.
                spline.Curves[splineIndex] = curves[localIndex];

                splineIndex++;
                // Wrap around when the spline index reached the end of Curves.
                if ((localIndex + 1) < curves.Length)
                    localIndex++;
                else
                    localIndex = 0;
            } while (localIndex != startCurveIndex);
        }

        [MenuItem("GameObject/3D Object/Spline")]
        public static void CreateEmptySpline()
        {
            var spline = new GameObject("Spline", typeof(Spline));
            Selection.activeObject = spline;
        }
    }
}
