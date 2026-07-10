using UnityEngine;

namespace ROS2Unity
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class DifferentialDriveController : MonoBehaviour
    {
        [SerializeField] private float commandTimeoutSeconds = 0.5f;

        private Rigidbody body;
        private float linearVelocity;
        private float angularVelocity;
        private float lastCommandTime;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            lastCommandTime = float.NegativeInfinity;
        }

        public void SetCommand(float linearX, float angularZ)
        {
            linearVelocity = linearX;
            angularVelocity = angularZ;
            lastCommandTime = Time.time;
        }

        private void FixedUpdate()
        {
            if (Time.time - lastCommandTime > commandTimeoutSeconds)
            {
                linearVelocity = 0f;
                angularVelocity = 0f;
            }

            Vector3 nextPosition = body.position
                + transform.forward * linearVelocity * Time.fixedDeltaTime;
            Quaternion nextRotation = body.rotation
                * Quaternion.Euler(0f, angularVelocity * Mathf.Rad2Deg * Time.fixedDeltaTime, 0f);

            body.MovePosition(nextPosition);
            body.MoveRotation(nextRotation);
        }
    }
}

