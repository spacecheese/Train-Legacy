using System;
using UnityEngine;

namespace Splines
{
    public class CurveNode
    {
        public enum HandleConstraintType { None, Aligned, Symmetric }
        public enum HandleRelation { None, Before, After }

        private Vector3? beforeHandle;
        /// <summary>
        /// Position before the node in node local space.
        /// </summary>
        public Vector3? BeforeHandle {
            get { return beforeHandle; }
            set { beforeHandle = value; OnHandleChanged(ref afterHandle, beforeHandle); }
        }

        private Vector3? afterHandle;
        /// <summary>
        /// Position after the node in node local space.
        /// </summary>
        public Vector3? AfterHandle 
        {
            get { return afterHandle; }
            set { afterHandle = value; OnHandleChanged(ref beforeHandle, afterHandle); }
        }

        /// <summary>
        /// Returns the opposite relation to that provided (ie Before when After is supplied)
        /// </summary>
        public static HandleRelation GetOtherRelation(HandleRelation relation)
        {
            switch (relation)
            {
                case HandleRelation.After:
                    return HandleRelation.Before;
                case HandleRelation.Before:
                    return HandleRelation.After;
                default:
                    return HandleRelation.None;
            }
        }

        /// <summary>
        /// Gets the handle corresponding to the specified relation.
        /// </summary>
        public Vector3? GetHandle(HandleRelation relation)
        {
            switch (relation)
            {
                case HandleRelation.After:
                    return AfterHandle;
                case HandleRelation.Before:
                    return BeforeHandle;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Sets the handle corresponding to the specified relation.
        /// </summary>
        public void SetHandle(HandleRelation relation, Vector3? newValue)
        {
            switch (relation)
            {
                case HandleRelation.Before:
                    BeforeHandle = newValue; return;
                case HandleRelation.After:
                    AfterHandle = newValue; return;
                default:
                    return;
            };
        }

        private Vector3 position;
        /// <summary>
        /// The position of the curve node in world space.
        /// </summary>
        public Vector3 Position {
            get { return position; } 
            set { position = value; OnNodeChanged(); }
        }

        private Quaternion rotation;
        /// <summary>
        /// The rotation of the curve node in world space.
        /// </summary>
        public Quaternion Rotation {
            get { return rotation; }
            set {
                if (beforeHandle.HasValue)
                    beforeHandle = beforeHandle.Value.RotateAbout(Vector3.zero, rotation, value);
                if (afterHandle.HasValue)
                    afterHandle = afterHandle.Value.RotateAbout(Vector3.zero, rotation, value);

                rotation = value; 
                OnNodeChanged(); 
            }
        }

        private HandleConstraintType handleConstraint;
        /// <summary>
        /// Indicates how the handles should be constrained to one another.
        /// </summary>
        public HandleConstraintType HandleConstraint {
            get { return handleConstraint; }
            set { handleConstraint = value; OnHandleChanged(ref beforeHandle, afterHandle); }
        }

        public event EventHandler NodeChanged;

        /// <summary>
        /// Rotate the node to align with the normal of the handles if possible.
        /// </summary>
        private void AlignRotation()
        {
            if (HandleConstraint == HandleConstraintType.None) return;
            if (!AfterHandle.HasValue || !BeforeHandle.HasValue) return;
            if (AfterHandle.Value == BeforeHandle.Value) return;

            Vector3 up = Rotation * Vector3.up;
            Vector3 forward = AfterHandle.Value - BeforeHandle.Value;

            rotation = Quaternion.LookRotation(forward, up);
        }

        /// <summary>
        /// Realign the handles specified to conform to the current <see cref="this.HandleConstraint"/>.
        /// </summary>
        /// <param name="mover">The handle to move during positioning.</param>
        /// <param name="target">The handle to keep still during positioning.</param>
        private void PositionHandles(ref Vector3 mover, Vector3 target)
        {
            switch (HandleConstraint)
            {
                case HandleConstraintType.Aligned:
                    mover = Vector3.RotateTowards(mover, -target, 4f, 0f);
                    return;
                case HandleConstraintType.Symmetric:
                    mover = -target;
                    return;
                case HandleConstraintType.None:
                default:
                    return;
            }
        }

        private void OnHandleChanged(ref Vector3? movedHandle, Vector3? otherHandle)
        {
            if (movedHandle.HasValue && otherHandle.HasValue)
            {
                // If both handles are set reposition the handles to match HandleConstraint.
                Vector3 movedHandleValue = movedHandle.Value;
                PositionHandles(ref movedHandleValue, otherHandle.Value);
                movedHandle = movedHandleValue;
            }

            AlignRotation();
            OnNodeChanged();
        }

        private void OnNodeChanged()
        {
            NodeChanged?.Invoke(this, new EventArgs());
        }

        public CurveNode(CurveNode node) : this(node.position, node.rotation, node.beforeHandle, node.afterHandle, node.handleConstraint) { }

        public CurveNode(Vector3 position, Quaternion rotation,
            Vector3? beforeHandle = null, Vector3? afterHandle = null, 
            HandleConstraintType handleConstraint = HandleConstraintType.None)
        {
            this.position = position;
            this.rotation = rotation;

            this.beforeHandle = beforeHandle;
            this.afterHandle = afterHandle;
            this.handleConstraint = handleConstraint;

            // Align handles.
            OnHandleChanged(ref beforeHandle, afterHandle);
        }
    }
}
