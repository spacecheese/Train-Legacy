using System;
using System.Collections.Generic;
using UnityEditor;
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

        [SerializeField]
        private bool padEnds = true;
        public bool PadEnds
        {
            get => padEnds;
            set { padEnds = value; Refresh(); }
        }

        private const int MAX_REPEAT_COUNT = 10000;
        private readonly Queue<Action> updateActions = new Queue<Action>();

        private int GetRepeatCount(Curve curve, bool includePadding = true)
        {
            int repeatCount = Mathf.Min(Mathf.RoundToInt(curve.Length / targetRepeatDistance), MAX_REPEAT_COUNT);

            if (includePadding && !padEnds)
                repeatCount++;
            return repeatCount;
        }

        private float GetRepeatGap(Curve curve) => 
            curve.Length / GetRepeatCount(curve, false);

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
            float curvePosition = padEnds ? repeatGap / 2 : 0;

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
            if (attachment == null)
                return;

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