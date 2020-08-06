using UnityEngine;

namespace Splines
{
    [RequireComponent(typeof(Rigidbody))]
    public class SplineFollower : MonoBehaviour
    {
        [SerializeField]
        Spline spline;

        [SerializeField]
        float distance = 0;

        float curveVelocity = 0;
        Rigidbody body;

        private void Start()
        {
            body = GetComponent<Rigidbody>();
            body.freezeRotation = true;
        }

        private void FixedUpdate()
        {
            if (spline == null)
                return;

            Vector3 curveTangent = spline.GetTangentAtDistance(distance).normalized;
            Quaternion rotation = spline.GetRotationAtDistance(distance);

            curveVelocity = Vector3.Dot(body.velocity, curveTangent);
            // Only keep the portion of the body velocity acting along the spline.
            body.velocity = curveVelocity * curveTangent;

            transform.position = spline.GetPositionAtDistance(distance);
            transform.rotation = rotation;

            // Move along the curve at the curve velocity.
            distance += curveVelocity * Time.deltaTime;

            // Stop the follower from leaving the spline.
            if (distance < 0)
            {
                distance = 0;
                body.velocity = -body.velocity;
            }
            else if (distance > spline.Length)
            {
                distance = spline.Length;
                body.velocity = -body.velocity;
            }
        }
    }
}
