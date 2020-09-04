using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;

namespace Splines.Deform
{
    class TerrainLeveler : CurveMonitor
    {
        [SerializeField]
        private Terrain targetTerrain = null;
        public Terrain TargetTerrain {
            get { return targetTerrain; }
            set { targetTerrain = value; }
        }

        [SerializeField]
        private int flattenRadius = 50;
        public int FlattenRadius
        {
            get { return flattenRadius; }
            set { flattenRadius = value; RefreshAll(); }
        }

        [SerializeField]
        private int smoothRadius = 100;
        public int SmoothRadius
        {
            get { return smoothRadius; }
            set { smoothRadius = value; RefreshAll(); }
        }

        [SerializeField]
        private float startDistance = 0f;
        public float StartDistance
        {
            get { return startDistance; }
            set { startDistance = value; RefreshAll(); }
        }

        [SerializeField]
        private float endDistance = 0f;
        public float EndDistance
        {
            get { return endDistance; }
            set { endDistance = value; RefreshAll(); }
        }

        private Material levelerMaterial = null;

        public override void OnEnable()
        {
            if (levelerMaterial == null)
                levelerMaterial = new Material(Shader.Find("Splines/TerrainLeveler"));

            TerrainCallbacks.heightmapChanged += OnHeightmapChanged;
            OnSplineChanged(null, Spline);
        }

        protected override void OnSplineCleared(object sender, EventArgs e) { }

        protected override void OnSplineCurveAdded(object sender, ListModifiedEventArgs<Curve> e) { }

        protected override void OnSplineCurveChanged(object sender, EventArgs e)
        {
            var curve = sender as Curve;
            if (IsCurveInRange(curve))
                Refresh(curve);
        }

        protected override void OnSplineCurveInserted(object sender, ListModifiedEventArgs<Curve> e)
        {
            if (IsCurveInRange(e.Item))
                Refresh(e.Item);
        }

        protected override void OnSplineCurveRemoved(object sender, ListModifiedEventArgs<Curve> e) { }

        protected override void OnSplineCurveReplaced(object sender, ListItemReplacedEventArgs<Curve> e)
        {
            if (IsCurveInRange(e.NewItem))
                Refresh(e.NewItem);
        }

        private void OnHeightmapChanged(Terrain terrain, RectInt heightRegion, bool synched)
        {
            RefreshAll();
        }

        private bool IsCurveInRange(Curve curve)
        {
            float curveStartDistance = Spline.GetDistanceOfNode(curve.Start);
            float curveEndDistance = curveStartDistance + curve.Length;

            return startDistance <= curveEndDistance && endDistance >= curveStartDistance;
        }

        private void RefreshAll()
        {
            foreach (var curve in Spline.Curves)
                Refresh(curve);
        }

        private void Refresh(Curve curve)
        {
            Rect curveTerrainBounds = TerrainUtils.WorldToTerrainSpace(targetTerrain, TerrainUtils.BoundsToRect(curve.Bounds));
            // Expand the bounds by radius in all directions
            int maxRadius = Math.Max(flattenRadius, smoothRadius);

            PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(targetTerrain, curveTerrainBounds, maxRadius);
            Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, levelerMaterial, -1);
            TerrainPaintUtility.EndPaintHeightmap(paintContext, "Spline - Terrain Leveler");
        }
    }
}
