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

        protected override void AttachNode(Spline spline, CurveNode node)
        {
            if (spline.Nodes.Count == 0)
                spline.Nodes.Add(node);
            else
                spline.Nodes.Insert(0, node);
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

        protected override void AttachNode(Spline spline, CurveNode node)
        {
            spline.Nodes.Add(node);
        }
    }
    
    abstract class SplineExtenderTool : EditorTool
    {
        /// <summary>
        /// Gets a new node attached to any relevant existing nodes. The position of this node will be overwritten.
        /// </summary>
        protected abstract void AttachNode(Spline spline, CurveNode node);

        /// <summary>
        /// Gets the handle relation to be used when dragging to create handles.
        /// </summary>
        protected abstract CurveNode.HandleRelation GetDragRelation();

        protected CurveNode lastNewNode = null;
        private bool activeDrag = false;

        private Spline GetActiveSpline()
        {
            var go = target as GameObject;
            if (go == null)
                return null;

            return go.GetComponent<Spline>();
        }

        public override bool IsAvailable()
        {
            return GetActiveSpline() != null;
        }

        public override void OnToolGUI(EditorWindow window)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            var activeSpline = GetActiveSpline();

            if (Event.current.type == EventType.MouseDown &&
                Event.current.button == 0)
            {
                MouseUtils.GetWorldMousePosition(activeSpline, out Vector3 position, out Vector3 normal);
                lastNewNode = new CurveNode(position, Quaternion.AngleAxis(0, normal),
                    null, null, CurveNode.HandleConstraintType.Symmetric);
                AttachNode(activeSpline, lastNewNode);

                Event.current.Use();
                window.Repaint();

                if (Event.current.shift)
                    activeDrag = true;
            }

            if (Event.current.type == EventType.MouseUp)
                activeDrag = false;

            if (Event.current.type == EventType.MouseDrag)
            {
                if (activeSpline != null &&
                    lastNewNode != null &&
                    activeDrag)
                {
                    // Create the other handle if it doesn't already exist.
                    if (!lastNewNode.GetHandle(CurveNode.GetOtherRelation(GetDragRelation())).HasValue)
                        lastNewNode.SetHandle(CurveNode.GetOtherRelation(GetDragRelation()), Vector3.zero);
                    
                    MouseUtils.GetWorldMousePosition(activeSpline, out Vector3 position, out _);
                    // Add a new handle at the mouse position.
                    Vector3? handlePosition = position - lastNewNode.Position;
                    lastNewNode.SetHandle(GetDragRelation(), handlePosition);

                    Event.current.Use();
                }
            }
        }
    }
}