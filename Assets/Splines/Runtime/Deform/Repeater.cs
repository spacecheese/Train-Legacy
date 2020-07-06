using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Splines.Deform
{
    [ExecuteAlways]
    public class Repeater : CurveAttacher<List<GameObject>>
    {
        [SerializeField]
        private float targetRepeatDistance = 10f;
        [SerializeField]
        private GameObject repeatObject = null;

        private const float MIN_REPEAT_DISTANCE = 0.2f;

        private int GetRepeatCount(Curve curve)
        {
            if (spline == null ||
                targetRepeatDistance < MIN_REPEAT_DISTANCE)
                return 0;

            return Mathf.CeilToInt(curve.Length / targetRepeatDistance);
        }

        private float GetRepeatGap(Curve curve)
        {
            return GetRepeatCount(curve) == 0 ? 0 : curve.Length / GetRepeatCount(curve);
        }

        private void MoveOrCreateRepeats(Curve curve, ref List<GameObject> objects)
        {
            for (int i = 0; i < GetRepeatCount(curve); i++)
            {
                float distance = GetRepeatGap(curve) * i;

                curve.GetTransformAtDistance(distance, out Vector3 position, out Quaternion rotation);

                if (objects.Count > i)
                {
                    if (objects[i] == null)
                        objects[i] = Instantiate(repeatObject, position, rotation, gameObject.transform);
                    else
                    {
                        objects[i].transform.position = position;
                        objects[i].transform.rotation = rotation;
                    }
                }
                else
                    objects.Add(Instantiate(repeatObject, position, rotation, gameObject.transform));
            }

            if (GetRepeatCount(curve) > objects.Count)
            {
                for (int i = GetRepeatCount(curve); i < objects.Count; i++)
                    Utils.AutoDestroy(objects[i]);
            }
        }

        protected override List<GameObject> CurveAdded(Curve curve)
        {
            var objects = new List<GameObject>(GetRepeatCount(curve));
            
            if (repeatObject == null || spline == null) 
                return objects;

            MoveOrCreateRepeats(curve, ref objects);

            return objects;
        }

        protected override void CurveChanged(Curve curve, ref List<GameObject> attachment)
        {
            if (repeatObject == null || spline == null) return;

            MoveOrCreateRepeats(curve, ref attachment);
        }

        protected override void CurveRemoved(List<GameObject> attachment)
        {
            foreach (var go in attachment)
            {
                if (go != null)
                    Utils.AutoDestroy(go);
            }
        }
    }
}