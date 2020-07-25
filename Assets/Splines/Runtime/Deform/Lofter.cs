using Splines;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace Splines.Deform
{
    [ExecuteAlways]
    [Serializable]
    public class Lofter : CurveAttacher<GameObject>
    {
        [SerializeField]
        private Mesh profile;
        public Mesh Profile
        {
            get => profile;
            set { profile = value; }
        }

        private readonly Queue<Action> updateActions = new Queue<Action>();

        private void UpdateCurve(Curve curve, GameObject attachment)
        {
            if (profile == null)
                return;

            Mesh mesh = attachment.GetComponent<MeshFilter>().sharedMesh;

            if (mesh == null)
                mesh = new Mesh();

            Vector3[] profileVertices = profile.vertices;
            int[] profileTriangles = profile.triangles;

            int neededVertexCount = profileVertices.Length * curve.Samples.Count;
            // side triangles + cap triangles
            int neededTriangleCount = profileVertices.Length * 2 * (curve.Samples.Count - 1) + (profileTriangles.Length / 3) * 2;

            Vector3[] outVertices = new Vector3[neededVertexCount];
            int[] outTriangles = new int[neededTriangleCount * 3];

            IReadOnlyList<CurveSample> samples = curve.Samples;

            for (int sampleI = 0; sampleI < samples.Count; sampleI++)
            {
                int startVertI = sampleI * profileVertices.Length;
                for (int vertI = 0; vertI < profileVertices.Length; vertI++)
                    // Add each of the profile vertices to the out vertices offset by the sample position and rotation.
                    outVertices[startVertI + vertI] = samples[sampleI].GetRotation(curve) * (profileVertices[vertI] + samples[sampleI].Position);
            }

            // Add the start cap.
            profileTriangles.CopyTo(outTriangles, 0);
            for (int sampleI = 0; sampleI < samples.Count - 1; sampleI++)
            {
                int startTriangleI = sampleI * profileVertices.Length * 2 + (profileTriangles.Length / 3);
                for (int triangleI = 0; triangleI < profileVertices.Length - 1; triangleI++)
                {
                    int i = (triangleI + startTriangleI) * 3;

                    // Add a triangle pointing to the end.
                    outTriangles[i + 0] = sampleI * profileVertices.Length + triangleI;
                    outTriangles[i + 1] = sampleI * profileVertices.Length + triangleI + 1;
                    outTriangles[i + 2] = (sampleI + 1) * profileVertices.Length + triangleI;

                    // Add a triangle pointing to the start.
                    outTriangles[i + 3] = (sampleI + 1) * profileVertices.Length + triangleI;
                    outTriangles[i + 4] = (sampleI + 1) * profileVertices.Length + triangleI + 1;
                    outTriangles[i + 5] = sampleI * profileVertices.Length + triangleI + 1;
                }
            }

            int endCapVertexStartIndex = neededVertexCount - profileVertices.Length;
            int endCapTriangleStartIndex = neededTriangleCount - profileTriangles.Length;
            // Add the end cap.
            for (int i = 0; i < profileTriangles.Length; i++)
            {
                outTriangles[endCapTriangleStartIndex + i] = profileTriangles[i] + endCapVertexStartIndex;
            }

            mesh.Clear();

            mesh.vertices = outVertices;
            mesh.triangles = outTriangles;
            mesh.RecalculateNormals();

            attachment.GetComponent<MeshFilter>().sharedMesh = mesh;
        }

        protected override GameObject OnCurveAdded(Curve curve)
        {
            var attachment = new GameObject("Loft", typeof(MeshFilter), typeof(MeshRenderer));
            attachment.transform.parent = transform;

            updateActions.Enqueue(() => UpdateCurve(curve, attachment));
            return attachment;
        }

        protected override void OnCurveChanged(Curve curve, GameObject attachment)
        {
            updateActions.Enqueue(() => UpdateCurve(curve, attachment));
        }

        protected override void OnCurveRemoved(GameObject attachment)
        {
            Utils.AutoDestroy(attachment);
        }

        private void Update()
        {
            while (updateActions.Count > 0)
                updateActions.Dequeue().Invoke();
        }
    }
}