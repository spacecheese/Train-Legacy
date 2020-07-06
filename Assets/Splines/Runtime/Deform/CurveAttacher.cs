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
    public abstract class CurveAttacher<TAttachment> : MonoBehaviour, ISerializationCallbackReceiver 
        where TAttachment : class
    {
        [SerializeField]
        protected Spline spline = null;

        private readonly Dictionary<Curve, TAttachment> attachments = new Dictionary<Curve, TAttachment>();
        private readonly List<Action> waitingActions = new List<Action>();

        protected abstract TAttachment CurveAdded(Curve curve);
        protected abstract void CurveChanged(Curve curve, ref TAttachment attachment);
        protected abstract void CurveRemoved(TAttachment attachment);

        private void RegisterSpline()
        {
            foreach (var curve in spline.Curves)
                AddCurve(curve);

            spline.Curves.ItemAdded += (s, e) => AddCurve(e.Item);
            spline.Curves.ItemRemoved += (s, e) => RemoveCurve(e.Item);
        }

        private void AddCurve(Curve curve)
        {
            // Queue the curve for addition.
            waitingActions.Add(() => CurveAdded(curve));
            curve.CurveChanged += OnCurveChanged;
        }

        private void RemoveCurve(Curve curve)
        {
            TAttachment attachment = attachments[curve];
            curve.CurveChanged -= OnCurveChanged;
            // Queue the curve for removal.
            waitingActions.Add(() => CurveRemoved(attachment));

            attachments.Remove(curve);
        }

        private void OnCurveChanged(object sender, EventArgs evt)
        {
            // Queue the curve for renewal in the next update.
            waitingActions.Add(() =>
            {
                var curve = sender as Curve;

                var attachment = attachments[curve];
                CurveChanged(curve, ref attachment);
                attachments[curve] = attachment;
            });
        }

        public void OnBeforeSerialize()
        {
            // Remove the curves directly as no OnUpdate will be called.
            foreach (var attachment in attachments.Values)
                CurveRemoved(attachment);
        }

        public void OnAfterDeserialize() { }

        private Spline oldSpline = null;

        private void OnValidate()
        {
            if (spline == null ||
                spline == oldSpline) 
                return;

            // Only re-register the spline if it has changed.
            RegisterSpline();
        }

        private void Update()
        {
            foreach (var action in waitingActions)
                action?.Invoke();
        }
    }
}