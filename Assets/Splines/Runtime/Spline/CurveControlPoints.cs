using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Splines
{
    public partial class Curve
    {
        private CurveNode start;
        /// <summary>
        /// The starting node of this curve.
        /// </summary>
        public CurveNode Start
        {
            get { return start; }
            set
            {
                if (start != null)
                    start.NodeChanged -= NodeChanged;
                if (value != null)
                    value.NodeChanged += NodeChanged;

                start = value;
                OnCurveChanged();
            }
        }

        private CurveNode end;
        /// <summary>
        /// The ending node of this curve.
        /// </summary>
        public CurveNode End
        {
            get { return end; }
            set
            {
                if (end != null)
                    end.NodeChanged -= NodeChanged;
                if (value != null)
                    value.NodeChanged += NodeChanged;

                end = value;
                OnCurveChanged();
            }
        }

        /// <summary>
        /// Returns a list of the positions of handles and nodes for convenient use by <see cref="Evaluate(float)"/>.
        /// </summary>
        private readonly List<Vector3> ControlPoints = new List<Vector3>(4);

        private void NodeChanged(object sender, EventArgs evt) => OnCurveChanged();

        private void PopulateControlPoints()
        {
            ControlPoints.Clear();

            ControlPoints.Add(Start.Position);
            if (Start.AfterHandle.HasValue)
                ControlPoints.Add(Start.AfterHandle.Value + Start.Position);
            if (End.BeforeHandle.HasValue)
                ControlPoints.Add(End.BeforeHandle.Value + End.Position);
            ControlPoints.Add(End.Position);
        }
    }
}
