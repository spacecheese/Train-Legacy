using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Splines.Deform
{
    [ExecuteAlways]
    [Serializable]
    public class Repeater : CurveAttacher<List<GameObject>>
    {
        [SerializeField]
        private float targetRepeatDistance = 10f;
        public float TargetRepeatDistance
        {
            get => targetRepeatDistance;
            set { targetRepeatDistance = value; Refresh(); }
        }

        [SerializeField]
        private GameObject repeatObject = null;
        public GameObject RepeatObject
        {
            get => repeatObject;
            set { repeatObject = value; Refresh(); }
        }

        public enum Justify
        {
            /// <summary>
            /// No padding.
            /// </summary>
            None = 0,
            /// <summary>
            /// Padding at the end.
            /// </summary>
            Left,
            /// <summary>
            /// Padding at the start and end.
            /// </summary>
            Center,
            /// <summary>
            /// Padding at the start.
            /// </summary>
            Right
        }

        [SerializeField]
        private Justify padding;
        /// <summary>
        /// Determines if and where padding should be inserted when repeating. <seealso cref="Justify"/>
        /// </summary>
        public Justify Padding
        {
            get => padding;
            set { padding = value; Refresh(); }
        }

        private const int MAX_REPEAT_COUNT = 10000;
        private readonly Queue<Action> updateActions = new Queue<Action>();

        private int GetIntervalCount(Curve curve) => 
            Mathf.RoundToInt(curve.Length / targetRepeatDistance);

        private int GetRepeatCount(Curve curve)
        {
            int repeatCount = GetIntervalCount(curve);
            // Add an extra repeat to the last curve.
            if (padding == Justify.None &&
                curve == Spline.Curves.Last())
                repeatCount++;

            return repeatCount;
        }

        private float GetRepeatGap(Curve curve) => 
            curve.Length / GetIntervalCount(curve);

        private void DestroyRange(List<GameObject> gameObjects, int index, int count)
        {
            for (int i = index; i < index + count && i < gameObjects.Count; i++)
                if (gameObjects[i] != null)
                    Utils.AutoDestroy(gameObjects[i]);

            gameObjects.RemoveRange(index, count);
        }

        private void UpdateCurve(Curve curve, List<GameObject> gameObjects)
        {
            if (repeatObject == null)
                return;

            int repeatCount = GetRepeatCount(curve);

            if (gameObjects.Count > repeatCount)
                DestroyRange(gameObjects, repeatCount, gameObjects.Count - repeatCount);

            float repeatGap = GetRepeatGap(curve);
            float curvePosition;

            switch (padding)
            {
                default:
                case Justify.None:
                case Justify.Left:
                    curvePosition = 0; break;
                case Justify.Right:
                    curvePosition = repeatGap; break;
                case Justify.Center:
                    curvePosition = repeatGap / 2; break;
            }

            for (int i = 0; i < repeatCount; i++)
            {
                GameObject go;
                if (gameObjects.Count > i &&
                    gameObjects[i] != null)
                    go = gameObjects[i];
                else
                    go = Instantiate(repeatObject);

                curve.GetTransformAtDistance(curvePosition, out Vector3 position, out Quaternion rotation);
                go.transform.parent = transform;
                go.transform.position = position;
                go.transform.rotation = rotation;

                if (gameObjects.Count > i)
                    gameObjects[i] = go;
                else
                    gameObjects.Add(go);

                curvePosition = Mathf.Min(curvePosition + repeatGap, curve.Length);
            }
        }

        protected override List<GameObject> OnCurveAdded(Curve curve)
        {
            var list = new List<GameObject>(GetRepeatCount(curve));
            updateActions.Enqueue(() => UpdateCurve(curve, list));
            return list;
        }

        protected override void OnCurveChanged(Curve curve, List<GameObject> attachment)
        {
            updateActions.Enqueue(() => UpdateCurve(curve, attachment));
        }

        protected override void OnCurveRemoved(List<GameObject> attachment)
        {
            foreach (var go in attachment)
                Utils.AutoDestroy(go);
        }

        private void Update()
        {
            while (updateActions.Count > 0)
                updateActions.Dequeue().Invoke();
        }
    }
}