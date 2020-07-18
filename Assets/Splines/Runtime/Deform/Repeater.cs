using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Splines.Deform
{
    [ExecuteAlways]
    [Serializable]
    [CanEditMultipleObjects]
    public class Repeater : CurveAttacher<List<GameObject>>
    {
        [SerializeField]
        private float targetRepeatDistance = 10f;
        public float TargetRepeatDistance
        {

            get => targetRepeatDistance;
            set { targetRepeatDistance = value; OnFieldChanged(); }
        }

        [SerializeField]
        private GameObject repeatObject = null;
        public GameObject RepeatObject {
            get => repeatObject;
            set { repeatObject = value; OnFieldChanged(); }
        }

        [SerializeField]
        private bool padEnds = true;
        public bool PadEnds
        {
            get => padEnds;
            set { padEnds = value; OnFieldChanged(); }
        }

        private const int MAX_REPEAT_COUNT = 10000;

        private readonly List<Action> updateActions = new List<Action>();

        private int GetRepeatCount(Curve curve, bool includePadding)
        {
            int repeatCount = Mathf.Min(Mathf.RoundToInt(curve.Length / TargetRepeatDistance), MAX_REPEAT_COUNT);

            if (includePadding && !padEnds)
                repeatCount++;

            return repeatCount;
        }
        private float GetRepeatGap(Curve curve) => curve.Length / GetRepeatCount(curve, false);
        private void GetRepeatTransform(Curve curve, int repeatIndex, out Vector3 position, out Quaternion rotation)
        {
            float repeatGap = GetRepeatGap(curve);
            float curvePosition = repeatIndex * repeatGap + (PadEnds ? repeatGap / 2 : 0);
            curvePosition = Mathf.Min(curvePosition, Spline.Length);

            curve.GetTransformAtDistance(curvePosition, out position, out rotation);
        }

        private void UpdateCurve(Curve curve, List<GameObject> gameObjects)
        {
            int repeatCount = GetRepeatCount(curve, true);

            if (gameObjects.Count > repeatCount)
                gameObjects.RemoveRange(gameObjects.Count - repeatCount, repeatCount);

            for (int i = 0; i < repeatCount; i++)
            {
                GameObject go = Instantiate(RepeatObject);

                GetRepeatTransform(curve, i, out Vector3 position, out Quaternion rotation);
                go.transform.parent = transform;
                go.transform.position = position;
                go.transform.rotation = rotation;

                if (gameObjects.Count > i)
                    gameObjects[i] = go;
                else
                    gameObjects.Add(go);
            }
        }

        protected override List<GameObject> OnCurveAdded(Curve curve)
        {
            if (RepeatObject == null)
                return null;

            var list = new List<GameObject>(GetRepeatCount(curve, true));
            updateActions.Add(() => UpdateCurve(curve, list));
            return list;
        }

        protected override void OnCurveChanged(Curve curve, List<GameObject> attachment)
        {
            var list = attachment ?? new List<GameObject>();
            updateActions.Add(() => UpdateCurve(curve, list));
        }

        protected override void OnCurveRemoved(List<GameObject> attachment)
        {
            if (attachment == null)
                return;

            foreach (var go in attachment)
                Utils.AutoDestroy(go);
        }

        protected override void OnFieldChanged()
        {
            updateActions.Clear();
            base.OnFieldChanged();
        }

        private void Update()
        {
            foreach (var action in updateActions)
                action.Invoke();
        }

        private void OnDisable()
        {
            foreach (GameObject child in transform)
                Utils.AutoDestroy(child as GameObject);
        }
    }
}