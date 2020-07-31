using System;
using System.Collections.Generic;
using UnityEngine;

namespace Splines.Deform
{
    /// <summary>
    /// Base class for all MonoBehaviour scipts that associate objects with 
    /// a the curves contained in a user specified spline.
    /// </summary>
    /// <typeparam name="TAttachment">The type to attach to each curve</typeparam>
    [ExecuteAlways]
    [Serializable]
    public abstract class CurveAttacher<TAttachment> : CurveMonitor where TAttachment : class
    {
        public IReadOnlyList<TAttachment> Attachments => attachments;
        private readonly List<TAttachment> attachments = new List<TAttachment>();

        // Implementation defined handlers for managing attachments.
        /// <summary>
        /// Called when a curve is added or inserted into the tracked spline.
        /// </summary>
        protected abstract TAttachment OnBeforeAttachmentAdded(Curve curve);
        /// <summary>
        /// Called when a curve is modified or replaced in the tracked spline.
        /// </summary>
        protected abstract void OnAttachmentChange(Curve curve, TAttachment attachment);
        /// <summary>
        /// Called when a curve is removed from the tracked spline.
        /// </summary>
        protected abstract void OnBeforeAttachmentRemoved(TAttachment attachment);

        protected override void OnSplineCurveAdded(object sender, ListModifiedEventArgs<Curve> e)
        {
            e.Item.CurveChanged += OnSplineCurveChanged;
            attachments.Add(OnBeforeAttachmentAdded(e.Item));
        }

        protected override void OnSplineCurveInserted(object sender, ListModifiedEventArgs<Curve> e)
        {
            e.Item.CurveChanged += OnSplineCurveChanged;
            attachments.Insert(e.Index, OnBeforeAttachmentAdded(e.Item));
        }

        protected override void OnSplineCurveReplaced(object sender, ListItemReplacedEventArgs<Curve> e)
        {
            e.OldItem.CurveChanged -= OnSplineCurveChanged;
            OnAttachmentChange(e.NewItem, attachments[e.Index]);
            e.NewItem.CurveChanged += OnSplineCurveChanged;
        }

        protected override void OnSplineCurveRemoved(object sender, ListModifiedEventArgs<Curve> e)
        {
            e.Item.CurveChanged -= OnSplineCurveChanged;
            OnBeforeAttachmentRemoved(attachments[e.Index]);
            attachments.RemoveAt(e.Index);
        }

        protected override void OnSplineCurveChanged(object sender, EventArgs e)
        {
            Curve curve = (Curve)sender;
            int index = Spline.Curves.IndexOf(curve);
            OnAttachmentChange(curve, attachments[index]);
        }

        protected override void OnSplineCleared(object sender, EventArgs e)
        {
            foreach (var attachment in attachments)
                OnBeforeAttachmentRemoved(attachment);

            attachments.Clear();
        }

        protected override void OnSplineChanged(Spline oldValue, Spline newValue)
        {
            if (oldValue == newValue) 
                return;

            if (oldValue != null)
            {
                for (int i = 0; i < oldValue.Curves.Count; i++)
                {
                    oldValue.Curves[i].CurveChanged -= OnSplineCurveChanged;
                    OnBeforeAttachmentRemoved(attachments[i]);
                }

                attachments.Clear();
            }


            if (newValue != null)
            {
                foreach (var curve in newValue.Curves)
                {
                    curve.CurveChanged += OnSplineCurveChanged;
                    attachments.Add(OnBeforeAttachmentAdded(curve));
                }
            }
        }

        public void Refresh()
        {
            if (Spline == null)
                return;

            for (int i = 0; i < Spline.Curves.Count; i++)
                OnAttachmentChange(Spline.Curves[i], attachments[i]);
        }
    }
}