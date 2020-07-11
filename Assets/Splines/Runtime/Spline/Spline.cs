using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;

namespace Splines
{
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public partial class Spline : MonoBehaviour
    {
#if UNITY_EDITOR 
        public bool HighlightSamples = false;
#endif
        /// <summary>
        /// A list of curves within this spline.
        /// </summary>
        public IObservableReadOnlyList<Curve> Curves => curves;
        private readonly ObservableList<Curve> curves = 
            new ObservableList<Curve>();

        /// <summary>
        /// A list of nodes within this spline.
        /// </summary>
        public readonly ObservableList<CurveNode> Nodes =
            new ObservableList<CurveNode>();

        /// <summary>
        /// The total of approximate curve lengths in this spline.
        /// </summary>
        public float Length => Curves.Sum((item) => item.Length);

        [SerializeField]
        private float tesselationError = .02f;
        /// <summary>
        /// The tesselation error set on all curves contained by the spline. <seealso cref="Curve.TesselationError"/>.
        /// </summary>
        public float TesselationError {
            get => tesselationError;
            set {
                tesselationError = value;

                foreach (var curve in Curves)
                    curve.TesselationError = value;
            }
        }

        /// <summary>
        /// Indicates if the spline is connected at either end.
        /// </summary>
        public bool IsClosed => Start != null && Start == End;
        /// <summary>
        /// The first node in the spline.
        /// </summary>
        public CurveNode Start => Curves.Count > 0 ? Curves.First().Start : null;
        /// <summary>
        /// The last node in the spline.
        /// </summary>
        public CurveNode End => Curves.Count > 0 ? Curves.Last().End : null;

        /// <summary>
        /// Closes the spline by connecting the Start and End nodes.
        /// </summary>
        public void Close() => curves.Add(new Curve(End, Start, tesselationError));

        /// <summary>
        /// Breaks the spline open by duplicating the specified node.
        /// </summary>
        public void Break(CurveNode node)
        {
            // Copy the current sequence of nodes to a new list.
            List<CurveNode> copiedNodes = new List<CurveNode>(Nodes);
            Nodes.Clear();

            int nodeIndex = copiedNodes.IndexOf(node);

            // Re add all the nodes after and including the break node.
            for (int i = nodeIndex; i < copiedNodes.Count; i++)
                Nodes.Add(copiedNodes[i]);

            // Re add all the nodes before the break node.
            for (int i = 0; i < nodeIndex; i++)
                Nodes.Add(copiedNodes[i]);

            // Add a separate copy of the break node.
            Nodes.Add(new CurveNode(node));
        }

        /// <summary>
        /// Gives the distance of a specified node.
        /// </summary>
        public float GetDistanceOfNode(CurveNode node)
        {
            if (node == Start) return 0;

            float distance = 0;
            foreach (var item in Curves)
            {
                distance += item.Length;
                
                if (node == item.End)
                    return distance;
            }

            throw new ArgumentException(
                "node is not contained by the spline.", "node");
        }

        /// <summary>
        /// Finds the curve containing a distance along the spline.
        /// </summary>
        /// <param name="distance">
        /// A distance from the start of the spline to a target point.
        /// </param>
        /// <param name="innerDistance">
        /// The distance from the start of the curve to the specified point.
        /// </param>
        public Curve GetCurveAtDistance(float distance, 
            out float innerDistance)
        {
            if (distance < 0)
                throw new ArgumentOutOfRangeException("distance");

            float curveStartDistance, curveEndDistance = 0;

            var enumerator = Curves.GetEnumerator();
            while (enumerator.MoveNext())
            {
                curveStartDistance = curveEndDistance;
                curveEndDistance += enumerator.Current.Length;

                if (curveEndDistance > distance ||
                    Mathf.Approximately(distance, curveEndDistance))
                {
                    innerDistance = distance - curveStartDistance;
                    return enumerator.Current;
                }
            }

            throw new ArgumentOutOfRangeException("distance");
        }

        /// <summary>
        /// Gives the approximate position of the spline at the specified distance.
        /// </summary>
        public Vector3 GetPositionAtDistance(float distance)
        {
            var curve = GetCurveAtDistance(distance, out float innerDistance);
            return curve.GetPositionAtDistance(innerDistance);
        }

        /// <summary>
        /// Gets a Quaternion that will rotate a <see cref="Vector3.forward"/> to face the normal of the spline at the specifed distance.
        /// </summary>
        public Quaternion GetRotationAtDistance(float distance)
        {
            var curve = GetCurveAtDistance(distance, out float innerDistance);
            return curve.GetRotationAtDistance(innerDistance);
        }

        /// <summary>
        /// Gives the approximate normal of the spline at the specified distance.
        /// </summary>
        public Vector3 GetNormalAtDistance(float distance)
        {
            var curve = GetCurveAtDistance(distance, out float innerDistance);
            return curve.GetNormalAtDistance(innerDistance);
        }

        /// <summary>
        /// Gets the approximate position and normal of the spline at a specified distance.
        /// </summary>
        public void GetTransformAtDistance(float distance, 
            out Vector3 position, out Quaternion rotation)
        {
            var curve = GetCurveAtDistance(distance, out float innerDistance);
            curve.GetTransformAtDistance(innerDistance, out position, out rotation);
        }

        private void NodeAdded(object sender, ListModifiedEventArgs<CurveNode> e)
        {
            if (e.Index > 0)
                // Connect the new node if there are multiple nodes.
                curves.Add(new Curve(Nodes[e.Index - 1], e.Item, tesselationError));
        }

        private void NodeInserted(object sender, ListModifiedEventArgs<CurveNode> e)
        {
            if (e.Index > 0)
                // Change the end node of the lower curve if this is not the new first node.
                curves[e.Index - 1].End = e.Item;

            if (e.Index < curves.Count)
                // Insert a new upper curve if this is not the new last node.
                curves.Insert(e.Index, new Curve(e.Item, Nodes[e.Index + 1], tesselationError));
        }

        private void NodeRemoved(object sender, ListModifiedEventArgs<CurveNode> e)
        {
            if (e.Index > 0)
            {
                // Remove the curve with the removal target as its end.
                curves.RemoveAt(e.Index - 1);

                // Set the start of the upper curve to the node before the one that was removed.
                curves[e.Index - 1].Start = Nodes[e.Index - 1];
            }
            else
                // Remove the first curve if the first node is being removed.
                curves.RemoveAt(0);
        }

        private void NodeReplaced(object sender, ListModifiedEventArgs<CurveNode> e)
        {
            if (e.Index > 0)
                // Replace the end of the lower curve (if it exists) with the new node.
                curves[e.Index - 1].End = e.Item;

            if (e.Index < curves.Count)
                // Replace the start of the upper curve (if it exists) with the new node.
                curves[e.Index].Start = e.Item;
        }

        private void NodesCleared(object sender, EventArgs e) => curves.Clear();

        public void OnEnable()
        {
            Nodes.ItemAdded += NodeAdded;
            Nodes.ItemInserted += NodeInserted;
            Nodes.ItemRemoved += NodeRemoved;
            Nodes.ItemReplaced += NodeReplaced;
            Nodes.Cleared += NodesCleared;
        }
    }
}
