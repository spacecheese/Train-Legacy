using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Splines
{
    public partial class Curve
    {
        private bool samplesDirty = true;

        /// <summary>
        /// Structure used to store data about cached curve samples.
        /// </summary>
        private struct CurveSample
        {
            public float Time;
            public Vector3 Position;
            public Vector3 Normal;
            public float? Distance;

            public CurveSample(float time, Vector3 position, Vector3 normal, float? distance = null)
            {
                Time = time;
                Distance = distance;
                Normal = normal;
                Position = position;
            }
        }

        private float tesselationError;
        /// <summary>
        /// The differential error that will cause a curve segment to be subdivided with a new sample by tesselation.
        /// Internally this is compared to the dot product of adjacent sample normals.
        /// </summary>
        public float TesselationError
        {
            get => tesselationError;
            set
            {
                tesselationError = value;
                OnCurveChanged();
            }
        }

        private int minSamples = 4;
        /// <summary>
        /// The minimum number of samples (including nodes at either end) used to approximate the curve.
        /// </summary>
        public int MinSamples
        {
            get => ControlPoints.Count <= 2 ? 2 : minSamples;
            set => minSamples = value;
        }

        private readonly List<CurveSample> samples = new List<CurveSample>();
        /// <summary>
        /// Enumerable of samples cached by the curve.
        /// </summary>
        public IEnumerable<Vector3> Samples
        {
            get
            {
                if (!CheckSamples()) return new List<Vector3>();

                return samples.Select((sample) => sample.Position);
            }
        }

        private readonly List<Vector3> evaluationPoints = new List<Vector3>(4);
        /// <summary>
        /// Evaluates the position of the underlying curve function at the specifed time where 0 <= time <= 1.
        /// </summary>
        private CurveSample Evaluate(float time)
        {
            evaluationPoints.Clear();
            evaluationPoints.AddRange(ControlPoints);

            Vector3 normal = Vector3.zero;
            while (evaluationPoints.Count > 1)
            {
                // See https://en.wikipedia.org/wiki/B%C3%A9zier_curve#Higher-order_curves
                if (evaluationPoints.Count == 2)
                    normal = evaluationPoints[1] - evaluationPoints[0];

                for (int i = 1; i < evaluationPoints.Count; i++)
                    evaluationPoints[i - 1] = Vector3.Lerp(evaluationPoints[i - 1], evaluationPoints[i], time);

                evaluationPoints.RemoveAt(evaluationPoints.Count - 1);
            }

            return new CurveSample(time, evaluationPoints[0], normal);
        }

        /// <summary>
        /// Evalutes the underlying curve function to populate samples with <see cref="MinSamples"/> samples.
        /// </summary>
        private void PopulateSamples()
        {
            if (start == null || end == null) throw new InvalidOperationException("Cannot evaluate a curve where either node is null");

            samples.Clear();

            // Evaluate at equal increments of t until samples is filled.
            for (int i = 0; i < MinSamples; i++)
            {
                float time = 1f / (MinSamples - 1) * i;
                samples.Add(Evaluate(time));
            }
        }

        /// <summary>
        /// Subdivides curve sample segments where the differential error exceeds <see cref="TesselationError"/>.
        /// <seealso cref="TesselationError"/>
        /// </summary>
        private void TesselateSamples()
        {
            if (TesselationError == 0) return;

            for (int i = 1; i < samples.Count; i++)
            {
                while (true)
                {
                    // Ideally the dot product of the normalized normals should be 1.
                    float normalError = 1 - Vector3.Dot(samples[i - 1].Normal.normalized, samples[i].Normal.normalized);

                    if (normalError < TesselationError)
                        // Do nothing if the error is within tolerance.
                        break;
                    else
                    {
                        // Evaluate a new point half way between the current samples.
                        float time = (samples[i - 1].Time + samples[i].Time) / 2;
                        samples.Insert(i, Evaluate(time));
                    }
                }
            }
        }

        /// <summary>
        /// Calculates and stores the approximate length of each curve segment in <see cref="samples"/>.
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
        /// Check if samples requires population or repopulation.
        /// </summary>
        /// <returns>True if samples is populated. False otherwise.</returns>
        private bool CheckSamples()
        {
            if (samplesDirty)
            {
                // Re sample the curve when a parameter is changed.
                samplesDirty = false;

                PopulateControlPoints();
                PopulateSamples();

                if (samples.Count != 0)
                {
                    // Only tesselate and calculate distances if the curve was sampled.
                    TesselateSamples();
                    CalculateDistances();
                }

                return samples.Count != 0;
            }

            return true;
        }
    }
}
