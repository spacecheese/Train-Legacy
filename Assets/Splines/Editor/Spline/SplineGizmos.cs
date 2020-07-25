using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Splines
{
    public class SplineGizmos
    {
        private static readonly Color lineColor = Color.green;
        private static readonly Color nodeColor = Color.green;
        private static readonly Color handleColor = Color.blue;
        private static readonly Color sampleHighlightColor = Color.blue;

        private const float nodeSize = .1f;
        private const float handleSize = .05f;

        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected | GizmoType.Pickable)]
        public static void DrawLineGizmos(Spline spline, GizmoType type)
        {
            DrawDirectionArrow(spline);

            foreach (var curve in spline.Curves)
                DrawCurve(curve, spline.HighlightSamples);

            foreach (var node in spline.Nodes)
                DrawTangent(node);
        }

        [DrawGizmo(GizmoType.NonSelected | GizmoType.Pickable)]
        public static void DrawNodeGizmos(Spline spline, GizmoType type)
        {
            foreach (var node in spline.Nodes)
                DrawNode(node);
        }

        private static void DrawTangent(CurveNode node)
        {
            Gizmos.color = handleColor;
            if (node.BeforeHandle.HasValue)
                Gizmos.DrawLine(node.BeforeHandle.Value + node.Position, node.Position);

            if (node.AfterHandle.HasValue)
                Gizmos.DrawLine(node.AfterHandle.Value + node.Position, node.Position);
        }

        private static void DrawNode(CurveNode node)
        {
            Gizmos.color = nodeColor;
            Gizmos.DrawCube(node.Position, 
                HandleUtility.GetHandleSize(node.Position) * nodeSize * Vector3.one);

            Gizmos.color = handleColor;
            if (node.BeforeHandle.HasValue)
                Gizmos.DrawCube(node.BeforeHandle.Value + node.Position,
                    HandleUtility.GetHandleSize(node.Position) * handleSize * Vector3.one);
                
            if (node.AfterHandle.HasValue)
                Gizmos.DrawCube(node.AfterHandle.Value + node.Position,
                    HandleUtility.GetHandleSize(node.Position) * handleSize * Vector3.one);
        }

        private static void DrawDirectionArrow(Spline spline)
        {
            if (spline.Curves.Count == 0) return;

            Curve curve = spline.Curves.First();

            Vector3 position = curve.GetPositionAtDistance(curve.Length / 2f);
            Quaternion rotation = curve.GetRotationAtDistance(curve.Length / 2f);

            Handles.color = lineColor;
            Handles.ConeHandleCap(0, position, rotation, HandleUtility.GetHandleSize(position) * .2f, EventType.Repaint);
        }

        private static void DrawCurve(Curve curve, bool highlightSamples)
        {
            CurveSample lastSample = default;
            bool isFirstSample = true;

            Gizmos.color = lineColor;
            foreach (var sample in curve.Samples)
            {
                if (!isFirstSample)
                    Gizmos.DrawLine(lastSample.Position, sample.Position);

                lastSample = sample;
                isFirstSample = false;
            }

            Gizmos.color = sampleHighlightColor;
            if (highlightSamples)
                foreach (var sample in curve.Samples)
                    Gizmos.DrawSphere(sample.Position, HandleUtility.GetHandleSize(sample.Position) * .02f);
        }
    }

}
