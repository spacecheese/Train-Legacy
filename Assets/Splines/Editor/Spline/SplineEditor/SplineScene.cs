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

            if (selectedNode == parent &&
                selectedHandleRelation == relation)
            {
                DrawTooledHandle(ref worldPosition, Quaternion.identity);
                parent.SetHandle(relation, worldPosition - parent.Position);

                Handles.color = selectedHandleColor;
            }
            else
                Handles.color = handleColor;

            if (Handles.Button(worldPosition, parent.Rotation,
                HandleUtility.GetHandleSize(worldPosition) * .1f, 
                HandleUtility.GetHandleSize(worldPosition) * .1f, 
                Handles.CubeHandleCap))
                SelectNodeHandle(parent, relation);
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
            if (selectedNode == node &&
                selectedHandleRelation == CurveNode.HandleRelation.None)
            {
                Vector3 position = node.Position;

                DrawTooledHandle(ref position, node.Rotation);
                    
                if (node.Position != position) node.Position = position;

                Handles.color = selectedNodeColor;
            }
            else
                Handles.color = nodeColor;

            if (Handles.Button(node.Position, node.Rotation,
                HandleUtility.GetHandleSize(node.Position) * .2f, 
                HandleUtility.GetHandleSize(node.Position) * .2f, 
                Handles.CubeHandleCap))
                SelectNodeHandle(node, CurveNode.HandleRelation.None);
                

            DrawSceneHandle(node, CurveNode.HandleRelation.Before);
            DrawSceneHandle(node, CurveNode.HandleRelation.After);
        }

        private void OnSceneGUI()
        {
            var spline = target as Spline;

            foreach (var node in spline.Nodes)
                DrawSceneNode(node);
        }
    }
}