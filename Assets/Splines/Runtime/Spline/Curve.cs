using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Splines
{
    public class Curve
    {
        /// <summary>
        /// Structure used to store data about cached curve samples.
        /// </summary>
        private struct CurveSample
        {
            public float Time;
            public float Distance;
            public Vector3 Position;

            public CurveSample(float time, float distance, Vector3 position)
            {
                Time = time;
                Distance = distance;
                Position = position;
            }
        }

        /// <summary>
        /// The differential error in any one axis that will cause a sample to be subdivided during a tesselation.
        /// </summary>
        public float TesselationError { get; set; } = 1f;

        private int sampleCount = 20;
        /// <summary>
        /// The number of samples used to approximate the curve. This will always be 2 if the curve has no handles (is a line).
        /// </summary>
        public int SampleCount
        {
            get => Points.Count <= 2 ? 2 : sampleCount;
            set => sampleCount = value;
        }

        private List<CurveSample> samples;
        /// <summary>
        /// Enumerable of samples cached by the curve.
        /// </summary>
        public IEnumerable<Vector3> Samples {
            get {
                if (!CheckSamples()) return new List<Vector3>();

                return samples.Select((sample) => sample.Position);
            }
        }

        /// <summary>
        /// Check if samples requires population or repopulation.
        /// </summary>
        /// <returns>True if samples is now populated. False otherwise.</returns>
        private bool CheckSamples()
        {
            if (samples == null)
                samples = new List<CurveSample>(SampleCount);

            if (samples.Count == 0)
                PopulateSamples();

            if (samples.Count != 0)
            {
                TesselateSamples();
                CalculateDistances();
            }

            return samples.Count != 0;
        }

        /// <summary>
        /// Subdivides curve sample segments where the differential error exceeds <see cref="TesselationError"/> in any one axis.
        /// </summary>
        private void TesselateSamples()
        {
            if (TesselationError == 0) return;
            
            for (int i = 1; i < samples.Count - 1; i++)
            {
                while (true)
                {
                    Vector3 lowerNormal = GetNormalFromSamples(samples[i - 1], samples[i]);
                    Vector3 upperNormal = GetNormalFromSamples(samples[i], samples[i + 1]);

                    Vector3 error = upperNormal - lowerNormal;
                    if (error.x > TesselationError ||
                        error.y > TesselationError ||
                        error.z > TesselationError)
                    {
                        float upperTime = (samples[i].Time + samples[i + 1].Time) / 2;
                        samples.Insert(i + 1, new CurveSample(upperTime, 0, Evaluate(upperTime)));

                        float lowerTime = (samples[i].Time + samples[i - 1].Time) / 2;
                        samples.Insert(i, new CurveSample(lowerTime, 0, Evaluate(lowerTime)));
                    }
                    else
                        break;
                }
            }
        }

        /// <summary>
        /// Calculates and stores the length of each curve segment in <see cref="samples"/>.
        /// </summary>
        private void CalculateDistances()
        {
            float distance = 0;

            CurveSample sample = samples[0];
            sample.Distance = 0;
            samples[0] = sample;

            for (int i = 1; i < samples.Count; i++)
            {
                distance += Vector3.Distance(samples[i].Position, samples[i - 1].Position);

                sample = samples[i];
                sample.Distance = distance;
                samples[i] = sample;
            }
        }

        /// <summary>
        /// Evalutes the underlying curve function to populate samples with <see cref="SampleCount"/> samples.
        /// </summary>
        private void PopulateSamples()
        {
            if (start == null && end == null) throw new InvalidOperationException("Cannot evaluate a curve with no nodes");
            
            if (end == null)
            {
                samples.Add(new CurveSample(0, 0, start.Position));
                return;
            }
            else if (start == null)
            {
                samples.Add(new CurveSample(0, 0, end.Position));
                return;
            }

            // Evaluate at equal increments of t until samples is filled.
            for (int i = 0; i < SampleCount; i++)
            {
                float time = 1f / (SampleCount - 1) * i;

                samples.Add(new CurveSample(time, 0, Evaluate(time)));
            }
        }

        /// <summary>
        /// Finds the samples immediatly above and below the specified distance (or on
        /// and above if distance lies at a sample). If only one sample exists then 
        /// same sample will be returned twice.
        /// </summary>
        private void FindBoundingSamples(float distance,
            out CurveSample lower, out CurveSample upper)
        {
            if (distance < 0 || distance > Length) 
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
        /// Evaluates the position of the underlying curve function at the specifed time where 0 <= time <= 1.
        /// </summary>
        private Vector3 Evaluate(float time)
        {
            IReadOnlyList<Vector3> lerpedPoints = Points;
            if (lerpedPoints.Count == 0)
                throw new InvalidOperationException("An empty curve cannot be evaluated.");

            while (lerpedPoints.Count > 1)
            {
                // See https://en.wikipedia.org/wiki/B%C3%A9zier_curve#Higher-order_curves
                var newPoints = new List<Vector3>(lerpedPoints.Count - 1);

                for (int i = 1; i < lerpedPoints.Count; i++)
                    newPoints.Add(Vector3.Lerp(lerpedPoints[i - 1], lerpedPoints[i], time));

                lerpedPoints = newPoints;
            }

            return lerpedPoints[0];
        }

        /// <summary>
        /// Returns a list of the positions of handles and nodes for convenient use by <see cref="Evaluate(float)"/>.
        /// </summary>
        private IReadOnlyList<Vector3> Points
        {
            get { 
                var points = new List<Vector3>();

                if (Start == null || End == null) return points;

                points.Add(Start.Position);

                if (Start.AfterHandle.HasValue) 
                    points.Add(Start.AfterHandle.Value + Start.Position);
                if (End.BeforeHandle.HasValue) 
                    points.Add(End.BeforeHandle.Value + End.Position);

                points.Add(End.Position);

                return points;
            }
        }

        /// <summary>
        /// The approximate length of the curve.
        /// </summary>
        public float Length { 
            get {
                CheckSamples();
                return samples[samples.Count - 1].Distance; 
            } 
        }

        /// <summary>
        /// The curve following this curve in a spline.
        /// </summary>
        public Curve Next { get; set; }
        
        /// <summary>
        /// The curve preceding this curve in a spline.
        /// </summary>
        public Curve Previous { get; set; }

        private CurveNode start;
        /// <summary>
        /// The starting node of this curve.
        /// </summary>
        public CurveNode Start {
            get { return start; }
            set {
                if (start != null)
                    start.NodeChanged -= OnNodeChanged;
                if (value != null)
                    value.NodeChanged += OnNodeChanged;

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
            set {
                if (end != null)
                    end.NodeChanged -= OnNodeChanged;
                if (value != null)
                    value.NodeChanged += OnNodeChanged;

                end = value;
                OnCurveChanged();
            }
        }

        private void OnNodeChanged(object sender, EventArgs evt) => OnCurveChanged();

        /// <summary>
        /// Fired when the curve is changed and marked for resampling.
        /// </summary>
        public event EventHandler CurveChanged;

        private void OnCurveChanged()
        {
            samples = null;
            bounds = null;
            CurveChanged?.Invoke(this, new EventArgs());
        }

        private Vector3 GetPositionFromSamples(CurveSample lower, CurveSample upper, float distance)
        {
            float time = (distance - lower.Distance) / (upper.Distance - lower.Distance);
            return Vector3.Lerp(lower.Position, upper.Position, time);
        }

        private Bounds? bounds = null;
        /// <summary>
        /// Computes and caches the bounds the of curve samples.
        /// </summary>
        public Bounds GetBounds()
        {
            if (bounds != null) return bounds.Value;

            CheckSamples();

            bounds = new Bounds(
                samples.Min((item) => item.Position), 
                samples.Max((item) => item.Position));
            return bounds.Value;
        }

        /// <summary>
        /// Gets the approximate position at a distance along the curve.
        /// </summary>
        public Vector3 GetPositionAtDistance(float distance)
        {
            FindBoundingSamples(distance, out CurveSample lower, out CurveSample upper);
            return GetPositionFromSamples(lower, upper, distance);
        }

        private Vector3 GetNormalFromSamples(CurveSample lower, CurveSample upper)
        {
            return upper.Position - lower.Position;
        }

        /// <summary>
        /// Gets the approximate normal at a distance along the curve.
        /// </summary>
        public Vector3 GetNormalAtDistance(float distance)
        {
            FindBoundingSamples(distance, out CurveSample lower, out CurveSample upper);
            return GetNormalFromSamples(lower, upper);
        }

        private Quaternion GetRotationFromSamples(CurveSample lower, CurveSample upper, float distance)
        {
            Vector3 normal = GetNormalFromSamples(lower, upper);
            if (normal == Vector3.zero)
                return Quaternion.identity;

            Quaternion rotation = Quaternion.Slerp(Start.Rotation, End.Rotation, distance / Length);

            return Quaternion.LookRotation(normal, rotation * Vector3.up);
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

        /// <summary>
        /// Gets the approximate position of the closest point in the spline to the specified point.
        /// </summary>
        public Vector3 GetClosestSample(Vector3 point)
        {
            CheckSamples();

            Vector3 closestSample = samples[0].Position;
            float lowestSqrDistance = float.PositiveInfinity;

            foreach (var sample in samples)
            {
                // Find the sample with the lowest sqr distance to the point.
                float sqrDistance = (sample.Position - point).sqrMagnitude;
                if (sqrDistance < lowestSqrDistance)
                {
                    lowestSqrDistance = sqrDistance;
                    closestSample = sample.Position;
                }
            }

            return closestSample;
        }

        /// <summary>
        /// Gets approximate position of the closest point in the spline to the specified ray.
        /// </summary>
        public Vector3 GetClosestSample(Ray ray, out float distance)
        {
            CheckSamples();

            Vector3 closestSample = samples[0].Position;
            float lowestSqrDistance = float.PositiveInfinity;

            foreach (var sample in samples)
            {
                Vector3 closestRayPoint = Vector3.Dot(ray.direction, sample.Position - ray.origin) * ray.direction;
                float sqrDistance = (sample.Position - closestRayPoint).sqrMagnitude;

                if (sqrDistance < lowestSqrDistance)
                {
                    lowestSqrDistance = sqrDistance;
                    closestSample = sample.Position;
                }
            }

            distance = Mathf.Sqrt(lowestSqrDistance);
            return closestSample;
        }

        public Curve(CurveNode start, CurveNode end, Curve next = null, Curve previous = null)
        {
            Start = start;
            End = end;

            Next = next;
            Previous = previous;
        }
    }
}
