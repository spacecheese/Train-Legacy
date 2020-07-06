using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Splines
{
    [DisallowMultipleComponent]
    public partial class Spline : MonoBehaviour
    {
        /// <summary>
        /// A List of curves within this spline.
        /// </summary>
        public readonly IObservableList<Curve> Curves = new ObservableList<Curve>();

        /// <summary>
        /// The total of approximate curve lengths in this spline.
        /// </summary>
        public float Length => Curves.Sum((item) => item.Length);

        /// <summary>
        /// Indicates if the spline is connected at either end.
        /// </summary>
        public bool IsClosed => Start != null && End != null && Start == End;
        /// <summary>
        /// The first node in the spline.
        /// </summary>
        public CurveNode Start => Curves.Count > 0 ? Curves[0].Start : null;
        /// <summary>
        /// The last node in the spline.
        /// </summary>
        public CurveNode End => Curves.Count > 0 ? Curves[Curves.Count - 1].End : null; 

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
        /// Gives the curve in which a given distance along the spline with lie.
        /// </summary>
        public void GetCurveAtDistance(float distance, 
            out Curve curve, out float innerDistance)
        {
            if (distance < 0)
                throw new ArgumentOutOfRangeException("distance");

            int i = 0;
            float totalDistance = 0;

            while ((totalDistance + Curves[i].Length) < distance)
            {
                totalDistance += Curves[i].Length;

                if (Mathf.Approximately(totalDistance, distance)) continue;

                if (++i >= Curves.Count)
                    throw new ArgumentOutOfRangeException("distance");
            }

            curve = Curves[i];
            innerDistance = distance - totalDistance;
            return;
        }

        /// <summary>
        /// Gives the approximate position of the spline at the specified distance.
        /// </summary>
        public Vector3 GetPositionAtDistance(float distance)
        {
            GetCurveAtDistance(distance, out Curve curve, out float innerDistance);
            return curve.GetPositionAtDistance(innerDistance);
        }

        /// <summary>
        /// Gets a Quaternion that will rotate a <see cref="Vector3.forward"/> to face the normal of the spline at the specifed distance.
        /// </summary>
        public Quaternion GetRotationAtDistance(float distance)
        {
            GetCurveAtDistance(distance, out Curve curve, out float innerDistance);
            return curve.GetRotationAtDistance(innerDistance);
        }

        /// <summary>
        /// Gives the approximate normal of the spline at the specified distance.
        /// </summary>
        public Vector3 GetNormalAtDistance(float distance)
        {
            GetCurveAtDistance(distance, out Curve curve, out float innerDistance);
            return curve.GetNormalAtDistance(innerDistance);
        }

        /// <summary>
        /// Gets the approximate position and normal of the spline at a specified distance.
        /// </summary>
        public void GetTransformAtDistance(float distance, 
            out Vector3 position, out Quaternion rotation)
        {
            GetCurveAtDistance(distance, out Curve curve, out float innerDistance);
            curve.GetTransformAtDistance(innerDistance, out position, out rotation);
        }

        /// <summary>
        /// The sequence of nodes in the spline.
        /// </summary>
        public IReadOnlyList<CurveNode> Nodes {
            get
            {
                var nodes = new List<CurveNode>();
                foreach (var curve in Curves)
                    if (curve.Start != null)
                        nodes.Add(curve.Start);

                if (End != null)
                    nodes.Add(End);

                return nodes;
            }
        }

        /// <summary>
        /// Connect the curves surrounding the specified index to the curve at the specifed index and vice-versa.
        /// </summary>
        private void ConnectCurves(int index)
        {
            if (index > 0)
            {
                Curves[index - 1].Next = Curves[index];
                Curves[index].Previous = Curves[index - 1];
            }
            else
                Curves[index].Previous = null;

            if ((index + 1) < Curves.Count)
            {
                Curves[index + 1].Previous = Curves[index];
                Curves[index].Next = Curves[index + 1];
            }
            else
                Curves[index].Next = null;
        }

        public void OnEnable()
        {
            Curves.ItemAdded += (s, e) => ConnectCurves(e.Index);
            Curves.ItemMoved += (s, e) => { ConnectCurves(e.NewIndex); ConnectCurves(e.OldIndex); };
            Curves.ItemRemoved += (s, e) => ConnectCurves(e.Index);
        }
    }
}
