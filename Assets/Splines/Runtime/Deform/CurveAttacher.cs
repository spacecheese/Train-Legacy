using Splines;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
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
    public abstract class CurveAttacher<TAttachment>
    {
        private Spline spline = null;
        public Spline Spline {
            get { return spline; }
            set { Spline oldSpline = spline; spline = value; OnSplineChanged(oldSpline, spline); }
        }

        private readonly List<TAttachment> attachments = null;
        public IReadOnlyList<TAttachment> Attachments => attachments;

        // Implementation defined handlers for managing attachments.
        /// <summary>
        /// Called when a curve is added or inserted into the tracked spline.
        /// </summary>
        protected abstract TAttachment OnCurveAdded(Curve curve);
        /// <summary>
        /// Called when a curve is modified or replaced in the tracked spline.
        /// </summary>
        protected abstract TAttachment OnCurveChanged(Curve curve, TAttachment attachment);
        /// <summary>
        /// Called when a curve is removed from the tracked spline.
        /// </summary>
        protected abstract void OnCurveRemoved(TAttachment attachment);

        // Internal handlers for curve related events.
        private void OnSplineCurveAdded(object sender, ListModifiedEventArgs<Curve> e)
        {
            e.Item.CurveChanged += OnSplineCurveChanged;
            attachments.Add(OnCurveAdded(e.Item));
        }

        private void OnSplineCurveInserted(object sender, ListModifiedEventArgs<Curve> e)
        {
            e.Item.CurveChanged += OnSplineCurveChanged;
            attachments.Insert(e.Index, OnCurveAdded(e.Item));
        }

        private void OnSplineCurveReplaced(object sender, ListItemReplacedEventArgs<Curve> e)
        {
            e.OldItem.CurveChanged -= OnSplineCurveChanged;
            attachments[e.Index] = OnCurveChanged(e.Item, attachments[e.Index]);
            e.Item.CurveChanged += OnSplineCurveChanged;
        }

        private void OnSplineCurveChanged(object sender, EventArgs e)
        {
            Curve curve = (Curve)sender;
            int index = spline.Curves.IndexOf(curve);
            OnCurveChanged(curve, attachments[index]);
        }

        private void OnSplineCurveRemoved(object sender, ListModifiedEventArgs<Curve> e)
        {
            e.Item.CurveChanged -= OnSplineCurveChanged;
            OnCurveRemoved(attachments[e.Index]);
            attachments.RemoveAt(e.Index);
        }

        private void OnSplineCurvesCleared(object sender, EventArgs e)
        {
            foreach (var attachment in attachments)
                OnCurveRemoved(attachment);

            attachments.Clear();
        }

        private void OnSplineChanged(Spline oldValue, Spline newValue)
        {
            if (oldValue == newValue) return;

            if (oldValue != null)
            {
                oldValue.Curves.ItemAdded -= OnSplineCurveAdded;
                oldValue.Curves.ItemInserted -= OnSplineCurveInserted;
                oldValue.Curves.ItemReplaced -= OnSplineCurveReplaced;
                oldValue.Curves.ItemRemoved -= OnSplineCurveRemoved;
                oldValue.Curves.Cleared -= OnSplineCurvesCleared;

                int i = 0;
                foreach (var curve in oldValue.Curves)
                {
                    curve.CurveChanged -= OnSplineCurveChanged;
                    OnCurveRemoved(attachments[i++]);
                }
                    
                attachments.Clear();
            }
            

            if (newValue != null)
            {
                newValue.Curves.ItemAdded += OnSplineCurveAdded;
                newValue.Curves.ItemInserted += OnSplineCurveInserted;
                newValue.Curves.ItemReplaced += OnSplineCurveReplaced;
                newValue.Curves.ItemRemoved += OnSplineCurveRemoved;
                newValue.Curves.Cleared += OnSplineCurvesCleared;

                foreach (var curve in newValue.Curves)
                {
                    curve.CurveChanged += OnSplineCurveChanged;
                    attachments.Add(OnCurveAdded(curve));
                }
            }
        }
    }
}