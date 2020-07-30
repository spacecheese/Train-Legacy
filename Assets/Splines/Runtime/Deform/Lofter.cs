using System;
using System.Collections.Generic;
using UnityEngine;

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
            set { profile = value; Refresh(); }
        }

        private Material loftMaterial;
        public Material LoftMaterial
        {
            get => loftMaterial;
            set { loftMaterial = value; UpdateMaterials(value); }
        }

        private readonly Queue<Action> updateActions = new Queue<Action>();

        private void UpdateMaterials(Material newMaterial)
        {
            foreach (var attach in Attachments) 
                if (attach != null)
                    attach.GetComponent<MeshRenderer>().material = loftMaterial;
        }

        private void UpdateCurve(Curve curve, GameObject attachment)
        {
            if (profile == null)
                return;

            if (attachment == null)
                attachment = CreateAttachment();

            Mesh mesh = attachment.GetComponent<MeshFilter>().sharedMesh;

            if (mesh == null)
            {
                mesh = new Mesh();
                attachment.GetComponent<MeshFilter>().sharedMesh = mesh;
            }
            else
                mesh.Clear();

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
                    outVertices[startVertI + vertI] = samples[sampleI].GetRotation(curve) * profileVertices[vertI] + samples[sampleI].Position;
            }

            // Add the start cap.
            for (int i = 0; i < profileTriangles.Length; i++)
                // Reverse the winding order on the start cap.
                outTriangles[i] = profileTriangles[profileTriangles.Length - 1 - i];

            for (int loopI = 0; loopI < samples.Count - 1; loopI++)
            {
                int startVertexI = loopI * profileVertices.Length * 6 + profileTriangles.Length;
                for (int vertexI = 0; vertexI < profileVertices.Length; vertexI++)
                {
                    int i = vertexI * 6 + startVertexI;

                    // Wrap around to 0 on the last vertex.
                    int nextVertexI = vertexI < profileVertices.Length - 1 ? vertexI + 1 : 0;

                    // Add a triangle pointing to the end.
                    outTriangles[i + 0] = loopI * profileVertices.Length + vertexI;
                    outTriangles[i + 1] = loopI * profileVertices.Length + nextVertexI;
                    outTriangles[i + 2] = (loopI + 1) * profileVertices.Length + vertexI;

                    // Add a triangle pointing to the start.
                    outTriangles[i + 3] = loopI * profileVertices.Length + nextVertexI;
                    outTriangles[i + 4] = (loopI + 1) * profileVertices.Length + nextVertexI;
                    outTriangles[i + 5] = (loopI + 1) * profileVertices.Length + vertexI;
                }
            }

            int endCapVertexStartIndex = neededVertexCount - profileVertices.Length;
            int endCapTriangleStartIndex = neededTriangleCount * 3 - profileTriangles.Length;
            // Add the end cap.
            for (int i = 0; i < profileTriangles.Length; i++)
                outTriangles[endCapTriangleStartIndex + i] = profileTriangles[i] + endCapVertexStartIndex;

            mesh.vertices = outVertices;
            mesh.triangles = outTriangles;
            mesh.RecalculateNormals();
        }

        private GameObject CreateAttachment()
        {
            var attachment = new GameObject("Loft", typeof(MeshFilter), typeof(MeshRenderer));
            attachment.transform.parent = transform;
            attachment.GetComponent<MeshRenderer>().material = LoftMaterial;
            return attachment;
        }

        protected override GameObject OnBeforeAttachmentAdded(Curve curve)
        {
            var attachment = CreateAttachment();
            updateActions.Enqueue(() => UpdateCurve(curve, attachment));
            return attachment;
        }

        protected override void OnAttachmentChange(Curve curve, GameObject attachment)
        {
            updateActions.Enqueue(() => UpdateCurve(curve, attachment));
        }

        protected override void OnBeforeAttachmentRemoved(GameObject attachment)
        {
            Utils.AutoDestroy(attachment);
        }

        private void Update()
        {
            while (updateActions.Count > 0)
                updateActions.Dequeue().Invoke();
        }
        // Something   something something
    }
}