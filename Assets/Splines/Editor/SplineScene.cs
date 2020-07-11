using System;
using UnityEditor;
using UnityEngine;

namespace Splines
{
    public partial class SplineEditor : Editor
    {
        private void DrawSceneHandle(CurveNode parent, CurveNode.HandleRelation relation)
        {
            Vector3? handle = parent.GetHandle(relation);
            if (!handle.HasValue) return;

            Vector3 worldPosition = handle.Value + parent.Position;

            Handles.color = handleColor;
            if (Handles.Button(worldPosition, Quaternion.identity,
                HandleUtility.GetHandleSize(worldPosition) * .1f, 
                HandleUtility.GetHandleSize(worldPosition) * .1f, 
                Handles.CubeHandleCap))
                SelectNodeHandle(parent, relation);

            if (selectedNode == parent && 
                selectedHandleRelation == relation)
            {
                DrawTooledHandle(ref worldPosition, Quaternion.identity);
                parent.SetHandle(relation, worldPosition - parent.Position);
            }
        }

        private void SelectNodeHandle(CurveNode node, CurveNode.HandleRelation relation)
        {
            if (selectedNode == node && selectedHandleRelation == relation)
            {
                // Deselect the currently selected node/ handle if it is already selected.
                selectedNode = null;
                selectedHandleRelation = CurveNode.HandleRelation.None;
            }
            else
            {
                // Select the target node/ handle if it isn't already selected.
                selectedNode = node;
                selectedHandleRelation = relation;
            }
            
            Repaint();
        }

        private void DrawSceneNode(CurveNode node)
        {
            Handles.color = nodeColor;
            if (Handles.Button(node.Position, node.Rotation,
                HandleUtility.GetHandleSize(node.Position) * .2f, 
                HandleUtility.GetHandleSize(node.Position) * .2f, 
                Handles.CubeHandleCap))
                SelectNodeHandle(node, CurveNode.HandleRelation.None);
                

            DrawSceneHandle(node, CurveNode.HandleRelation.Before);
            DrawSceneHandle(node, CurveNode.HandleRelation.After);

            if (selectedNode == node)
            {
                Vector3 position = node.Position; 
                Quaternion rotation = node.Rotation;
                
                if (selectedHandleRelation == CurveNode.HandleRelation.None)
                    DrawTooledHandle(ref position, ref rotation);

                if (node.Position != position) node.Position = position;
                if (node.Rotation != rotation) node.Rotation = rotation;
            }
        }

        private void OnSceneGUI()
        {
            var spline = target as Spline;

            foreach (var node in spline.Nodes)
                DrawSceneNode(node);
        }
    }
}