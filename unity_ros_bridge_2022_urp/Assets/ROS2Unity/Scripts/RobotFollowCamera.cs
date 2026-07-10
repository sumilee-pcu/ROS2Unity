using UnityEngine;

namespace ROS2Unity
{
    [DisallowMultipleComponent]
    public sealed class RobotFollowCamera : MonoBehaviour
    {
        public Transform Target;
        public Vector3 LocalOffset = new Vector3(0.95f, 0.58f, -1.20f);
        public Vector3 LookOffset = new Vector3(0f, 0.12f, -0.02f);
        public float PositionSmoothTime = 0.16f;
        public float RotationSharpness = 9f;

        private Vector3 velocity;

        public void SnapToTarget()
        {
            if (Target == null)
            {
                return;
            }

            transform.position = Target.TransformPoint(LocalOffset);
            transform.rotation = CalculateLookRotation();
            velocity = Vector3.zero;
        }

        private void LateUpdate()
        {
            if (Target == null)
            {
                return;
            }

            Vector3 desiredPosition = Target.TransformPoint(LocalOffset);
            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref velocity,
                PositionSmoothTime);

            float blend = 1f - Mathf.Exp(-RotationSharpness * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                CalculateLookRotation(),
                blend);
        }

        private Quaternion CalculateLookRotation()
        {
            Vector3 lookDirection = Target.TransformPoint(LookOffset) - transform.position;
            return Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
        }
    }
}
