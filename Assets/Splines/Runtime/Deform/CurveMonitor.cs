using System;
using UnityEngine;

namespace Splines.Deform
{
    public abstract class CurveMonitor : MonoBehaviour
    {
        [SerializeField]
        [HideInInspector]
        private Spline spline = null;
        public Spline Spline
        {
            get { return spline; }
            set { Spline oldSpline = spline; spline = value; OnSplineChanged(oldSpline, spline); }
        }

        protected abstract void OnSplineCurveAdded(object sender, ListModifiedEventArgs<Curve> e);
        protected abstract void OnSplineCurveInserted(object sender, ListModifiedEventArgs<Curve> e);
        protected abstract void OnSplineCurveReplaced(object sender, ListItemReplacedEventArgs<Curve> e);
        protected abstract void OnSplineCurveRemoved(object sender, ListModifiedEventArgs<Curve> e);
        protected abstract void OnSplineCurveChanged(object sender, EventArgs e);
        protected abstract void OnSplineCleared(object sender, EventArgs e);

        public virtual void OnEnable()
        {
            OnSplineChanged(null, Spline);
        }

        protected virtual void OnSplineChanged(Spline oldValue, Spline newValue)
        {
            if (oldValue == newValue) return;

            if (oldValue != null)
            {
                oldValue.Curves.ItemAdded -= OnSplineCurveAdded;
                oldValue.Curves.ItemInserted -= OnSplineCurveInserted;
                oldValue.Curves.ItemReplaced -= OnSplineCurveReplaced;
                oldValue.Curves.ItemRemoved -= OnSplineCurveRemoved;
                oldValue.Curves.Cleared -= OnSplineCleared;
            }

            if (newValue != null)
            {
                newValue.Curves.ItemAdded += OnSplineCurveAdded;
                newValue.Curves.ItemInserted += OnSplineCurveInserted;
                newValue.Curves.ItemReplaced += OnSplineCurveReplaced;
                newValue.Curves.ItemRemoved += OnSplineCurveRemoved;
                newValue.Curves.Cleared += OnSplineCleared;
            }
        }
    }
}
