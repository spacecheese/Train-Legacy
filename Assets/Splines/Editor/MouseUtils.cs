using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.HighDefinition.Attributes;

namespace Splines
{
    static class MouseUtils
    {
        /// <summary>
        /// Gets the current scene view camera.
        /// </summary>
        /// <returns></returns>
        public static Camera GetCurrentCamera() => SceneView.currentDrawingSceneView.camera;

        /// <summary>
        /// Gets the world position of the mouse in the current SceneView.
        /// </summary>
        /// <param name="spline">
        /// The spline used to calculate the depth of the mouse when there are no objects below it.
        /// </param>
        public static Vector3 GetWorldMousePosition(Spline spline)
        {
            var camera = GetCurrentCamera();
            var mousePoint = new Vector3(Event.current.mousePosition.x,
                camera.pixelHeight - Event.current.mousePosition.y, 10f);
            var ray = camera.ViewportPointToRay(mousePoint);

            if (Physics.Raycast(ray, out RaycastHit hit))
                return hit.point;
            else
            {
                if (spline.Nodes.Count != 0)
                {
                    Vector3 nodeAverage = spline.Nodes.Average((item) => item.Position);
                    mousePoint.z = Vector3.Dot(nodeAverage - GetCurrentCamera().transform.position, ray.direction);
                }

                return camera.ScreenToWorldPoint(mousePoint);
            }
        }
    }
}
