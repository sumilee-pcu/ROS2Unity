#if ROSSHARP_PRESENT
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.MessageTypes.Geometry;
using UnityEngine;

namespace ROS2Unity
{
    public sealed class RosSharpTwistSubscriber : UnitySubscriber<Twist>
    {
        [SerializeField] private DifferentialDriveController controller;

        protected override void Start()
        {
            base.Start();
            if (controller == null)
            {
                controller = GetComponent<DifferentialDriveController>();
            }
        }

        protected override void ReceiveMessage(Twist message)
        {
            controller.SetCommand((float)message.linear.x, (float)message.angular.z);
        }
    }
}
#endif

