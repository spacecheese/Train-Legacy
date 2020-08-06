using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Splines.Deform
{
    public class Bender : CurveAttacher<MaterialPropertyBlock>
    {
        [SerializeField]
        private Material bendingMaterial;
        public Material BendingMaterial
        {
            get { return bendingMaterial; }
            set { bendingMaterial = value; }
        }

        [SerializeField]
        private Mesh bendingMesh;
        public Mesh BendingMesh
        {
            get { return bendingMesh; }
            set { bendingMesh = value; OnMeshChanged(); }
        }

        private int startId, startAngleId, startHandleId;
        private int endId, endAngleId, endHandleId;

        private void OnMeshChanged()
        {
            foreach (var attachment in Attachments)
                UpdateMeshProperties(attachment);
        }

        private void UpdateCurveProperties(Curve curve, MaterialPropertyBlock block)
        {
            block.SetVector(startId, curve.Start.Position);
            block.SetFloat(startAngleId, 0);
            block.SetVector(startHandleId, curve.Start.AfterHandle.GetValueOrDefault());

            block.SetVector(endId, curve.End.Position);
            block.SetFloat(endAngleId, 0);
            block.SetVector(endHandleId, curve.End.BeforeHandle.GetValueOrDefault());
        }

        private void UpdateMeshProperties(MaterialPropertyBlock block)
        {
            if (bendingMesh == null)
                return;

            block.SetFloat("_MinZ", bendingMesh.bounds.min.z);
            block.SetFloat("_ZLength", bendingMesh.bounds.size.z);
        }

        public void Start()
        {
            startId = Shader.PropertyToID("_Start");
            startAngleId = Shader.PropertyToID("_StartAngle");
            startHandleId = Shader.PropertyToID("_StartHandle");

            endId = Shader.PropertyToID("_End");
            endAngleId = Shader.PropertyToID("_EndAngle");
            endHandleId = Shader.PropertyToID("_EndHandle");
        }

        protected override MaterialPropertyBlock OnBeforeAttachmentAdded(Curve curve)
        {
            var block = new MaterialPropertyBlock();
            UpdateCurveProperties(curve, block);
            UpdateMeshProperties(block);
            return block;
        }

        protected override void OnAttachmentChange(Curve curve, MaterialPropertyBlock attachment)
        {
            UpdateCurveProperties(curve, attachment);
        }

        protected override void OnBeforeAttachmentRemoved(MaterialPropertyBlock attachment)
        {
            
        }

        public void Update()
        {
            // Draw a mesh for each curve.
            foreach (var attachment in Attachments)
                Graphics.DrawMesh(bendingMesh, Matrix4x4.identity,
                    bendingMaterial, 0, null, 0, attachment);
        }
    }

}
