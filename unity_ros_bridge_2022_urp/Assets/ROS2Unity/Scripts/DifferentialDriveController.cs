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
        private bool runtimeMotionLogged;

        public bool HasReceivedCommand { get; private set; }
        public float DistanceMoved { get; private set; }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            lastCommandTime = float.NegativeInfinity;
        }

        public void SetCommand(float linearX, float angularZ)
        {
            linearVelocity = linearX;
            angularVelocity = angularZ;
            lastCommandTime = Time.unscaledTime;
            HasReceivedCommand = true;
        }

        private void FixedUpdate()
        {
            if (Time.unscaledTime - lastCommandTime > commandTimeoutSeconds)
            {
                linearVelocity = 0f;
                angularVelocity = 0f;
            }

            Vector3 nextPosition = body.position
                + transform.forward * linearVelocity * Time.fixedDeltaTime;
            Quaternion nextRotation = body.rotation
                * Quaternion.Euler(0f, angularVelocity * Mathf.Rad2Deg * Time.fixedDeltaTime, 0f);

            DistanceMoved += Vector3.Distance(body.position, nextPosition);
            body.MovePosition(nextPosition);
            body.MoveRotation(nextRotation);

            if (!runtimeMotionLogged && DistanceMoved >= 0.02f)
            {
                Debug.Log("ROS2UNITY_RUNTIME_MOTION_OK: distance="
                    + DistanceMoved.ToString("F3") + "m");
                runtimeMotionLogged = true;
            }
        }
    }
}
