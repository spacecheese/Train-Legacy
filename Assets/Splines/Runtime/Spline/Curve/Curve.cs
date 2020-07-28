using System;
using UnityEngine;

namespace Splines
{
    public partial class Curve
    {
        /// <summary>
        /// The approximate length of the curve.
        /// </summary>
        public float Length
        {
            get
            {
                CheckSamples();
                return samples[samples.Count - 1].Distance;
            }
        }

        /// <summary>
        /// Fired when the curve is changed and marked for resampling.
        /// </summary>
        public event EventHandler CurveChanged;

        private void OnCurveChanged()
        {
            samplesDirty = true;
            CurveChanged?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Cache used the store the last result of <see cref="FindSampleIndex(float)"/>. Format is distance, index.
        /// </summary>
        private (float, int) sampleCache = (-1,-1);
        /// <summary>
        /// Performs a binary search to find the index of the sample at the specified distance or 
        /// the binary complement of the index of the sample above the specified distance.
        /// </summary>
        /// <seealso cref="IListExtensions.BinarySearch{TItem, TSearch}(IList{TItem}, TSearch, Func{TSearch, TItem, int})"/>
        private int FindSampleIndex(float distance)
        {
            if (distance < 0 ||
                distance > Length && !Mathf.Approximately(distance, Length)) 
                throw new ArgumentOutOfRangeException("distance");

            if (distance == sampleCache.Item1)
                return sampleCache.Item2;

            int comparer(float a, CurveSample b)
            {
                if (a > b.Distance)
                    return 1;
                else if (a < b.Distance)
                    return -1;
                else
                    return 0;
            }

            // Cache the last requested sample so that position/ normal lookups only require one search.
            int samleIndex = samples.BinarySearch(distance, comparer);
            sampleCache = (distance, samleIndex);
            return samleIndex;
        }

        /// <summary>
        /// Calculates the t (0 <= t <= 1) of a distance given two bounding samples.
        /// </summary>
        private static float GetIntervalTime(CurveSample lower, CurveSample upper, float distance) =>
            (distance - lower.Distance) / (upper.Distance - lower.Distance);

        private static Vector3 GetPositionFromSamples(CurveSample lower, CurveSample upper, float distance) =>
            Vector3.Lerp(lower.Position, upper.Position, GetIntervalTime(lower, upper, distance));

        private static Vector3 GetNormalFromSamples(CurveSample lower, CurveSample upper, float distance) =>
            Vector3.Lerp(lower.Normal, upper.Normal, GetIntervalTime(lower, upper, distance));

        /// <summary>
        /// Gets the approximate position at a distance along the curve.
        /// </summary>
        public Vector3 GetPositionAtDistance(float distance)
        {
            int sampleIndex = FindSampleIndex(distance);

            if (sampleIndex < 0)
                // Distance lies between two samples.
                return GetPositionFromSamples(samples[~sampleIndex - 1], samples[~sampleIndex], distance);
            else
                // Exact match for distance found.
                return samples[sampleIndex].Position;
        }

        /// <summary>
        /// Gets the approximate normal at a distance along the curve.
        /// </summary>
        public Vector3 GetNormalAtDistance(float distance)
        {
            int sampleIndex = FindSampleIndex(distance);

            if (sampleIndex < 0)
                // Normal lies between two samples.
                return GetNormalFromSamples(samples[~sampleIndex - 1], samples[~sampleIndex], distance);
            else
                // Exact match for distance found.
                return samples[sampleIndex].Normal;
        }

        /// <summary>
        /// Gets a Quaternion that will rotate a <see cref="Vector3.forward"/> to face the normal of the curve at the specifed distance.
        /// </summary>
        public Quaternion GetRotationAtDistance(float distance)
        {

            Quaternion nodeRotation = Quaternion.Slerp(Start.Rotation, End.Rotation, distance / Length);
            Vector3 normal = GetNormalAtDistance(distance);

            return Quaternion.LookRotation(normal, nodeRotation * Vector3.up);
        }

        /// <summary>
        /// Gets the approximate position and normal of the curve at a specified distance.
        /// </summary>
        public void GetTransformAtDistance(float distance,
            out Vector3 position, out Quaternion rotation)
        {
            position = GetPositionAtDistance(distance);
            rotation = GetRotationAtDistance(distance);
        }

        public Curve(CurveNode start, CurveNode end, float tesselationError)
        {
            Start = start;
            End = end;
            TesselationError = tesselationError;
        }
    }
}
