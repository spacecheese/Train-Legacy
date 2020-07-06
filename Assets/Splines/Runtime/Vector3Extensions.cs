
using UnityEngine;

namespace Splines
{
    public static class Vector3Extensions
    {
        /// <summary>
        /// Rotate the vector about a center point based on the difference between from and to.
        /// </summary>
        public static Vector3 RotateAbout(this Vector3 point, Vector3 center, Quaternion from, Quaternion to)
        {
            Quaternion deltaRotation = to * Quaternion.Inverse(from);
            return RotateAbout(point, center, deltaRotation);
        }

        /// <summary>
        /// Rotate the vector about a center point using a provided rotation.
        /// </summary>
        public static Vector3 RotateAbout(this Vector3 point, Vector3 center, Quaternion deltaRotation)
        {
            return (deltaRotation * (point - center)) + center;
        }

        /// <summary>
        /// Checks if two points are within a cuboidal range of one another.
        /// </summary>
        public static bool IsWithinRange(this Vector3 point, Vector3 other, float range)
        {
            return
                (point.x - other.x) < range &&
                (point.y - other.y) < range &&
                (point.z - other.z) < range;
        }
    }
}