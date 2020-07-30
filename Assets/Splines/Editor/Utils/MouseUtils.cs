
using UnityEditor;
using UnityEngine;

namespace Splines
{
    static internal class MouseUtils
    {
        /// <summary>
        /// Gets the current scene view camera.
        /// </summary>
        public static Camera GetCurrentCamera() => SceneView.currentDrawingSceneView.camera;

        /// <summary>
        /// Gets the ray corresponding to the current mouse position.
        /// </summary>
        public static Ray GetMouseRay() => HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        /// <summary>
        /// Searches for a raycast hit with the provided ray or gives the point at the provided noHitDepth if no hit is detected.
        /// </summary>
        public static void GetRayPointOrHit(Ray ray, float noHitDepth, out Vector3 position, out Vector3 normal)
        {
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                position = hit.point;
                normal = hit.normal;
            }
            else
            {
                position = ray.GetPoint(noHitDepth);
                normal = Vector3.up;
            }
        }

        /// <summary>
        /// Gets the average depth of a spline along a ray or the provided defaultDepth if the spline has no nodes.
        /// </summary>
        public static float GetAverageDepth(Spline spline, Ray ray, float defaultDepth)
        {
            if (spline.Nodes.Count != 0)
            {
                Vector3 nodeAverage = spline.Nodes.Average((item) => item.Position);
                return Vector3.Dot(nodeAverage - GetCurrentCamera().transform.position, ray.direction);
            }
            else
                return defaultDepth;
        }

        /// <summary>
        /// Gets the current mouse position in the world using the average spline position, the depth of the first raycast hit or 
        /// the defaultDepth as a distance along the mouse ray (See <see cref="GetRayPosition(Ray, float)>"/>.
        /// </summary>
        public static void GetWorldMousePosition(Spline spline, out Vector3 position, out Vector3 normal, 
            float defaultDepth = 10f, float minDepth = 4f)
        {
            Ray ray = GetMouseRay();
            float defaultMouseRayDepth = Mathf.Max(GetAverageDepth(spline, ray, defaultDepth), minDepth);
            GetRayPointOrHit(ray, defaultMouseRayDepth, out position, out normal);
        }
    }
}
