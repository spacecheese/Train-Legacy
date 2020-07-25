using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        /// Finds the samples immediatly above and below the specified distance (or on
        /// and above if distance lies at a sample). If only one sample exists then 
        /// same sample will be returned twice.
        /// </summary>
        private void FindBoundingSamples(float distance,
            out CurveSample lower, out CurveSample upper)
        {
            if (distance < 0 ||
                distance > Length && !Mathf.Approximately(distance, Length)) 
                throw new ArgumentOutOfRangeException("distance");


            if (!CheckSamples())
                throw new InvalidOperationException("The curve could not be sampled.");

            if (samples.Count == 1)
            {
                lower = samples[0]; upper = samples[0];
                return;
            }

            for (int i = 1; i < samples.Count; i++)
            {
                if (samples[i].Distance >= distance)
                {
                    lower = samples[i - 1]; upper = samples[i];
                    return;
                }
            }

            throw new ArgumentOutOfRangeException("distance");
        }

        /// <summary>
        /// Calculates the t (0 <= t <= 1) of a distance given two bounding samples.
        /// </summary>
        private static float GetIntervalTime(CurveSample lower, CurveSample upper, float distance)
        {
            return (distance - lower.Distance) / (upper.Distance - lower.Distance);
        }

        private static Vector3 GetPositionFromSamples(CurveSample lower, CurveSample upper, float time)
        {
            return Vector3.Lerp(lower.Position, upper.Position, GetIntervalTime(lower, upper, time));
        }

        private static Vector3 GetNormalFromSamples(CurveSample lower, CurveSample upper, float time)
        {
            return Vector3.Lerp(lower.Normal, upper.Normal, GetIntervalTime(lower, upper, time));
        }

        private Quaternion GetRotationFromSamples(CurveSample lower, CurveSample upper, float distance)
        {
            Vector3 normal = GetNormalFromSamples(lower, upper, distance);
            Quaternion rotation = Quaternion.Slerp(Start.Rotation, End.Rotation, distance / Length);
            return Quaternion.LookRotation(normal, rotation * Vector3.up);
        }

        /// <summary>
        /// Gets the approximate position at a distance along the curve.
        /// </summary>
        public Vector3 GetPositionAtDistance(float distance)
        {
            FindBoundingSamples(distance, out CurveSample lower, out CurveSample upper);
            return GetPositionFromSamples(lower, upper, distance);
        }

        /// <summary>
        /// Gets the approximate normal at a distance along the curve.
        /// </summary>
        public Vector3 GetNormalAtDistance(float distance)
        {
            FindBoundingSamples(distance, out CurveSample lower, out CurveSample upper);
            return GetNormalFromSamples(lower, upper, distance);
        }

        /// <summary>
        /// Gets a Quaternion that will rotate a <see cref="Vector3.forward"/> to face the normal of the curve at the specifed distance.
        /// </summary>
        public Quaternion GetRotationAtDistance(float distance)
        {
            FindBoundingSamples(distance, out CurveSample lower, out CurveSample upper);
            return GetRotationFromSamples(lower, upper, distance);
        }

        /// <summary>
        /// Gets the approximate position and normal of the curve at a specified distance.
        /// </summary>
        public void GetTransformAtDistance(float distance,
            out Vector3 position, out Quaternion rotation)
        {
            FindBoundingSamples(distance, out CurveSample lower, out CurveSample upper);

            position = GetPositionFromSamples(lower, upper, distance);
            rotation = GetRotationFromSamples(lower, upper, distance);
        }

        public Curve(CurveNode start, CurveNode end, float tesselationError)
        {
            Start = start;
            End = end;
            TesselationError = tesselationError;
        }
    }
}
