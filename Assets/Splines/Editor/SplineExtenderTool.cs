using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;

namespace Splines
{
    [EditorTool("Spline Prepend Extender Tool", typeof(Spline))]
    class SplinePrependExtenderTool : SplineExtenderTool
    {
        public override GUIContent toolbarIcon
        {
            get => new GUIContent(Resources.Load<Texture2D>("Splines/PrependDrawingTool"));
        }

        protected override CurveNode.HandleRelation GetDragRelation() => CurveNode.HandleRelation.Before;

        protected override CurveNode GetNewNode(Spline spline)
        {
            CurveNode newNode = new CurveNode(
                MouseUtils.GetWorldMousePosition(spline), Quaternion.identity, 
                null, null, CurveNode.HandleConstraintType.Symmetric);

            if (spline.Nodes.Count == 0)
                spline.Nodes.Add(newNode);
            else
                spline.Nodes.Insert(0, newNode);
                
            return newNode;
        }
    }

    [EditorTool("Spline Postpend Extender Tool", typeof(Spline))]
    class SplinePostpendExtenderTool : SplineExtenderTool
    {
        public override GUIContent toolbarIcon
        {
            get => new GUIContent(Resources.Load<Texture2D>("Splines/PostpendDrawingTool"));
        }

        protected override CurveNode.HandleRelation GetDragRelation() => CurveNode.HandleRelation.After;

        protected override CurveNode GetNewNode(Spline spline)
        {
            CurveNode newNode = new CurveNode(
                MouseUtils.GetWorldMousePosition(spline), Quaternion.identity, 
                null, null, CurveNode.HandleConstraintType.Symmetric);

            spline.Nodes.Add(newNode);

            return newNode;
        }
    }

    
    abstract class SplineExtenderTool : EditorTool
    {
        /// <summary>
        /// Gets a new node attached to any relevant existing nodes. The position of this node will be overwritten.
        /// </summary>
        protected abstract CurveNode GetNewNode(Spline spline);

        /// <summary>
        /// Gets the handle relation to be used when dragging to create handles.
        /// </summary>
        protected abstract CurveNode.HandleRelation GetDragRelation();

        protected CurveNode lastNewNode = null;

        public override void OnToolGUI(EditorWindow window)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            var go = target as GameObject;
            if (go == null)
                return;

            Spline activeSpline = go.GetComponent<Spline>();
            if (activeSpline == null)
                return;

            if (Event.current.type == EventType.MouseDown &&
                Event.current.button == 0)
            {
                lastNewNode = GetNewNode(activeSpline);
                Event.current.Use();
                window.Repaint();
            }
                

            if (Event.current.type == EventType.MouseDrag)
            {
                if (activeSpline != null &&
                    lastNewNode != null)
                {
                    // Create the other handle if it doesn't already exist.
                    if (!lastNewNode.GetHandle(CurveNode.GetOtherRelation(GetDragRelation())).HasValue)
                        lastNewNode.SetHandle(CurveNode.GetOtherRelation(GetDragRelation()), Vector3.zero);

                    // Add a new handle at the mouse position.
                    Vector3 handlePosition = MouseUtils.GetWorldMousePosition(activeSpline) - lastNewNode.Position;
                    lastNewNode.SetHandle(GetDragRelation(), handlePosition);

                    Event.current.Use();
                }
            }
        }
    }
}