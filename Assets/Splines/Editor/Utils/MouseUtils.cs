
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
        public static Vector3 GetRayPosition(Ray ray, float noHitDepth) => Physics.Raycast(ray, out RaycastHit hit) ? hit.point : ray.GetPoint(noHitDepth);

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
        /// Gets the current mouse position in the world using the average spline position, the depth of the first raycast hit or the defaultDepth as a distance along 
        /// the mouse ray (See <see cref="GetRayPosition(Ray, float)>"/>.
        /// </summary>
        public static Vector3 GetWorldMousePosition(Spline spline, float defaultDepth = 10f)
        {
            Ray ray = GetMouseRay();
            return GetRayPosition(ray, GetAverageDepth(spline, ray, defaultDepth));
        }
    }
}
