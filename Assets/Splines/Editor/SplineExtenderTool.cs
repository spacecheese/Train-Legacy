using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;

using static Splines.EnumerableExtensions;

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

            if (spline.Curves.Count > 0)
                spline.Curves.Insert(0, new Curve(newNode, spline.Start, spline.Curves.First(), null));
            else 
                spline.Curves.Add(new Curve(newNode, null));
            
            newNode.Position = MouseUtils.GetWorldMousePosition(spline);
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

            if (spline.Curves.Count > 0)
                spline.Curves.Add(new Curve(newNode, spline.Start, spline.Curves.Last(), null));
            else
                spline.Curves.Add(new Curve(newNode, null));

            newNode.Position = MouseUtils.GetWorldMousePosition(spline);
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
                    Vector3 handlePosition = MouseUtils.GetWorldMousePosition(activeSpline);
                    lastNewNode.SetHandle(GetDragRelation(), handlePosition);

                    Event.current.Use();
                }
            }
        }
    }
}